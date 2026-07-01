using System.Text;
using FlowBlox.AIAssistant.Builder;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Util.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Services
{
    public class AiAssistantService
    {
        private readonly IAiExecutor _executor;
        private readonly IFlowBloxAIToolApi _tools;
        private readonly ILogger? _logger;
        private readonly Func<AssistantConfiguration>? _configurationProvider;
        private readonly StringComparer _nameComparer = StringComparer.OrdinalIgnoreCase;
        private readonly object _sessionSync = new();
        private int _activeRunCount;
        private AssistantSessionState? _session;

        public event EventHandler<FlowBlocksChangedEventArgs>? FlowBlocksChanged;
        public event EventHandler<FlowBlocksConnectionsChangedEventArgs>? FlowBlocksConnectionsChanged;
        public event EventHandler<AssistantTranscriptLine>? TranscriptLineAdded;

        public AiAssistantService(
            IAiExecutor executor,
            IFlowBloxAIToolApi tools,
            ILogger? logger = null,
            Func<AssistantConfiguration>? configurationProvider = null)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _logger = logger;
            _configurationProvider = configurationProvider;
            _tools.FlowBlocksConnectionsChanged += Tools_FlowBlocksConnectionsChanged;
        }

        public void ResetSession()
        {
            lock (_sessionSync)
            {
                if (_session != null)
                    ToolHandlerUtilities.ClearSessionCache(_session.SessionId);

                _session = null;
            }
        }

        public AssistantConfiguration GetConfiguration(out string error)
        {
            error = string.Empty;
            if (_configurationProvider != null)
                return _configurationProvider() ?? new AssistantConfiguration();

            var rawConfig = FlowBloxOptions.GetOptionInstance().GetOption("AI.AssistantConfiguration")?.Value ?? string.Empty;
            var parseResult = AssistantConfigurationJson.Parse(rawConfig);
            if (parseResult.HasError)
            {
                error = parseResult.Error;
                return new AssistantConfiguration();
            }

            return parseResult.Configuration ?? new AssistantConfiguration();
        }

        public bool SaveConfiguration(AssistantConfiguration configuration, out string error)
        {
            error = string.Empty;
            try
            {
                var serialized = AssistantConfigurationJson.Serialize(configuration ?? new AssistantConfiguration());
                var options = FlowBloxOptions.GetOptionInstance();
                var option = options.GetOption("AI.AssistantConfiguration");
                if (option == null)
                {
                    error = $"Option 'AI.AssistantConfiguration' not found.";
                    return false;
                }

                option.Value = serialized;
                options.Save();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static AssistantTokenBudget BuildTokenBudget(AssistantConfiguration config)
        {
            return new AssistantTokenBudget
            {
                MaxContextTokens = Math.Max(0, config?.MaxContextTokens ?? 0),
                ReservedResponseTokens = Math.Max(0, config?.ReservedResponseTokens ?? 0),
                ApproximateCharactersPerToken = Math.Clamp(config?.ApproximateCharactersPerToken ?? 4, 1, 20)
            };
        }

        public async Task<AssistantResult> GenerateProjectAsync(string userPrompt, CancellationToken ct)
        {
            var result = new AssistantResult();
            AddTranscript(result, AssistantTranscriptKind.Status, "Running...");

            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                result.Success = false;
                var message = "Prompt is empty.";
                result.Errors.Add(message);
                AddTranscript(result, AssistantTranscriptKind.Error, message);
                return result;
            }

            var config = GetConfiguration(out var configurationError);
            if (!string.IsNullOrWhiteSpace(configurationError))
            {
                result.Success = false;
                result.Errors.Add(configurationError);
                AddTranscript(result, AssistantTranscriptKind.Error, configurationError);
                return result;
            }

            var maxRounds = Math.Clamp(config.MaxToolRounds, 1, 200);
            var maxLatestMessages = Math.Clamp(config.MaxLatestMessages, 0, 50);
            var tokenBudget = BuildTokenBudget(config);
            var session = GetOrCreateSession();
            ToolHandlerUtilities.SetCurrentSessionGuid(session.SessionId);
            var isConversationStart = session.Messages.Count == 0;
            var toolDefinitions = _tools.GetToolDefinitions();
            var systemPrompt = AssistantPromptBuilder.BuildSystemPrompt();
            var sessionBootstrapPrompt = AssistantPromptBuilder.BuildSessionBootstrapPrompt(toolDefinitions);
            var latestToolTranscript = new List<string>();
            var knownFlowBlocksByName = CaptureFlowBlocksByName();
            var protocolWriter = TryCreateCommunicationProtocolWriter(config, userPrompt, session.SessionId);
            var formatRetryIssued = false;
            var hasExecutedAnyToolCall = false;
            protocolWriter?.AppendAiAssistantServiceText("System prompt prepared", systemPrompt);
            protocolWriter?.AppendAiAssistantServiceText("Session bootstrap prompt prepared", sessionBootstrapPrompt);

            AddTranscript(result, AssistantTranscriptKind.Status, "Thinking...");

            try
            {
                Interlocked.Increment(ref _activeRunCount);
                for (var round = 1; round <= maxRounds; round++)
                {
                    ct.ThrowIfCancellationRequested();
                    var roundPrompt = AssistantPromptBuilder.BuildRoundPrompt(
                        userPrompt,
                        round == 1 && isConversationStart ? GetCurrentProjectJson() : null,
                        latestToolTranscript,
                        round,
                        maxRounds);
                    protocolWriter?.AppendAiAssistantServiceText($"Round {round} prompt prepared", roundPrompt);

                    var chatRequest = AssistantChatRequestBuilder.Build(
                        systemPrompt,
                        sessionBootstrapPrompt,
                        session.ConversationSummary,
                        session.Messages,
                        roundPrompt,
                        maxLatestMessages,
                        tokenBudget);

                    var exec = await _executor.ExecuteChatAsync(
                        chatRequest,
                        config,
                        ct).ConfigureAwait(false);
                    result.RawModelOutput = exec.RawOutput ?? exec.OutputText ?? string.Empty;

                    if (!exec.Success)
                    {
                        result.Success = false;
                        var error = string.IsNullOrWhiteSpace(exec.Error) ? "AI request failed." : exec.Error;
                        result.Errors.Add(error);
                        AddTranscript(result, AssistantTranscriptKind.Error, error);
                        _logger?.Warn($"AI Assistant execution failed: {error}");
                        return result;
                    }

                    var assistantOutput = exec.OutputText ?? string.Empty;

                    if (!TryParseAssistantInstruction(assistantOutput, out var instruction))
                    {
                        protocolWriter?.AppendAiText(round, assistantOutput);
                        AddTranscript(
                            result,
                            AssistantTranscriptKind.Assistant,
                            assistantOutput,
                            assistantOutput);

                        if (!formatRetryIssued)
                        {
                            formatRetryIssued = true;
                            const string formatGuidance =
                                "FORMAT_VALIDATION: Your previous response did not follow the required JSON schema. " +
                                "For the next response, return exactly one JSON object in this format: " +
                                "{\"assistantMessage\":\"short status or final answer\",\"final\":false,\"toolCalls\":[{\"toolName\":\"ToolName\",\"arguments\":{}}]} " +
                                "Set \"final\" to true only for the final answer. Do not output additional text outside this JSON object.";

                            AddTranscript(result, AssistantTranscriptKind.Status,
                                "Response format invalid. Retrying once.");
                            result.Warnings.Add("Assistant response format invalid; retrying once with explicit format guidance.");
                            protocolWriter?.AppendAiAssistantServiceText("Format validation guidance issued", formatGuidance);
                            latestToolTranscript = [formatGuidance];

                            continue;
                        }

                        result.Success = false;
                        result.Errors.Add("Assistant returned an invalid response format twice. Aborting execution.");
                        AddTranscript(result, AssistantTranscriptKind.Error,
                            "Invalid response format repeated after correction. Aborting.");
                        AppendSessionTurn(session, userPrompt, assistantOutput);
                        await TryUpdateConversationSummaryAsync(session, config, ct).ConfigureAwait(false);
                        return result;
                    }


                    protocolWriter?.AppendAiJson(round, TryParseFirstJsonObject(assistantOutput));

                    var assistantRoundMessage = BuildAssistantRoundMessage(instruction, assistantOutput);
                    AddTranscript(
                        result,
                        AssistantTranscriptKind.Assistant,
                        assistantRoundMessage,
                        instruction.InternalContent);

                    if (instruction.ToolCalls.Count == 0 || instruction.Final)
                    {
                        if (hasExecutedAnyToolCall)
                            RunAutomaticAdjustmentIfEnabled("AI run completed");

                        result.Success = true;
                        result.AssistantText = instruction.AssistantMessage;
                        result.Summary = "Assistant response generated.";

                        var finalText = string.IsNullOrWhiteSpace(instruction.AssistantMessage)
                            ? assistantOutput
                            : instruction.AssistantMessage;
                        AppendSessionTurn(session, userPrompt, finalText);
                        await TryUpdateConversationSummaryAsync(session, config, ct).ConfigureAwait(false);
                        return result;
                    }

                    var roundToolTranscript = new List<string>();
                    var executedToolCalls = new List<ExecutedToolCallInfo>();

                    var processingStatus = BuildToolProcessingTranscript(instruction.ToolCalls);
                    AddTranscript(
                        result,
                        AssistantTranscriptKind.ToolProcessing,
                        "Processing requested operations...",
                        processingStatus);

                    for (var toolCallIndex = 0; toolCallIndex < instruction.ToolCalls.Count; toolCallIndex++)
                    {
                        var toolCall = instruction.ToolCalls[toolCallIndex];
                        ct.ThrowIfCancellationRequested();

                        var request = new ToolRequest
                        {
                            ToolName = toolCall.ToolName,
                            Arguments = toolCall.Arguments,
                            CorrelationId = Guid.NewGuid().ToString("N")
                        };

                        AddTranscript(
                            result,
                            AssistantTranscriptKind.ToolProcessing,
                            $"Processing tool request {toolCallIndex + 1}/{instruction.ToolCalls.Count}: {request.ToolName}",
                            (request.Arguments ?? new JObject()).ToString(Formatting.Indented));

                        var response = await _tools.ExecuteAsync(request, ct).ConfigureAwait(false);
                        hasExecutedAnyToolCall = true;

                        protocolWriter?.AppendToolCall(round, request, response);

                        roundToolTranscript.Add(JsonConvert.SerializeObject(new
                        {
                            tool = request.ToolName,
                            arguments = request.Arguments,
                            response
                        }, Formatting.None));
                        executedToolCalls.Add(new ExecutedToolCallInfo
                        {
                            ToolName = request.ToolName,
                            Arguments = request.Arguments,
                            Response = response
                        });

                        if (!response.Ok)
                        {
                            result.Warnings.Add($"Tool '{request.ToolName}' reported a problem: {response.Error}");
                            _logger?.Warn($"Assistant tool call reported a problem. Tool={request.ToolName}, Error={response.Error}");
                        }

                        knownFlowBlocksByName = NotifyFlowBlockChanges(knownFlowBlocksByName);
                    }

                    latestToolTranscript = roundToolTranscript;

                    var executionSummary = BuildToolExecutionSummary(executedToolCalls);

                    var internalStatus = BuildToolExecutionTranscript(
                        executionSummary.RequestedOperationCount,
                        executionSummary.SuccessfulOperationCount,
                        executionSummary.FailedToolNames,
                        executedToolCalls);

                    var toolExecutionKind = executionSummary.AllSuccessful
                        ? AssistantTranscriptKind.ToolSuccess
                        : executionSummary.AnySuccessful
                            ? AssistantTranscriptKind.ToolPartialSuccess
                            : AssistantTranscriptKind.ToolError;

                    var toolExecutionMessage = BuildToolExecutionMessage(executionSummary);
                    AddTranscript(
                        result,
                        toolExecutionKind,
                        toolExecutionMessage,
                        internalStatus);
                }

                result.Success = false;
                result.Errors.Add($"Assistant reached max tool rounds ({maxRounds}) without a final response.");
                AddTranscript(result, AssistantTranscriptKind.Error,
                    $"Reached max tool rounds ({maxRounds}) without a final response.");
                AppendSessionTurn(session, userPrompt, $"No final response after {maxRounds} rounds.");
                await TryUpdateConversationSummaryAsync(session, config, ct).ConfigureAwait(false);
                return result;
            }
            finally
            {
                Interlocked.Decrement(ref _activeRunCount);
                protocolWriter?.TryWrite(_logger);
            }
        }

        private void Tools_FlowBlocksConnectionsChanged(object? sender, FlowBlocksConnectionsChangedEventArgs e)
        {
            if (e == null || !e.HasChanges)
                return;

            FlowBlocksConnectionsChanged?.Invoke(this, e);

            if (Volatile.Read(ref _activeRunCount) <= 0)
                return;

            RunAutomaticAdjustmentIfEnabled($"AI connect/disconnect (connections={e.Changes.Count})");
        }

        private void RunAutomaticAdjustmentIfEnabled(string reason)
        {
            var configuration = GetConfiguration(out _);
            if (configuration?.EnableAutomaticAdjustment != true)
                return;

            var layoutResult = FlowBlockAutoLayoutAdjuster.AdjustCurrentRegistryLayout();
            _logger?.Info(
                $"AutoAdjustFlowLayout executed ({reason}). Updated={layoutResult.UpdatedFlowBlocks}, Total={layoutResult.TotalFlowBlocks}, Components={layoutResult.ComponentsProcessed}");
        }

        private AiCommunicationProtocolWriter? TryCreateCommunicationProtocolWriter(AssistantConfiguration config, string userPrompt, string sessionId)
        {
            if (config?.EnableCommunicationProtocol != true)
                return null;

            try
            {
                var options = FlowBloxOptions.GetOptionInstance();
                var directory = options
                    .GetOption("AI.CommuncationProtocolDir")?
                    .Value;

                if (string.IsNullOrWhiteSpace(directory))
                {
                    directory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "FlowBlox",
                        "logs",
                        "ai_assistant_protocol");
                }

                return new AiCommunicationProtocolWriter(directory, sessionId, userPrompt);
            }
            catch (Exception ex)
            {
                _logger?.Warn($"Could not initialize communication protocol writer: {ex.Message}");
                return null;
            }
        }

        private AssistantSessionState GetOrCreateSession()
        {
            lock (_sessionSync)
            {
                _session ??= new AssistantSessionState();
                return _session;
            }
        }

        private void AppendSessionTurn(AssistantSessionState session, string userPrompt, string assistantMessage)
        {
            if (session == null)
                return;

            lock (_sessionSync)
            {
                AppendSessionMessage(session, "user", userPrompt);
                AppendSessionMessage(session, "assistant", assistantMessage);
            }
        }

        private static void AppendSessionMessage(AssistantSessionState session, string role, string content)
        {
            if (session == null || string.IsNullOrWhiteSpace(content))
                return;

            session.Messages.Add(new AssistantConversationMessage
            {
                Role = role,
                Content = content.Trim()
            });

            const int maxMessages = 200;
            if (session.Messages.Count > maxMessages)
            {
                var removedMessageCount = session.Messages.Count - maxMessages;
                session.Messages.RemoveRange(0, removedMessageCount);
                session.SummarizedMessageCount = Math.Max(0, session.SummarizedMessageCount - removedMessageCount);
            }
        }

        private async Task TryUpdateConversationSummaryAsync(
            AssistantSessionState session,
            AssistantConfiguration config,
            CancellationToken ct)
        {
            if (session == null || session.Messages.Count == 0)
                return;

            try
            {
                string currentSummary;
                int targetSummarizedMessageCount;
                List<AssistantConversationMessage> messagesToSummarize;

                lock (_sessionSync)
                {
                    var maxLatestMessages = Math.Clamp(config.MaxLatestMessages, 0, 50);
                    targetSummarizedMessageCount = Math.Max(0, session.Messages.Count - maxLatestMessages);
                    if (targetSummarizedMessageCount <= session.SummarizedMessageCount)
                        return;

                    currentSummary = session.ConversationSummary;
                    messagesToSummarize = session.Messages
                        .Skip(session.SummarizedMessageCount)
                        .Take(targetSummarizedMessageCount - session.SummarizedMessageCount)
                        .ToList();
                }

                if (messagesToSummarize.Count == 0)
                    return;

                var summaryRequest = AssistantSummaryRequestBuilder.Build(currentSummary, messagesToSummarize);
                var summaryResult = await _executor.ExecuteChatAsync(summaryRequest, config, ct).ConfigureAwait(false);
                if (!summaryResult.Success || string.IsNullOrWhiteSpace(summaryResult.OutputText))
                {
                    if (!string.IsNullOrWhiteSpace(summaryResult.Error))
                        _logger?.Warn($"AI Assistant summary update failed: {summaryResult.Error}");

                    return;
                }

                lock (_sessionSync)
                {
                    session.ConversationSummary = summaryResult.OutputText.Trim();
                    session.SummarizedMessageCount = Math.Max(session.SummarizedMessageCount, targetSummarizedMessageCount);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.Warn($"AI Assistant summary update failed: {ex.Message}");
            }
        }

        private static string GetCurrentProjectJson()
        {
            var project = FlowBloxProjectManager.Instance.ActiveProject;
            if (project == null)
                return "{}";

            return JsonConvert.SerializeObject(project, JsonSettings.ProjectExportForAiAssistant());
        }

        private Dictionary<string, BaseFlowBlock> CaptureFlowBlocksByName()
        {
            var project = FlowBloxProjectManager.Instance.ActiveProject;
            if (project == null)
                return new Dictionary<string, BaseFlowBlock>(_nameComparer);

            return project.FlowBloxRegistry.GetFlowBlocks()
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name, _nameComparer)
                .ToDictionary(x => x.Key, x => x.First(), _nameComparer);
        }

        private Dictionary<string, BaseFlowBlock> NotifyFlowBlockChanges(Dictionary<string, BaseFlowBlock> knownFlowBlocksByName)
        {
            var currentFlowBlocksByName = CaptureFlowBlocksByName();

            var added = currentFlowBlocksByName
                .Where(x => !knownFlowBlocksByName.ContainsKey(x.Key))
                .Select(x => x.Value)
                .ToList();

            var removed = knownFlowBlocksByName.Keys
                .Where(x => !currentFlowBlocksByName.ContainsKey(x))
                .ToList();

            if (added.Count == 0 && removed.Count == 0)
                return currentFlowBlocksByName;

            FlowBlocksChanged?.Invoke(this, new FlowBlocksChangedEventArgs
            {
                AddedFlowBlocks = added,
                RemovedFlowBlockNames = removed
            });

            return currentFlowBlocksByName;
        }

        private void AddTranscript(AssistantResult result, AssistantTranscriptKind kind, string text, string? internalContent = null)
        {
            var line = new AssistantTranscriptLine
            {
                Timestamp = DateTime.Now,
                Kind = kind,
                Text = text,
                InternalContent = internalContent ?? string.Empty
            };

            result.TranscriptLines.Add(line);
            TranscriptLineAdded?.Invoke(this, line);
        }

        private static bool TryParseAssistantInstruction(string output, out AssistantInstruction instruction)
        {
            instruction = new AssistantInstruction();
            if (string.IsNullOrWhiteSpace(output))
                return false;

            var root = TryParseFirstJsonObject(output);
            if (root == null)
                return false;

            instruction.AssistantMessage = root.Value<string>("assistantMessage")
                ?? root.Value<string>("message")
                ?? root.Value<string>("finalResponse")
                ?? string.Empty;
            instruction.InternalContent = root.ToString(Formatting.Indented);

            instruction.Final = root.Value<bool?>("final") == true;

            if (root["toolCalls"] is JArray toolCalls)
            {
                foreach (var token in toolCalls.OfType<JObject>())
                {
                    var toolName = token.Value<string>("toolName") ?? token.Value<string>("tool") ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(toolName))
                        continue;

                    instruction.ToolCalls.Add(new AssistantToolCall
                    {
                        ToolName = toolName,
                        Arguments = token["arguments"] as JObject ?? new JObject()
                    });
                }
            }
            else if (root["toolName"] != null || root["tool"] != null)
            {
                var toolName = root.Value<string>("toolName") ?? root.Value<string>("tool") ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(toolName))
                {
                    instruction.ToolCalls.Add(new AssistantToolCall
                    {
                        ToolName = toolName,
                        Arguments = root["arguments"] as JObject ?? new JObject()
                    });
                }
            }

            return true;
        }

        private static string BuildToolExecutionTranscript(
            int requestedOperationCount,
            int successfulOperationCount,
            List<string> failedToolNames,
            List<ExecutedToolCallInfo> executedToolCalls)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Requested operations: {requestedOperationCount}");
            sb.AppendLine($"Executed operations: {executedToolCalls?.Count ?? 0}");
            sb.AppendLine($"Successful operations: {successfulOperationCount}");

            var hasAnySuccessful = successfulOperationCount > 0;
            var allSuccessful = requestedOperationCount > 0 && successfulOperationCount == requestedOperationCount;
            var status = allSuccessful
                ? "success"
                : hasAnySuccessful
                    ? "partial success"
                    : "issues detected";
            sb.AppendLine($"Status: {status}");

            if (!allSuccessful && failedToolNames?.Count > 0)
                sb.AppendLine("Failed tools: " + string.Join(", ", failedToolNames.Distinct(StringComparer.OrdinalIgnoreCase)));

            if (executedToolCalls == null || executedToolCalls.Count == 0)
                return sb.ToString().TrimEnd();

            sb.AppendLine();
            sb.AppendLine("Tool API outputs:");

            for (var i = 0; i < executedToolCalls.Count; i++)
            {
                var call = executedToolCalls[i];
                var response = call.Response ?? new ToolResponse();

                sb.AppendLine($"[{i + 1}] {call.ToolName}");
                sb.AppendLine("arguments:");
                sb.AppendLine((call.Arguments ?? new JObject()).ToString(Formatting.Indented));
                sb.AppendLine($"ok: {response.Ok}");

                if (!string.IsNullOrWhiteSpace(response.Error))
                {
                    sb.AppendLine("error:");
                    sb.AppendLine(response.Error);
                }

                if (response.Result != null && response.Result.HasValues)
                {
                    sb.AppendLine("result:");
                    sb.AppendLine(response.Result.ToString(Formatting.Indented));
                }

                if (response.Log != null && response.Log.HasValues)
                {
                    sb.AppendLine("log:");
                    sb.AppendLine(response.Log.ToString(Formatting.Indented));
                }

                if (i < executedToolCalls.Count - 1)
                    sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildToolExecutionMessage(ToolExecutionSummary summary)
        {
            if (summary.AnySuccessful && !summary.AllSuccessful)
            {
                return $"{summary.SuccessfulOperationCount}/{summary.RequestedOperationCount} requested operations were executed successfully.";
            }

            if (summary.AllSuccessful)
            {
                if (summary.RequestedOperationCount == 1)
                    return "The requested operation was executed successfully.";

                return $"All {summary.RequestedOperationCount} requested operations were executed successfully.";
            }

            return "There was a problem with the request. A problem report was included.";
        }

        private static ToolExecutionSummary BuildToolExecutionSummary(List<ExecutedToolCallInfo> executedToolCalls)
        {
            var requestedOperationCount = 0;
            var successfulOperationCount = 0;
            var failedToolNames = new List<string>();

            if (executedToolCalls == null || executedToolCalls.Count == 0)
            {
                return new ToolExecutionSummary
                {
                    RequestedOperationCount = 0,
                    SuccessfulOperationCount = 0,
                    FailedToolNames = failedToolNames
                };
            }

            foreach (var executedToolCall in executedToolCalls)
            {
                var response = executedToolCall?.Response ?? new ToolResponse();
                var result = response.Result;

                if (result?["batchResults"] is JArray batchResults)
                {
                    var requestedBatchCount = executedToolCall.Arguments?["requests"] is JArray requests
                        ? requests.Count
                        : batchResults.Count;

                    var successfulBatchCount = batchResults
                        .OfType<JObject>()
                        .Count(x => x.Value<bool?>("ok") == true);

                    requestedOperationCount += requestedBatchCount;
                    successfulOperationCount += successfulBatchCount;

                    if (successfulBatchCount < requestedBatchCount)
                        failedToolNames.Add(executedToolCall.ToolName);

                    continue;
                }

                requestedOperationCount += 1;
                if (response.Ok)
                {
                    successfulOperationCount += 1;
                }
                else
                {
                    failedToolNames.Add(executedToolCall.ToolName);
                }
            }

            return new ToolExecutionSummary
            {
                RequestedOperationCount = requestedOperationCount,
                SuccessfulOperationCount = successfulOperationCount,
                FailedToolNames = failedToolNames
            };
        }

        private static string BuildToolProcessingTranscript(IReadOnlyList<AssistantToolCall> toolCalls)
        {
            var sb = new StringBuilder();
            var calls = toolCalls ?? Array.Empty<AssistantToolCall>();
            sb.AppendLine($"Requested operations: {calls.Count}");

            if (calls.Count == 0)
                return sb.ToString().TrimEnd();

            for (var i = 0; i < calls.Count; i++)
            {
                var call = calls[i];
                sb.AppendLine();
                sb.AppendLine($"[{i + 1}] {call.ToolName}");
                sb.AppendLine("arguments:");
                sb.AppendLine((call.Arguments ?? new JObject()).ToString(Formatting.Indented));
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildAssistantRoundMessage(AssistantInstruction instruction, string assistantOutput)
        {
            if (!string.IsNullOrWhiteSpace(instruction?.AssistantMessage))
                return instruction.AssistantMessage;

            if (instruction?.ToolCalls?.Count > 0)
            {
                var toolNames = instruction.ToolCalls
                    .Select(x => x.ToolName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (toolNames.Count > 0)
                    return $"Executing {instruction.ToolCalls.Count} operation(s): {string.Join(", ", toolNames)}";

                return $"Executing {instruction.ToolCalls.Count} operation(s).";
            }

            if (!string.IsNullOrWhiteSpace(assistantOutput))
                return assistantOutput;

            return "[no assistant message]";
        }

        private static JObject? TryParseFirstJsonObject(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return null;

            try
            {
                if (TextHelper.TrySubstringFromFirstOccurrence(output, '{', out var objectCandidate) &&
                    !string.IsNullOrWhiteSpace(objectCandidate))
                {
                    output = objectCandidate;
                }

                using var stringReader = new StringReader(output);
                using var jsonReader = new JsonTextReader(stringReader)
                {
                    SupportMultipleContent = true
                };

                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType != JsonToken.StartObject)
                        continue;

                    var token = JToken.ReadFrom(jsonReader);
                    if (token is JObject obj)
                        return obj;

                    return null;
                }
            }
            catch
            {
                // ignored - caller handles parse failure
            }

            return null;
        }

        private sealed class AssistantInstruction
        {
            public string AssistantMessage { get; set; } = string.Empty;
            public string InternalContent { get; set; } = string.Empty;
            public bool Final { get; set; }
            public List<AssistantToolCall> ToolCalls { get; set; } = new();
        }

        private sealed class AssistantToolCall
        {
            public string ToolName { get; set; } = string.Empty;
            public JObject Arguments { get; set; } = new JObject();
        }

        private sealed class ExecutedToolCallInfo
        {
            public string ToolName { get; set; } = string.Empty;
            public JObject Arguments { get; set; } = new JObject();
            public ToolResponse Response { get; set; } = new ToolResponse();
        }

        private sealed class ToolExecutionSummary
        {
            public int RequestedOperationCount { get; set; }
            public int SuccessfulOperationCount { get; set; }
            public List<string> FailedToolNames { get; set; } = new();
            public bool AnySuccessful => SuccessfulOperationCount > 0;
            public bool AllSuccessful => RequestedOperationCount > 0 && SuccessfulOperationCount == RequestedOperationCount;
        }

        private sealed class AssistantSessionState
        {
            public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
            public string ConversationSummary { get; set; } = string.Empty;
            public int SummarizedMessageCount { get; set; }
            public List<AssistantConversationMessage> Messages { get; set; } = new();
        }

    }
}

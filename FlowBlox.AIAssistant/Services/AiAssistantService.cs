using System.Text;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;
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
        private readonly StringComparer _nameComparer = StringComparer.OrdinalIgnoreCase;
        private readonly object _sessionSync = new();
        private AssistantSessionState? _session;

        public event EventHandler<FlowBlocksChangedEventArgs>? FlowBlocksChanged;
        public event EventHandler<AssistantTranscriptLine>? TranscriptLineAdded;

        public AiAssistantService(
            IAiExecutor executor,
            IFlowBloxAIToolApi tools,
            ILogger? logger = null)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _logger = logger;
        }

        public void ResetSession()
        {
            lock (_sessionSync)
            {
                _session = null;
            }
        }

        public AssistantConfiguration GetConfiguration(out string error)
        {
            error = string.Empty;
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

        public async Task<AssistantResult> GenerateProjectAsync(string userPrompt, CancellationToken ct)
        {
            var result = new AssistantResult();
            AddTranscript(result, AssistantTranscriptKind.Status, "Assistant: Running...");

            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                result.Success = false;
                var message = "Prompt is empty.";
                result.Errors.Add(message);
                AddTranscript(result, AssistantTranscriptKind.Error, $"Assistant: {message}");
                return result;
            }

            var config = GetConfiguration(out var configurationError);
            if (!string.IsNullOrWhiteSpace(configurationError))
            {
                result.Success = false;
                result.Errors.Add(configurationError);
                AddTranscript(result, AssistantTranscriptKind.Error, $"Assistant: {configurationError}");
                return result;
            }

            var maxRounds = Math.Clamp(config.MaxToolRounds, 1, 200);
            var useNativeContinuation = config.Provider?.SupportsNativeResponseContinuation == true;
            var session = GetOrCreateSession();
            var toolDefinitions = _tools.GetToolDefinitions();
            var systemPrompt = BuildSystemPrompt();
            var sessionBootstrapPrompt = BuildSessionBootstrapPrompt(toolDefinitions);
            var latestToolTranscript = new List<string>();
            var knownFlowBlocksByName = CaptureFlowBlocksByName();
            var protocolWriter = TryCreateCommunicationProtocolWriter(config, userPrompt, session.SessionId);
            var formatRetryIssued = false;
            protocolWriter?.AppendAiAssistantServiceText("System prompt prepared", systemPrompt);
            protocolWriter?.AppendAiAssistantServiceText("Session bootstrap prompt prepared", sessionBootstrapPrompt);

            AddTranscript(result, AssistantTranscriptKind.Status, "Assistant: Starting tool loop...");

            try
            {
                for (var round = 1; round <= maxRounds; round++)
                {
                    ct.ThrowIfCancellationRequested();

                    var roundPrompt = BuildRoundPrompt(
                        session,
                        sessionBootstrapPrompt,
                        userPrompt,
                        (round == 1 || !useNativeContinuation) ? GetCurrentProjectJson() : null,
                        latestToolTranscript,
                        round,
                        maxRounds,
                        includeFullContext: !useNativeContinuation);
                    protocolWriter?.AppendAiAssistantServiceText($"Round {round} prompt prepared", roundPrompt);

                    var exec = await _executor.ExecutePromptAsync(
                        systemPrompt,
                        roundPrompt,
                        config,
                        session.LastResponseId,
                        ct).ConfigureAwait(false);
                    result.RawModelOutput = exec.RawOutput ?? exec.OutputText ?? string.Empty;
                    UpdateSessionResponseId(session, exec.ResponseId);

                    if (!exec.Success)
                    {
                        result.Success = false;
                        var error = string.IsNullOrWhiteSpace(exec.Error) ? "AI request failed." : exec.Error;
                        result.Errors.Add(error);
                        AddTranscript(result, AssistantTranscriptKind.Error, $"Assistant: {error}");
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
                            $"Assistant (round {round}): {assistantOutput}",
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
                                "Assistant: Response format invalid. Sent one correction hint and retrying.");
                            result.Warnings.Add("Assistant response format invalid; retrying once with explicit format guidance.");
                            protocolWriter?.AppendAiAssistantServiceText("Format validation guidance issued", formatGuidance);

                            if (useNativeContinuation)
                                latestToolTranscript = [formatGuidance];
                            else
                                latestToolTranscript.Add(formatGuidance);

                            continue;
                        }

                        result.Success = false;
                        result.Errors.Add("Assistant returned an invalid response format twice. Aborting execution.");
                        AddTranscript(result, AssistantTranscriptKind.Error,
                            "Assistant: Invalid response format repeated after correction. Aborting.");
                        AppendSessionTurn(session, userPrompt, assistantOutput);
                        return result;
                    }


                    protocolWriter?.AppendAiJson(round, TryParseFirstJsonObject(assistantOutput));

                    var assistantRoundMessage = string.IsNullOrWhiteSpace(instruction.AssistantMessage)
                        ? $"Assistant (round {round}): [no assistantMessage]"
                        : $"Assistant (round {round}): {instruction.AssistantMessage}";
                    AddTranscript(
                        result,
                        AssistantTranscriptKind.Assistant,
                        assistantRoundMessage,
                        instruction.InternalContent);

                    if (instruction.ToolCalls.Count == 0 || instruction.Final)
                    {
                        result.Success = true;
                        result.AssistantText = instruction.AssistantMessage;
                        result.Summary = "Assistant response generated.";

                        var finalText = string.IsNullOrWhiteSpace(instruction.AssistantMessage)
                            ? assistantOutput
                            : instruction.AssistantMessage;
                        AppendSessionTurn(session, userPrompt, finalText);
                        return result;
                    }

                    AddTranscript(result, AssistantTranscriptKind.Status, $"Assistant: Executing {instruction.ToolCalls.Count} tool call(s)...");
                    var roundToolTranscript = new List<string>();

                    foreach (var toolCall in instruction.ToolCalls)
                    {
                        ct.ThrowIfCancellationRequested();

                        var request = new ToolRequest
                        {
                            ToolName = toolCall.ToolName,
                            Arguments = toolCall.Arguments,
                            CorrelationId = Guid.NewGuid().ToString("N")
                        };

                        var response = await _tools.ExecuteAsync(request, ct).ConfigureAwait(false);
                        protocolWriter?.AppendToolCall(round, request, response);

                        AddTranscript(result, response.Ok ? AssistantTranscriptKind.Status : AssistantTranscriptKind.Error,
                            $"Tool {request.ToolName}: {(response.Ok ? "OK" : response.Error)}");

                        roundToolTranscript.Add(JsonConvert.SerializeObject(new
                        {
                            tool = request.ToolName,
                            arguments = request.Arguments,
                            response
                        }, Formatting.None));

                        if (!response.Ok)
                        {
                            result.Warnings.Add($"Tool '{request.ToolName}' failed: {response.Error}");
                            _logger?.Warn($"Assistant tool call failed. Tool={request.ToolName}, Error={response.Error}");
                        }

                        knownFlowBlocksByName = NotifyFlowBlockChanges(knownFlowBlocksByName);
                    }

                    if (useNativeContinuation)
                        latestToolTranscript = roundToolTranscript;
                    else
                        latestToolTranscript.AddRange(roundToolTranscript);
                }

                result.Success = false;
                result.Errors.Add($"Assistant reached max tool rounds ({maxRounds}) without a final response.");
                AddTranscript(result, AssistantTranscriptKind.Error,
                    $"Assistant: Reached max tool rounds ({maxRounds}) without a final response.");
                AppendSessionTurn(session, userPrompt, $"No final response after {maxRounds} rounds.");
                return result;
            }
            finally
            {
                protocolWriter?.TryWrite(_logger);
            }
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

        private void UpdateSessionResponseId(AssistantSessionState session, string responseId)
        {
            if (session == null || string.IsNullOrWhiteSpace(responseId))
                return;

            lock (_sessionSync)
            {
                session.LastResponseId = responseId;
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

            session.Messages.Add(new SessionMessage
            {
                Role = role,
                Content = content.Trim()
            });

            const int maxMessages = 20;
            if (session.Messages.Count > maxMessages)
                session.Messages.RemoveRange(0, session.Messages.Count - maxMessages);
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

        private static string BuildSystemPrompt()
        {
            var prompt = AssistantPromptCatalog.GetPromptContentOrNull(AssistantPromptCatalog.SystemMessageKey);
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new InvalidOperationException(
                    $"Required assistant system prompt '{AssistantPromptCatalog.SystemMessageKey}' is missing or empty.");
            }

            return prompt;
        }

        private static string BuildSessionBootstrapPrompt(IReadOnlyList<ToolDefinition> toolDefinitions)
        {
            var rootCategories = FlowBlockCategory.GetAll()
                .Where(x => x.ParentCategory == null)
                .Select(x => x.DisplayName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();
            var template = AssistantPromptCatalog.GetPromptContentOrNull(AssistantPromptCatalog.SessionBootstrapKey);
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new InvalidOperationException(
                    $"Required assistant bootstrap prompt '{AssistantPromptCatalog.SessionBootstrapKey}' is missing or empty.");
            }

            return template
                .Replace("{{ROOT_CATEGORIES}}", string.Join(", ", rootCategories), StringComparison.Ordinal)
                .Replace("{{CENTRAL_GUIDELINES}}", BuildCentralGuidelinesText(), StringComparison.Ordinal)
                .Replace("{{EXPLANATION_MANIFEST}}", BuildExplanationManifestText(), StringComparison.Ordinal)
                .Replace("{{AVAILABLE_TOOLS}}", BuildToolDefinitionsText(toolDefinitions), StringComparison.Ordinal);
        }

        private static string BuildToolDefinitionsText(IReadOnlyList<ToolDefinition> toolDefinitions)
        {
            var sb = new StringBuilder();
            foreach (var tool in toolDefinitions)
            {
                sb.Append("- ");
                sb.Append(tool.Name);
                sb.Append(": ");
                sb.Append(tool.Description);
                if (tool.ArgumentsSchema?.HasValues == true)
                {
                    sb.Append(" args=");
                    sb.Append(tool.ArgumentsSchema.ToString(Formatting.None));
                }

                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildExplanationManifestText()
        {
            var explanations = AssistantPromptCatalog.GetAllEntries();
            if (explanations.Count == 0)
                return "[]";

            return string.Join(
                ", ",
                explanations.Select(x => $"{x.Key}:{x.ContentHash}"));
        }

        private static string BuildCentralGuidelinesText()
        {
            var sections = new List<string>();

            var iteration = AssistantPromptCatalog.GetPromptContentOrNull(AssistantPromptCatalog.IterationContextKey);
            if (!string.IsNullOrWhiteSpace(iteration))
            {
                sections.Add("Topic: IterationContext / Flow\n" + iteration.Trim());
            }

            var editDelete = AssistantPromptCatalog.GetPromptContentOrNull(AssistantPromptCatalog.EditAndDeleteKey);
            if (!string.IsNullOrWhiteSpace(editDelete))
            {
                sections.Add("Topic: Update / Delete Handling\n" + editDelete.Trim());
            }

            if (sections.Count == 0)
                return "No central guidelines available.";

            return string.Join("\n\n", sections);
        }

        private static string BuildRoundPrompt(
            AssistantSessionState session,
            string sessionBootstrapPrompt,
            string userPrompt,
            string? projectJson,
            List<string> toolTranscript,
            int round,
            int maxRounds,
            bool includeFullContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Round: {round}/{maxRounds}");

            if (session != null && (string.IsNullOrWhiteSpace(session.LastResponseId) || includeFullContext))
            {
                sb.AppendLine(sessionBootstrapPrompt);
                sb.AppendLine();
            }

            if (round == 1 || includeFullContext)
            {
                sb.AppendLine("User prompt:");
                sb.AppendLine(userPrompt);
                sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(projectJson))
                {
                    sb.AppendLine("Current project JSON:");
                    sb.AppendLine(projectJson);
                    sb.AppendLine();
                }

                sb.AppendLine("Tool execution history (latest first):");
            }
            else
            {
                sb.AppendLine("Tool execution updates since last round:");
            }

            if (toolTranscript.Count == 0)
            {
                sb.AppendLine("[]");
            }
            else
            {
                foreach (var item in toolTranscript.TakeLast(20).Reverse())
                    sb.AppendLine(item);
            }

            return sb.ToString();
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

        private static JObject? TryParseFirstJsonObject(string output)
        {
            try
            {
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

        private sealed class AssistantSessionState
        {
            public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
            public string LastResponseId { get; set; } = string.Empty;
            public List<SessionMessage> Messages { get; set; } = new();
        }

        private sealed class SessionMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

    }
}

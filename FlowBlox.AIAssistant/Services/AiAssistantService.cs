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
            var rawConfig = FlowBloxOptions.GetOptionInstance().GetOption(AssistantConfiguration.OptionKey)?.Value ?? string.Empty;
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
                var option = options.GetOption(AssistantConfiguration.OptionKey);
                if (option == null)
                {
                    error = $"Option '{AssistantConfiguration.OptionKey}' not found.";
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

            var maxRounds = Math.Clamp(config.MaxToolRounds, 1, 20);
            var session = GetOrCreateSession();
            var toolDefinitions = _tools.GetToolDefinitions();
            var systemPrompt = BuildSystemPrompt();
            var sessionBootstrapPrompt = BuildSessionBootstrapPrompt(toolDefinitions);
            var latestToolTranscript = new List<string>();
            var knownFlowBlocksByName = CaptureFlowBlocksByName();

            AddTranscript(result, AssistantTranscriptKind.Status, "Assistant: Starting tool loop...");

            for (var round = 1; round <= maxRounds; round++)
            {
                ct.ThrowIfCancellationRequested();

                var roundPrompt = BuildRoundPrompt(
                    session,
                    sessionBootstrapPrompt,
                    userPrompt,
                    round == 1 ? GetCurrentProjectJson() : null,
                    latestToolTranscript,
                    round,
                    maxRounds);

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
                    AddTranscript(
                        result,
                        AssistantTranscriptKind.Assistant,
                        $"Assistant (round {round}): {assistantOutput}",
                        assistantOutput);
                    result.Success = true;
                    result.AssistantText = assistantOutput;
                    result.Summary = "Assistant response generated without tool calls.";
                    AppendSessionTurn(session, userPrompt, assistantOutput);
                    return result;
                }

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

                latestToolTranscript = roundToolTranscript;
            }

            result.Success = false;
            result.Errors.Add($"Assistant reached max tool rounds ({maxRounds}) without a final response.");
            AddTranscript(result, AssistantTranscriptKind.Error,
                $"Assistant: Reached max tool rounds ({maxRounds}) without a final response.");
            AppendSessionTurn(session, userPrompt, $"No final response after {maxRounds} rounds.");
            return result;
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
            var sb = new StringBuilder();
            sb.AppendLine("You are FlowBlox AI Assistant.");
            sb.AppendLine("Do not output markdown. Output exactly one JSON object (single object, no duplicates, no trailing text) using this schema:");
            sb.AppendLine("{");
            sb.AppendLine("  \"assistantMessage\": \"short status or final answer\",");
            sb.AppendLine("  \"final\": false,");
            sb.AppendLine("  \"toolCalls\": [");
            sb.AppendLine("    { \"toolName\": \"ToolName\", \"arguments\": { } }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            sb.AppendLine("Rules for final flag:");
            sb.AppendLine("- final=true ends the complete prompt processing for this user request.");
            sb.AppendLine("- Use final=true only when the user goal is completed or you can prove it is blocked.");
            sb.AppendLine("- If more tool work is needed, final must be false.");
            sb.AppendLine("- This is a single user-prompt session with iterative tool API conversation. Set final=true only after the complete requested task is fully done.");
            sb.AppendLine("- Do not set final=true for intermediate status updates, partial progress, or missing verification steps.");
            sb.AppendLine("When finished, set \"final\": true and return no toolCalls.");
            return sb.ToString();
        }

        private static string BuildSessionBootstrapPrompt(IReadOnlyList<ToolDefinition> toolDefinitions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Session bootstrap context for FlowBlox project editing:");
            sb.AppendLine("You can call tools to inspect and modify the active FlowBlox project directly (CRUD + connect/disconnect). Use batch calls to reduce request count.");
            sb.AppendLine("Property paths for updates use JSON-path-like slash syntax: /Property/0/NestedProperty.");
            sb.AppendLine("Use GetProjectJson for the current project snapshot before and after structural changes.");
            var rootCategories = FlowBlockCategory.GetAll()
                .Where(x => x.ParentCategory == null)
                .Select(x => x.DisplayName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();
            sb.AppendLine($"Known root categories: {string.Join(", ", rootCategories)}");
            sb.AppendLine("Rules for updates and references:");
            sb.AppendLine("- Never update managed objects indirectly through parent objects. If a property is a managed object (including FieldElement), update that object directly with UpdateManagedObject.");
            sb.AppendLine("- You may update simple properties and FlowBloxReactiveObject properties (and collections of them) via JSON path from the primary object.");
            sb.AppendLine("- For flow block references, use flow block name strings or {\"resolveFlowBlockByName\":\"FlowBlockName\"}.");
            sb.AppendLine("- For managed object references, resolve references explicitly with one of:");
            sb.AppendLine("  - {\"resolveManagedObjectByName\":\"ManagedObjectName\"}");
            sb.AppendLine("  - {\"resolveFieldElementByFQName\":\"$FlowBlock::FieldName\"}");
            sb.AppendLine("- BaseSingleResultFlowBlock creates its default ResultField automatically. Do not create that default field manually.");
            sb.AppendLine("- For multi-result flow blocks (BaseResultFlowBlock that are not BaseSingleResultFlowBlock), create extra fields with CreateField.");
            sb.AppendLine("- After CreateField, set field references on reactive mapping objects via UpdateFlowBlock/UpdateManagedObject using resolver syntax.");
            sb.AppendLine("- For string properties with EnableFieldSelection, field placeholders use the syntax $FlowBlock::FieldName.");
            sb.AppendLine("- Snapshot rule: rely on GetProjectJson for state verification and use identifiers only (FlowBlock=Name, FieldElement=FullyQualifiedFieldName, ManagedObject=Name).");
            sb.AppendLine("- Iteration context rule: connecting multiple predecessors to one flow block enables automatic combination logic.");
            sb.AppendLine("- Combination default is cross-product of predecessor iteration datasets (Combine).");
            sb.AppendLine("- InputBehavior can override per predecessor, e.g. First instead of Combine.");
            sb.AppendLine("- Layout rule: flows are built from left to right and should follow a center line.");
            sb.AppendLine("- FlowBlock default size is approximately 328x235 px; keep horizontal spacing accordingly.");
            sb.AppendLine("- Always set Location when creating a flow block.");
            sb.AppendLine("- Start by creating StartFlowBlock with typeFullName \"FlowBlox.Core.Models.FlowBlocks.SequenceFlow.StartFlowBlock\" at location x=50, y=400.");
            sb.AppendLine("- Keep primary stacks symmetric around the center line to maintain a clean design.");
            sb.AppendLine("- For interface/abstract FlowBloxReactiveObject property types (FlowBlocks, ManagedObjects, nested ReactiveObjects), call GetSupportedTypes first, then inspect chosen type with kind info before creation/linking.");
            sb.AppendLine("- ActivationConditions can be used on every flow block to control whether it should execute.");
            sb.AppendLine("- Use FieldLogicalComparisonCondition (with logical operator) or LogicalGroupCondition (group of logical conditions) in ActivationConditions.");
            sb.AppendLine("- If a flow block is not activated, it produces an empty result and forwards it to downstream blocks (follow-up chain is effectively cleared).");
            sb.AppendLine("- Always call kind info tools before writing updates: inspect inheritance, UI metadata, enum members, nullability, supported types, and property semantics. Prefer GetTypeKindsInfo.");
            sb.AppendLine("- BaseFlowBlock members are excluded by default when describing derived flow blocks. Query GetTypeKindsInfo explicitly for typeFullName \"FlowBlox.Core.Models.FlowBlocks.Base.BaseFlowBlock\" when needed.");
            sb.AppendLine("- Do not assume category names, paths, types, or objects.");
            sb.AppendLine("- Use only data returned by tools; especially for BatchExecuteToolRequests, every request must be based on known values.");
            sb.AppendLine("- Do not query GetCategoryChildren for guessed paths. Use root categories and returned child paths exactly.");
            sb.AppendLine("Available tools:");

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

            return sb.ToString();
        }

        private static string BuildRoundPrompt(
            AssistantSessionState session,
            string sessionBootstrapPrompt,
            string userPrompt,
            string? projectJson,
            List<string> toolTranscript,
            int round,
            int maxRounds)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Round: {round}/{maxRounds}");

            if (session != null && string.IsNullOrWhiteSpace(session.LastResponseId))
            {
                sb.AppendLine(sessionBootstrapPrompt);
                sb.AppendLine();
            }

            if (round == 1)
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

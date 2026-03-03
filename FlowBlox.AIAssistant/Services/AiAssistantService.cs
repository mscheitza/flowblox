using System.Text;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Services
{
    public class AiAssistantService
    {
        private readonly IAiExecutor _executor;
        private readonly IFlowBloxAIToolApi _tools;
        private readonly IOptionsProvider _optionsProvider;
        private readonly ILogger? _logger;

        public AiAssistantService(
            IAiExecutor executor,
            IFlowBloxAIToolApi tools,
            IOptionsProvider optionsProvider,
            ILogger? logger = null)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _optionsProvider = optionsProvider ?? throw new ArgumentNullException(nameof(optionsProvider));
            _logger = logger;
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

            var rawConfig = _optionsProvider.GetOptionValue(AssistantConfiguration.OptionKey);
            var parseResult = AssistantConfigurationJson.Parse(rawConfig);
            if (parseResult.HasError)
            {
                result.Success = false;
                result.Errors.Add(parseResult.Error);
                AddTranscript(result, AssistantTranscriptKind.Error, $"Assistant: {parseResult.Error}");
                return result;
            }

            var config = parseResult.Configuration;
            var maxRounds = Math.Clamp(config.MaxToolRounds, 1, 20);
            var systemPrompt = BuildSystemPrompt(_tools.GetToolDefinitions());
            var toolTranscript = new List<string>();

            AddTranscript(result, AssistantTranscriptKind.Status, "Assistant: Starting tool loop...");

            for (var round = 1; round <= maxRounds; round++)
            {
                ct.ThrowIfCancellationRequested();

                var projectJson = GetCurrentProjectJson();
                var roundPrompt = BuildRoundPrompt(userPrompt, projectJson, toolTranscript, round, maxRounds);

                var exec = await _executor.ExecutePromptAsync(systemPrompt, roundPrompt, config, ct).ConfigureAwait(false);
                result.RawModelOutput = exec.RawOutput ?? exec.OutputText ?? string.Empty;

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
                AddTranscript(result, AssistantTranscriptKind.Assistant, $"Assistant (round {round}): {assistantOutput}");

                if (!TryParseAssistantInstruction(assistantOutput, out var instruction))
                {
                    result.Success = true;
                    result.AssistantText = assistantOutput;
                    result.Summary = "Assistant response generated without tool calls.";
                    return result;
                }

                if (instruction.ToolCalls.Count == 0 || instruction.Final)
                {
                    result.Success = true;
                    result.AssistantText = instruction.AssistantMessage;
                    result.Summary = "Assistant response generated.";
                    if (!string.IsNullOrWhiteSpace(instruction.AssistantMessage))
                        AddTranscript(result, AssistantTranscriptKind.Assistant, $"Assistant: {instruction.AssistantMessage}");
                    return result;
                }

                AddTranscript(result, AssistantTranscriptKind.Status, $"Assistant: Executing {instruction.ToolCalls.Count} tool call(s)...");

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

                    toolTranscript.Add(JsonConvert.SerializeObject(new
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
                }
            }

            result.Success = false;
            result.Errors.Add($"Assistant reached max tool rounds ({maxRounds}) without a final response.");
            AddTranscript(result, AssistantTranscriptKind.Error,
                $"Assistant: Reached max tool rounds ({maxRounds}) without a final response.");
            return result;
        }

        private static string GetCurrentProjectJson()
        {
            var project = FlowBloxProjectManager.Instance.ActiveProject;
            if (project == null)
                return "{}";

            return JsonConvert.SerializeObject(project, JsonSettings.ProjectExport());
        }

        private static void AddTranscript(AssistantResult result, AssistantTranscriptKind kind, string text)
        {
            result.TranscriptLines.Add(new AssistantTranscriptLine
            {
                Timestamp = DateTime.Now,
                Kind = kind,
                Text = text
            });
        }

        private static string BuildSystemPrompt(IReadOnlyList<ToolDefinition> toolDefinitions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are FlowBlox AI Assistant.");
            sb.AppendLine("You can call tools to inspect and modify the active FlowBlox project directly (CRUD + connect/disconnect). Use batch calls to reduce request count.");
            sb.AppendLine("Property paths for updates use JSON-path-like slash syntax: /Property/0/NestedProperty.");
            sb.AppendLine("Use snapshot tools to inspect single components before mutating them.");
            sb.AppendLine("Do not output markdown. Always output exactly one JSON object.");
            sb.AppendLine("JSON response contract:");
            sb.AppendLine("{");
            sb.AppendLine("  \"assistantMessage\": \"short status or final answer\",");
            sb.AppendLine("  \"final\": false,");
            sb.AppendLine("  \"toolCalls\": [");
            sb.AppendLine("    { \"toolName\": \"ToolName\", \"arguments\": { } }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            sb.AppendLine("When finished, set \"final\": true and return no toolCalls.");
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
            string userPrompt,
            string projectJson,
            List<string> toolTranscript,
            int round,
            int maxRounds)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Round: {round}/{maxRounds}");
            sb.AppendLine("User prompt:");
            sb.AppendLine(userPrompt);
            sb.AppendLine();
            sb.AppendLine("Current project JSON:");
            sb.AppendLine(projectJson);
            sb.AppendLine();
            sb.AppendLine("Tool execution history (latest first):");

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

            JObject root;
            try
            {
                root = JObject.Parse(output);
            }
            catch
            {
                return false;
            }

            instruction.AssistantMessage = root.Value<string>("assistantMessage")
                ?? root.Value<string>("message")
                ?? root.Value<string>("finalResponse")
                ?? string.Empty;

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

        private sealed class AssistantInstruction
        {
            public string AssistantMessage { get; set; } = string.Empty;
            public bool Final { get; set; }
            public List<AssistantToolCall> ToolCalls { get; set; } = new();
        }

        private sealed class AssistantToolCall
        {
            public string ToolName { get; set; } = string.Empty;
            public JObject Arguments { get; set; } = new JObject();
        }
    }
}

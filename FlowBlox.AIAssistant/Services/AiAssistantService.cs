using System.Text;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Services
{
    public class AiAssistantService
    {
        private readonly IAiExecutor _executor;
        private readonly IFlowBloxAIToolApi _tools;
        private readonly IOptionsProvider _optionsProvider;
        private readonly ILogger _logger;

        public AiAssistantService(
            IAiExecutor executor,
            IFlowBloxAIToolApi tools,
            IOptionsProvider optionsProvider,
            ILogger logger = null)
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
            var parsed = AssistantConfiguration.Parse(rawConfig);
            if (!string.IsNullOrWhiteSpace(parsed.Error))
            {
                result.Success = false;
                result.Errors.Add(parsed.Error);
                AddTranscript(result, AssistantTranscriptKind.Error, $"Assistant: {parsed.Error}");
                return result;
            }

            AddTranscript(result, AssistantTranscriptKind.Status, "Assistant: Preparing prompt...");

            var systemPrompt = BuildSystemPrompt();
            var exec = await _executor.ExecutePromptAsync(systemPrompt, userPrompt, parsed.Configuration, ct).ConfigureAwait(false);
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

            AddTranscript(result, AssistantTranscriptKind.Status, "Assistant: Parsing output...");
            var extraction = TryExtractProjectJson(exec.OutputText);
            if (!extraction.Success)
            {
                result.Success = false;
                result.Errors.Add(extraction.Error);
                AddTranscript(result, AssistantTranscriptKind.Error, $"Assistant: {extraction.Error}");
                return result;
            }

            result.Success = true;
            result.ProjectJson = extraction.ProjectJson;
            result.Summary = "Project JSON generated.";
            AddTranscript(result, AssistantTranscriptKind.Assistant, "Assistant: Done. Project JSON generated.");
            return result;
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

        private static string BuildSystemPrompt()
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are FlowBlox AI Assistant.");
            sb.AppendLine("Goal: Produce exactly one JSON object representing a FlowBloxProject.");
            sb.AppendLine("Constraints:");
            sb.AppendLine("- Output must be valid JSON object only.");
            sb.AppendLine("- Use top-level-first references where possible.");
            sb.AppendLine("- FlowBlock names should use type name without 'FlowBlock' suffix plus semantic postfix.");
            sb.AppendLine("- Single result flow blocks should use 'ResultField' names that describe the content.");
            sb.AppendLine("Tool API is available for future tool-calling expansion.");
            return sb.ToString();
        }

        private static (bool Success, string ProjectJson, string Error) TryExtractProjectJson(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return (false, string.Empty, "Model output is empty.");

            if (TryParseObject(output, out var direct))
                return (true, direct, string.Empty);

            if (TryExtractFirstJsonObject(output, out var extracted) && TryParseObject(extracted, out var normalized))
                return (true, normalized, string.Empty);

            return (false, string.Empty, "Could not extract a JSON object from model output.");
        }

        private static bool TryParseObject(string candidate, out string normalized)
        {
            normalized = string.Empty;
            try
            {
                var obj = JObject.Parse(candidate);
                normalized = obj.ToString(Formatting.Indented);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryExtractFirstJsonObject(string text, out string jsonObject)
        {
            jsonObject = string.Empty;
            var start = text.IndexOf('{');
            if (start < 0)
                return false;

            var depth = 0;
            var inString = false;
            var escaped = false;
            for (var i = start; i < text.Length; i++)
            {
                var ch = text[i];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }

                    if (ch == '\\')
                    {
                        escaped = true;
                        continue;
                    }

                    if (ch == '"')
                        inString = false;

                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == '{')
                    depth++;
                else if (ch == '}')
                    depth--;

                if (depth == 0)
                {
                    jsonObject = text.Substring(start, i - start + 1);
                    return true;
                }
            }

            return false;
        }

        private async Task RunToolLoopAsync(CancellationToken ct)
        {
            await Task.CompletedTask;
            // Reserved for post-MVP tool-calling loop.
        }
    }
}

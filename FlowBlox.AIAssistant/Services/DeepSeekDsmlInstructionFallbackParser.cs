using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Services
{
    /// <summary>
    /// Recovers DeepSeek DSML tool-call envelopes that can be returned as assistant text even though FlowBlox currently
    /// expects its own JSON instruction contract. This keeps DeepSeek usable during the transition, but should become
    /// unnecessary once FlowBlox drives tools through provider-native/Semantic-Kernel function calling end to end.
    /// </summary>
    public sealed class DeepSeekDsmlInstructionFallbackParser : IAssistantInstructionFallbackParser
    {
        private const string ToolPluginPrefix = "FlowBloxAIToolApi";
        private const string DsmlMarkerPattern = @"(?:\|\|DSML\|\||\uFF5C\uFF5CDSML\uFF5C\uFF5C|\uFF5CDSML\uFF5C)";
        private const string ToolCallsTagPattern = @"tool\\?_calls";

        private static readonly Regex ToolCallsRegex = new(
            $@"<\s*{DsmlMarkerPattern}\s*{ToolCallsTagPattern}\s*>(?<body>.*?)\\?\s*</\s*{DsmlMarkerPattern}\s*{ToolCallsTagPattern}\s*>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly Regex InvokeRegex = new(
            $@"<\s*{DsmlMarkerPattern}\s*invoke\s+name\s*=\s*""(?<name>[^""]+)""\s*>(?<body>.*?)\\?\s*</\s*{DsmlMarkerPattern}\s*invoke\s*>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly Regex ParameterRegex = new(
            $@"<\s*{DsmlMarkerPattern}\s*parameter\s+name\s*=\s*""(?<name>[^""]+)""(?:\s+string\s*=\s*""(?<string>true|false)"")?\s*>(?<value>.*?)\\?\s*</\s*{DsmlMarkerPattern}\s*parameter\s*>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public bool TryParse(
            string output,
            Exception? primaryParseException,
            out AssistantInstructionParseResult result)
        {
            result = new AssistantInstructionParseResult
            {
                ResponseContent = output ?? string.Empty,
                Exception = primaryParseException
            };

            if (string.IsNullOrWhiteSpace(output))
                return false;

            var toolCallsMatch = ToolCallsRegex.Match(output);
            if (!toolCallsMatch.Success)
                return false;

            var instruction = new AssistantInstruction
            {
                AssistantMessage = output[..toolCallsMatch.Index].Trim(),
                Final = false
            };

            foreach (Match invokeMatch in InvokeRegex.Matches(toolCallsMatch.Groups["body"].Value))
            {
                var toolName = NormalizeToolName(invokeMatch.Groups["name"].Value);
                if (string.IsNullOrWhiteSpace(toolName))
                    continue;

                instruction.ToolCalls.Add(new AssistantToolCall
                {
                    ToolName = toolName,
                    Arguments = ParseArguments(invokeMatch.Groups["body"].Value)
                });
            }

            if (instruction.ToolCalls.Count == 0)
                return false;

            var root = ToJsonObject(instruction);
            instruction.InternalContent = root.ToString(Formatting.Indented);
            result = new AssistantInstructionParseResult
            {
                Instruction = instruction,
                JsonObject = root,
                ResponseContent = output ?? string.Empty
            };
            return true;
        }

        private static JObject ParseArguments(string invokeBody)
        {
            var arguments = new JObject();
            foreach (Match parameterMatch in ParameterRegex.Matches(invokeBody ?? string.Empty))
            {
                var name = parameterMatch.Groups["name"].Value.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var value = parameterMatch.Groups["value"].Value.Trim();
                var isString = !string.Equals(parameterMatch.Groups["string"].Value, "false", StringComparison.OrdinalIgnoreCase);
                arguments[name] = isString
                    ? value
                    : ParseJsonValueOrString(value);
            }

            return arguments;
        }

        private static JToken ParseJsonValueOrString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return JValue.CreateNull();

            try
            {
                return JToken.Parse(value);
            }
            catch (JsonException)
            {
                return value;
            }
        }

        private static string NormalizeToolName(string toolName)
        {
            var normalized = (toolName ?? string.Empty).Trim();
            if (normalized.StartsWith(ToolPluginPrefix + "-", StringComparison.OrdinalIgnoreCase))
                return normalized[(ToolPluginPrefix.Length + 1)..];

            if (normalized.StartsWith(ToolPluginPrefix + ".", StringComparison.OrdinalIgnoreCase))
                return normalized[(ToolPluginPrefix.Length + 1)..];

            return normalized;
        }

        private static JObject ToJsonObject(AssistantInstruction instruction)
        {
            return new JObject
            {
                ["assistantMessage"] = instruction.AssistantMessage ?? string.Empty,
                ["final"] = instruction.Final,
                ["toolCalls"] = new JArray(instruction.ToolCalls.Select(toolCall => new JObject
                {
                    ["toolName"] = toolCall.ToolName,
                    ["arguments"] = toolCall.Arguments ?? new JObject()
                }))
            };
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    internal static class DeepSeekDsmlInstructionParser
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

        public static bool TryParse(string output, out JObject instructionJson)
        {
            instructionJson = new JObject();

            if (string.IsNullOrWhiteSpace(output))
                return false;

            var toolCallsMatch = ToolCallsRegex.Match(output);
            if (!toolCallsMatch.Success)
                return false;

            var toolCalls = new JArray();
            foreach (Match invokeMatch in InvokeRegex.Matches(toolCallsMatch.Groups["body"].Value))
            {
                var toolName = NormalizeToolName(invokeMatch.Groups["name"].Value);
                if (string.IsNullOrWhiteSpace(toolName))
                    continue;

                toolCalls.Add(new JObject
                {
                    ["toolName"] = toolName,
                    ["arguments"] = ParseArguments(invokeMatch.Groups["body"].Value)
                });
            }

            if (toolCalls.Count == 0)
                return false;

            instructionJson = new JObject
            {
                ["assistantMessage"] = output[..toolCallsMatch.Index].Trim(),
                ["final"] = false,
                ["toolCalls"] = toolCalls
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
    }
}

using FlowBlox.Core.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Services
{
    internal sealed class AiAssistantInstructionParser
    {
        public AssistantInstructionParseResult Parse(string output)
        {
            var jsonResult = ParseFirstJsonObject(output);
            if (jsonResult.JsonObject == null)
            {
                return new AssistantInstructionParseResult
                {
                    ResponseContent = output ?? string.Empty,
                    Exception = jsonResult.Exception
                };
            }

            var root = jsonResult.JsonObject;
            var instruction = new AssistantInstruction
            {
                AssistantMessage = root.Value<string>("assistantMessage")
                    ?? root.Value<string>("message")
                    ?? root.Value<string>("finalResponse")
                    ?? string.Empty,
                InternalContent = root.ToString(Formatting.Indented),
                Final = root.Value<bool?>("final") == true
            };

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

            return new AssistantInstructionParseResult
            {
                Instruction = instruction,
                JsonObject = root,
                ResponseContent = output ?? string.Empty
            };
        }

        public AssistantInstructionParseResult ParseFirstJsonObject(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return new AssistantInstructionParseResult
                {
                    ResponseContent = output ?? string.Empty,
                    Exception = new FormatException("Assistant response was empty.")
                };
            }

            try
            {
                if (TextHelper.TrySubstringFromFirstOccurrence(output, '{', out var objectCandidate) && !string.IsNullOrWhiteSpace(objectCandidate))
                    output = objectCandidate;

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
                    {
                        return new AssistantInstructionParseResult
                        {
                            JsonObject = obj,
                            ResponseContent = output
                        };
                    }

                    return new AssistantInstructionParseResult
                    {
                        ResponseContent = output,
                        Exception = new FormatException("First JSON token was not an object.")
                    };
                }
            }
            catch (Exception ex)
            {
                return new AssistantInstructionParseResult
                {
                    ResponseContent = output,
                    Exception = ex
                };
            }

            return new AssistantInstructionParseResult
            {
                ResponseContent = output,
                Exception = new FormatException("Assistant response did not contain a JSON object.")
            };
        }
    }
}
using Newtonsoft.Json;

namespace FlowBlox.AIAssistant.Builder
{
    [JsonConverter(typeof(AssistantSessionMessageJsonConverter))]
    public abstract class AssistantSessionMessage
    {
        public abstract string Role { get; }
        public abstract string CompleteMessage { get; }
        public int CharacterCount => CompleteMessage?.Length ?? 0;

        public abstract AssistantSessionMessage Clone();
    }

    public sealed class AssistantSingleMessage : AssistantSessionMessage
    {
        public string MessageRole { get; set; } = "user";
        public string Message { get; set; } = string.Empty;

        public override string Role => string.Equals(MessageRole, "assistant", StringComparison.OrdinalIgnoreCase)
            ? "assistant"
            : "user";

        public override string CompleteMessage => Message?.Trim() ?? string.Empty;

        public override AssistantSessionMessage Clone()
        {
            return new AssistantSingleMessage
            {
                MessageRole = Role,
                Message = Message
            };
        }
    }

    public sealed class AssistantMessagePair : AssistantSessionMessage
    {
        public string AssistantRequest { get; set; } = string.Empty;
        public string ToolApiResponse { get; set; } = string.Empty;

        public override string Role => "user";

        public override string CompleteMessage
        {
            get
            {
                var assistantRequest = AssistantRequest?.Trim() ?? string.Empty;
                var toolApiResponse = ToolApiResponse?.Trim() ?? string.Empty;
                return
                    "Assistant request:\n" +
                    assistantRequest +
                    "\n\nTool API response:\n" +
                    toolApiResponse;
            }
        }

        public override AssistantSessionMessage Clone()
        {
            return new AssistantMessagePair
            {
                AssistantRequest = AssistantRequest,
                ToolApiResponse = ToolApiResponse
            };
        }
    }

    internal sealed class AssistantSessionMessageJsonConverter : JsonConverter<AssistantSessionMessage>
    {
        public override void WriteJson(JsonWriter writer, AssistantSessionMessage? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            switch (value)
            {
                case AssistantMessagePair pair:
                    writer.WritePropertyName("messageType");
                    writer.WriteValue("pair");
                    writer.WritePropertyName(nameof(AssistantMessagePair.AssistantRequest));
                    writer.WriteValue(pair.AssistantRequest);
                    writer.WritePropertyName(nameof(AssistantMessagePair.ToolApiResponse));
                    writer.WriteValue(pair.ToolApiResponse);
                    break;

                case AssistantSingleMessage single:
                    writer.WritePropertyName("messageType");
                    writer.WriteValue("single");
                    writer.WritePropertyName(nameof(AssistantSingleMessage.MessageRole));
                    writer.WriteValue(single.Role);
                    writer.WritePropertyName(nameof(AssistantSingleMessage.Message));
                    writer.WriteValue(single.Message);
                    break;

                case null:
                    break;

                default:
                    throw new JsonSerializationException($"Unsupported assistant session message type '{value.GetType().FullName}'.");
            }

            writer.WriteEndObject();
        }

        public override AssistantSessionMessage? ReadJson(
            JsonReader reader,
            Type objectType,
            AssistantSessionMessage? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var obj = Newtonsoft.Json.Linq.JObject.Load(reader);
            var messageType = obj.Value<string>("messageType") ?? obj.Value<string>("MessageType") ?? "single";

            if (string.Equals(messageType, "pair", StringComparison.OrdinalIgnoreCase))
            {
                return new AssistantMessagePair
                {
                    AssistantRequest = obj.Value<string>(nameof(AssistantMessagePair.AssistantRequest)) ?? string.Empty,
                    ToolApiResponse = obj.Value<string>(nameof(AssistantMessagePair.ToolApiResponse)) ?? string.Empty
                };
            }

            return new AssistantSingleMessage
            {
                MessageRole = obj.Value<string>(nameof(AssistantSingleMessage.MessageRole))
                    ?? obj.Value<string>("Role")
                    ?? "user",
                Message = obj.Value<string>(nameof(AssistantSingleMessage.Message))
                    ?? obj.Value<string>("Content")
                    ?? string.Empty
            };
        }
    }
}

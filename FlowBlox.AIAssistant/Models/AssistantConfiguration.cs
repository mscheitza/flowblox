using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Models
{
    public class AssistantProviderConfiguration
    {
        public string Type { get; set; } = "OpenAI";
        public string Name { get; set; } = string.Empty;
        public JObject Properties { get; set; } = new JObject();
    }

    public class AssistantConfiguration
    {
        public const string OptionKey = "AI.AssistantConfiguration";

        public AssistantProviderConfiguration Provider { get; set; } = new AssistantProviderConfiguration();
        public string Model { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }

        public static (AssistantConfiguration Configuration, string Error) Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return (new AssistantConfiguration(), string.Empty);
            }

            try
            {
                var root = JObject.Parse(raw);
                var config = new AssistantConfiguration();

                if (root["provider"] is JObject providerObject)
                {
                    config.Provider = new AssistantProviderConfiguration
                    {
                        Type = providerObject.Value<string>("type") ?? "OpenAI",
                        Name = providerObject.Value<string>("name") ?? string.Empty,
                        Properties = providerObject
                    };
                }
                else if (root["provider"] is JValue providerValue)
                {
                    config.Provider = new AssistantProviderConfiguration
                    {
                        Type = providerValue.Value<string>() ?? "OpenAI"
                    };
                }

                config.Model = root.Value<string>("model") ?? string.Empty;
                config.Endpoint = root.Value<string>("endpoint") ?? string.Empty;
                config.ApiKey = root.Value<string>("apiKey") ?? string.Empty;
                config.Temperature = root.Value<double?>("temperature");
                config.MaxTokens = root.Value<int?>("maxTokens");

                var validationError = Validate(config);
                return (config, validationError);
            }
            catch (Exception ex)
            {
                return (new AssistantConfiguration(), $"Invalid JSON in option '{OptionKey}': {ex.Message}");
            }
        }

        private static string Validate(AssistantConfiguration config)
        {
            if (config == null)
                return "Assistant configuration is null.";

            if (config.Temperature.HasValue && (config.Temperature.Value < 0 || config.Temperature.Value > 2))
                return "Temperature must be between 0 and 2.";

            if (config.MaxTokens.HasValue && config.MaxTokens.Value <= 0)
                return "MaxTokens must be greater than 0.";

            return string.Empty;
        }
    }
}

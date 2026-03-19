using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using Newtonsoft.Json;

namespace FlowBlox.AIAssistant.Services
{
    public static class AssistantConfigurationJson
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static AssistantConfigurationParseResult Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new AssistantConfigurationParseResult();

            try
            {
                var configuration = JsonConvert.DeserializeObject<AssistantConfiguration>(raw, _settings)
                    ?? new AssistantConfiguration();

                configuration.Provider ??= new OpenAIProvider();
                return new AssistantConfigurationParseResult
                {
                    Configuration = configuration
                };
            }
            catch (Exception ex)
            {
                return new AssistantConfigurationParseResult
                {
                    Error = $"Invalid JSON in option 'AI.AssistantConfiguration': {ex.Message}"
                };
            }
        }

        public static string Serialize(AssistantConfiguration configuration)
        {
            var normalized = configuration ?? new AssistantConfiguration();
            normalized.Provider ??= new OpenAIProvider();
            return JsonConvert.SerializeObject(normalized, _settings);
        }
    }
}

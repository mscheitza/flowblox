namespace FlowBlox.AIAssistant.Models
{
    public class AssistantConfigurationParseResult
    {
        public AssistantConfiguration Configuration { get; set; } = new AssistantConfiguration();
        public string Error { get; set; } = string.Empty;
        public bool HasError => !string.IsNullOrWhiteSpace(Error);
    }
}

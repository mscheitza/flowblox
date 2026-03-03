using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;

namespace FlowBlox.AIAssistant.Models
{
    public class AssistantConfiguration
    {
        public const string OptionKey = "AI.AssistantConfiguration";

        public AIProviderBase Provider { get; set; } = new OpenAIProvider();
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public int MaxToolRounds { get; set; } = 8;
    }
}

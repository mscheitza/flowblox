using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.AIAssistant.Services
{
    internal sealed class AssistantInstructionFallbackParserProvider
    {
        public IEnumerable<IAssistantInstructionFallbackParser> GetParsers(AIProviderBase? provider)
        {
            if (string.Equals(provider?.ProviderType, "DeepSeek", StringComparison.OrdinalIgnoreCase))
                yield return new DeepSeekDsmlInstructionFallbackParser();
        }
    }
}
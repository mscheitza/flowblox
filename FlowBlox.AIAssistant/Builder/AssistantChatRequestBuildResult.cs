using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.AIAssistant.Builder
{
    internal sealed class AssistantChatRequestBuildResult
    {
        public AIChatRequest Request { get; init; } = new();
        public int FirstIncludedHistoryMessageIndex { get; init; }
        public int IncludedHistoryMessageCount { get; init; }
    }
}

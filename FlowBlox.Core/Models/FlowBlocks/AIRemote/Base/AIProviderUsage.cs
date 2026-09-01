namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    internal sealed record AIProviderUsage(
        int? PromptTokens,
        int? CompletionTokens);
}
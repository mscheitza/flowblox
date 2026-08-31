using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Enums;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    [Display(Name = "KimiAIProvider_DisplayName", Description = "KimiAIProvider_Description", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("KimiAIProvider_DisplayName_Plural", typeof(FlowBloxTexts))]
    public sealed class KimiAIProvider : OpenAICompatibleProviderBase
    {
        public override string ProviderType => "Kimi";

        protected override string ProviderDisplayName => "Kimi";

        public KimiAIProvider() : base("https://api.moonshot.ai/v1", "kimi-k3")
        {
            EstimatedSystemPromptCacheSavingsRate = 0.50d;
        }

        protected override string GetReasoningEffortValue()
        {
            return ReasoningEffort == AIReasoningEffort.Low
                ? "low"
                : "high";
        }
    }
}
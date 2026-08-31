using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Enums;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Util.Resources;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    [Display(Name = "DeepSeekAIProvider_DisplayName", Description = "DeepSeekAIProvider_Description", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("DeepSeekAIProvider_DisplayName_Plural", typeof(FlowBloxTexts))]
    public sealed class DeepSeekAIProvider : OpenAICompatibleProviderBase
    {
        public override string ProviderType => "DeepSeek";

        protected override string ProviderDisplayName => "DeepSeek";

        public DeepSeekAIProvider() : base("https://api.deepseek.com", "deepseek-v4-flash")
        {
            EstimatedSystemPromptCacheSavingsRate = 0.60d;
            ReasoningEffort = AIReasoningEffort.Low;
        }

        protected override string GetReasoningEffortValue()
        {
            return ReasoningEffort == AIReasoningEffort.Low
                ? "low"
                : "high";
        }

        protected override void ConfigureExecutionSettings(OpenAIPromptExecutionSettings settings, AIChatRequest request)
        {
#pragma warning disable SKEXP0010
            settings.ExtraBody = new Dictionary<string, object>
            {
                ["thinking"] = new Dictionary<string, object>
                {
                    ["type"] = "enabled"
                }
            };
#pragma warning restore SKEXP0010
        }
    }
}
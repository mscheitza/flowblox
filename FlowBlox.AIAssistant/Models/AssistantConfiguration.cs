using FlowBlox.Core.Attributes;
using FlowBlox.Core;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.AIAssistant.Models
{
    [Display(Name = "AssistantConfiguration_DisplayName", Description = "AssistantConfiguration_Description", ResourceType = typeof(FlowBloxTexts))]
    [FlowBloxUIGroup("AssistantConfiguration_Groups_General", 0)]
    public class AssistantConfiguration : FlowBloxReactiveObject
    {
        [Required]
        [Display(Name = "AssistantConfiguration_Provider", Description = "AssistantConfiguration_Provider_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Association, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public AIProviderBase Provider { get; set; }

        [Display(Name = "AssistantConfiguration_Temperature", Description = "AssistantConfiguration_Temperature_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public double? Temperature { get; set; }

        [Display(Name = "AssistantConfiguration_MaxTokens", Description = "AssistantConfiguration_MaxTokens_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public int? MaxTokens { get; set; }

        [Display(Name = "AssistantConfiguration_MaxToolRounds", Description = "AssistantConfiguration_MaxToolRounds_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        public int MaxToolRounds { get; set; } = 50;

        [Display(Name = "AssistantConfiguration_EnableCommunicationProtocol", Description = "AssistantConfiguration_EnableCommunicationProtocol_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        public bool EnableCommunicationProtocol { get; set; }

        [Display(Name = "AssistantConfiguration_EnableAutomaticAdjustment", Description = "AssistantConfiguration_EnableAutomaticAdjustment_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        public bool EnableAutomaticAdjustment { get; set; } = true;

        [Display(Name = "Max latest messages", Description = "Number of latest user/assistant messages kept verbatim in chat context.", GroupName = "AssistantConfiguration_Groups_General", Order = 6)]
        public int MaxLatestMessages { get; set; } = 5;

        [Display(Name = "Max context tokens", Description = "Approximate maximum context window used for system prompts, summary, latest messages, and current prompt.", GroupName = "AssistantConfiguration_Groups_General", Order = 7)]
        public int MaxContextTokens { get; set; } = 32000;

        [Display(Name = "Reserved response tokens", Description = "Approximate tokens reserved for the model response when trimming latest messages.", GroupName = "AssistantConfiguration_Groups_General", Order = 8)]
        public int ReservedResponseTokens { get; set; } = 4096;

        [Display(Name = "Characters per token", Description = "Approximate character-to-token ratio used for provider-independent context trimming.", GroupName = "AssistantConfiguration_Groups_General", Order = 9)]
        public int ApproximateCharactersPerToken { get; set; } = 4;
    }
}

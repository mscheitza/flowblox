using FlowBlox.Core.Attributes;
using FlowBlox.Core;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using FlowBlox.AIAssistant.Constants;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.AIAssistant.Models
{
    [Display(Name = "AssistantConfiguration_DisplayName", Description = "AssistantConfiguration_Description", ResourceType = typeof(FlowBloxTexts))]
    [FlowBloxUIGroup("AssistantConfiguration_Groups_General", 0)]
    [FlowBloxUIGroup("AssistantConfiguration_Groups_Extended", 1)]
    public class AssistantConfiguration : FlowBloxReactiveObject
    {
        [Required]
        [Display(Name = "AssistantConfiguration_Provider", Description = "AssistantConfiguration_Provider_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Association, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public AIProviderBase Provider { get; set; }

        [Display(Name = "AssistantConfiguration_Temperature", Description = "AssistantConfiguration_Temperature_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public double? Temperature { get; set; }

        [Display(Name = "AssistantConfiguration_EnableCommunicationProtocol", Description = "AssistantConfiguration_EnableCommunicationProtocol_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public bool EnableCommunicationProtocol { get; set; }

        [Display(Name = "AssistantConfiguration_EnableAutomaticAdjustment", Description = "AssistantConfiguration_EnableAutomaticAdjustment_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        public bool EnableAutomaticAdjustment { get; set; } = true;

        [Display(Name = "AssistantConfiguration_AttachProjectJsonAutomatically", Description = "AssistantConfiguration_AttachProjectJsonAutomatically_Tooltip", GroupName = "AssistantConfiguration_Groups_General", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        public bool AttachProjectJsonAutomatically { get; set; } = false;

        [Display(Name = "AssistantConfiguration_MaxTokens", Description = "AssistantConfiguration_MaxTokens_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        public int? MaxTokens { get; set; }

        [Display(Name = "AssistantConfiguration_MaxToolRounds", Description = "AssistantConfiguration_MaxToolRounds_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public int MaxToolRounds { get; set; } = 50;

        [Display(Name = "AssistantConfiguration_MaxLatestMessages", Description = "AssistantConfiguration_MaxLatestMessages_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [Range(AssistantConfigurationLimits.MinLatestMessages, AssistantConfigurationLimits.MaxLatestMessages)]
        public int MaxLatestMessages { get; set; } = 10;

        [Display(Name = "AssistantConfiguration_MinLatestMessages", Description = "AssistantConfiguration_MinLatestMessages_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [Range(AssistantConfigurationLimits.MinLatestMessages, AssistantConfigurationLimits.MaxLatestMessages)]
        public int MinLatestMessages { get; set; } = 2;

        [Display(Name = "AssistantConfiguration_SummaryCompactionRate", Description = "AssistantConfiguration_SummaryCompactionRate_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        [Range(AssistantConfigurationLimits.MinSummaryCompactionRate, AssistantConfigurationLimits.MaxSummaryCompactionRate)]
        public double SummaryCompactionRate { get; set; } = 1d;

        [Display(Name = "AssistantConfiguration_MaxContextTokens", Description = "AssistantConfiguration_MaxContextTokens_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        public int MaxContextTokens { get; set; } = 32000;

        [Display(Name = "AssistantConfiguration_ReservedResponseTokens", Description = "AssistantConfiguration_ReservedResponseTokens_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        public int ReservedResponseTokens { get; set; } = 4096;

        [Display(Name = "AssistantConfiguration_ApproximateCharactersPerToken", Description = "AssistantConfiguration_ApproximateCharactersPerToken_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 7)]
        public int ApproximateCharactersPerToken { get; set; } = 4;

        [Display(Name = "AssistantConfiguration_MaxFieldValuesTokensPerResponse", Description = "AssistantConfiguration_MaxFieldValuesTokensPerResponse_Tooltip", GroupName = "AssistantConfiguration_Groups_Extended", ResourceType = typeof(FlowBloxTexts), Order = 8)]
        [Range(1, 20000)]
        public int MaxFieldValuesTokensPerResponse { get; set; } = 4000;
    }
}
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
    }
}

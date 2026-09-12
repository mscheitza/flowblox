using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AWS
{
    public enum AWSCompareFacesQualityFilter
    {
        [Display(Name = "AWSCompareFacesQualityFilter_None", ResourceType = typeof(FlowBloxTexts))]
        None,

        [Display(Name = "AWSCompareFacesQualityFilter_Auto", ResourceType = typeof(FlowBloxTexts))]
        Auto,

        [Display(Name = "AWSCompareFacesQualityFilter_Low", ResourceType = typeof(FlowBloxTexts))]
        Low,

        [Display(Name = "AWSCompareFacesQualityFilter_Medium", ResourceType = typeof(FlowBloxTexts))]
        Medium,

        [Display(Name = "AWSCompareFacesQualityFilter_High", ResourceType = typeof(FlowBloxTexts))]
        High
    }
}

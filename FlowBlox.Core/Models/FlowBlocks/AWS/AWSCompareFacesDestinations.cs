using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AWS
{
    public enum AWSCompareFacesDestinations
    {
        [Display(Name = "AWSCompareFacesDestinations_PersonKey", ResourceType = typeof(FlowBloxTexts))]
        PersonKey,

        [Display(Name = "AWSCompareFacesDestinations_Similarity", ResourceType = typeof(FlowBloxTexts))]
        Similarity,

        [Display(Name = "AWSCompareFacesDestinations_MatchCount", ResourceType = typeof(FlowBloxTexts))]
        MatchCount
    }
}

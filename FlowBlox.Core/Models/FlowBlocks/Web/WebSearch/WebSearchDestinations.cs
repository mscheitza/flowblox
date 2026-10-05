using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Web.WebSearch
{
    public enum WebSearchDestinations
    {
        [Display(Name = "WebSearchDestinations_Title", ResourceType = typeof(FlowBloxTexts))]
        Title,

        [Display(Name = "WebSearchDestinations_Url", ResourceType = typeof(FlowBloxTexts))]
        Url,

        [Display(Name = "WebSearchDestinations_Description", ResourceType = typeof(FlowBloxTexts))]
        Description,

        [Display(Name = "WebSearchDestinations_Age", ResourceType = typeof(FlowBloxTexts))]
        Age,

        [Display(Name = "WebSearchDestinations_PageAge", ResourceType = typeof(FlowBloxTexts))]
        PageAge,

        [Display(Name = "WebSearchDestinations_Language", ResourceType = typeof(FlowBloxTexts))]
        Language,

        [Display(Name = "WebSearchDestinations_Rank", ResourceType = typeof(FlowBloxTexts))]
        Rank,

        [Display(Name = "WebSearchDestinations_Query", ResourceType = typeof(FlowBloxTexts))]
        Query
    }
}

using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Web.WebSearch
{
    public enum WebSearchSafeSearch
    {
        [Display(Name = "WebSearchSafeSearch_Off", ResourceType = typeof(FlowBloxTexts))]
        Off,

        [Display(Name = "WebSearchSafeSearch_Moderate", ResourceType = typeof(FlowBloxTexts))]
        Moderate,

        [Display(Name = "WebSearchSafeSearch_Strict", ResourceType = typeof(FlowBloxTexts))]
        Strict
    }
}

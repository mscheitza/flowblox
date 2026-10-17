using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Xml
{
    public enum XPathResultMode
    {
        [Display(Name = "XPathResultMode_InnerXml", ResourceType = typeof(FlowBloxTexts))]
        InnerXml = 0,

        [Display(Name = "XPathResultMode_OuterXml", ResourceType = typeof(FlowBloxTexts))]
        OuterXml = 1
    }
}

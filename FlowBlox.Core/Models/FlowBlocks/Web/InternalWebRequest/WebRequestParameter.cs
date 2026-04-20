using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Web.InternalWebRequest
{
    public class WebRequestParameter : FlowBloxReactiveObject
    {
        [Display(Name = "WebRequestParameter_Key", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public string Key { get; set; }

        [Display(Name = "WebRequestParameter_Value", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public string Value { get; set; }
    }
}

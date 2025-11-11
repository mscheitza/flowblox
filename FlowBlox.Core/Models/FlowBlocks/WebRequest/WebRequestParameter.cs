using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.WebRequest
{
    public class WebRequestParameter : FlowBloxReactiveObject
    {
        [Display(Name = "WebRequestParameter_Key", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection | UIOptions.FieldSelectionDefaultNotRequired)]
        public string Key { get; set; }

        [Display(Name = "WebRequestParameter_Value", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection | UIOptions.FieldSelectionDefaultNotRequired)]
        public string Value { get; set; }
    }
}

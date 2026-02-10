using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Core.Models.Components.Modifier
{
    [Display(Name = "RegexReplaceModifier", ResourceType = typeof(FlowBloxTexts))]
    public class RegexReplaceModifier : RegexModifierBase
    {
        [Display(Name = "RegexMatchModifier_Replace", Order = 1, ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(ToolboxCategory = FlowBloxToolboxCategory.Regex, UiOptions = UIOptions.EnableFieldSelection)]
        public string Replace { get; set; }

        public override string Modify(BaseRuntime runtime, string value)
        {
            string search = FlowBloxFieldHelper.ReplaceFieldsInString(this.Search);
            string replace = FlowBloxFieldHelper.ReplaceFieldsInString(this.Replace);
            Regex regex = new Regex(search);
            var result = regex.Replace(value, replace);
            return result;
        }

        public override string ToString()
        {
            var format = FlowBloxResourceUtil.GetLocalizedString(nameof(RegexReplaceModifier), nameof(ObjectDisplayName));

            return string.Format(format,
                !string.IsNullOrEmpty(this.Search) ? this.Search : "?",
                !string.IsNullOrEmpty(this.Replace) ? this.Replace : "?");
        }
    }
}

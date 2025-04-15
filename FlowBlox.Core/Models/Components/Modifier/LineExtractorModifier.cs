using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FlowBlox.Core.Models.Components.Modifier
{
    [Display(Name = "LineExtractorModifier", ResourceType = typeof(FlowBloxTexts))]
    public class LineExtractorModifier : ModifierBase
    {
        [Required]
        [Display(Name = "LineExtractorModifier_StartLine", Order = 0, ResourceType = typeof(FlowBloxTexts))]
        public int StartLine { get; set; }

        [Display(Name = "LineExtractorModifier_LineCount", Order = 1, ResourceType = typeof(FlowBloxTexts))]
        public int? LineCount { get; set; }

        public override string Modify(BaseRuntime runtime, string value)
        {
            var lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (LineCount.HasValue)
            {
                return string.Join(Environment.NewLine, lines.Skip(StartLine).Take(LineCount.Value));
            }
            else
            {
                return string.Join(Environment.NewLine, lines.Skip(StartLine));
            }
        }

        public override string ToString()
        {
            var format = FlowBloxResourceUtil.GetLocalizedString(nameof(LineExtractorModifier), nameof(ObjectDisplayName));
            return string.Format(format, this.StartLine, this.LineCount.HasValue ? this.StartLine + this.LineCount.Value : "*");
        }
    }
}

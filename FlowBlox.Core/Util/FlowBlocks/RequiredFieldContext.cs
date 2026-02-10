using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;

namespace FlowBlox.Core.Util.FlowBlocks
{
    public class RequiredFieldContext
    {
        public FieldElement FieldElement { get; set; }

        public FlowBloxComponent FlowBloxComponent { get; set; }
    }
}

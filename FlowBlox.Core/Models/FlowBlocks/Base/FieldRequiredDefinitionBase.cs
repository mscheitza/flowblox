using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public abstract class FieldRequiredDefinitionBase : FlowBloxReactiveObject
    {
        public abstract FieldElement Field { get; set; }

        public abstract bool IsRequired { get; set; }
    }
}

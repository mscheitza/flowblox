using FlowBlox.Core.Models.Components;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public abstract class FieldRequiredDefinitionBase
    {
        public abstract FieldElement Field { get; set; }

        public abstract bool IsRequired { get; set; }
    }
}

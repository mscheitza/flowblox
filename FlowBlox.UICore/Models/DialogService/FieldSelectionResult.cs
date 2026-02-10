using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.UICore.Models.DialogService
{
    public class FieldSelectionResult
    {
        public bool Success { get; set; }
        public List<FieldElement> SelectedFields { get; set; }
        public bool IsRequired { get; set; }
        public BaseFlowBlock Target { get; set; }
    }
}   

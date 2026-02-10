using FlowBlox.Core.Models.Components;

namespace FlowBlox.UICore.Models.DialogService
{
    public class InsertTextOrFieldResult
    {
        public bool Success { get; set; }
        public string InsertedValue { get; set; }
        public bool IsSelectedFieldRequired { get; set; }
        public FieldElement SelectedField { get; set; }
    }
}

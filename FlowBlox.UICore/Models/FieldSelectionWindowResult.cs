using FlowBlox.Core.Models.Components;
using FlowBlox.UICore.Enums;
using System.Collections.Generic;

namespace FlowBlox.UICore.Models
{
    public sealed class FieldSelectionWindowResult
    {
        public FieldSelectionMode SelectionMode { get; set; }

        // Selected project fields (only meaningful when SelectionMode == Fields).
        public List<FieldElement> SelectedFields { get; set; } = new List<FieldElement>();

        // Selected options (only meaningful when SelectionMode == Options).
        public List<OptionElement> SelectedOptions { get; set; } = new List<OptionElement>();

        // Only meaningful when SelectionMode == Fields.
        public bool IsRequired { get; set; }
    }
}

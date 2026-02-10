using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Enums;
using System.Collections.Generic;

namespace FlowBlox.UICore.Models
{
    public sealed class FieldSelectionWindowArgs
    {
        // If provided, connected fields are detected via FlowBlock.ReferencedFlowBlocks and sorted first.
        // If null, connected state is always false.
        public BaseFlowBlock FlowBlock { get; set; }

        // Controls which tab is initially active and which selection result is considered primary.
        public FieldSelectionMode SelectionMode { get; set; } = FieldSelectionMode.Fields;

        // Controls which tabs are supported.
        public FieldSelectionMode[] AllowedFieldSelectionModes { get; set; } = [FieldSelectionMode.Fields, FieldSelectionMode.Options];

        // Applies to the active tab list selection behavior.
        public bool MultiSelect { get; set; } = true;

        // Only relevant for Fields tab: initial value for "Required".
        public bool IsRequired { get; set; } = false;

        // Only relevant for Fields tab: hides the "Required" checkbox completely.
        public bool HideRequired { get; set; } = false;

        // Optional: explicitly provided project field elements. If null, fields are loaded from registry automatically.
        public IEnumerable<FieldElement> FieldElements { get; set; }

        // Optional: explicitly provided option elements. If null, options are loaded from FlowBlockOptions automatically.
        public IEnumerable<OptionElement> OptionElements { get; set; }
    }
}
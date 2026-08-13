using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace FlowBlox.UICore.ViewModels.ComponentLibrary
{
    public sealed class ComponentLibraryNodeViewModel
    {
        public string DisplayName { get; init; }
        public ImageSource Icon { get; init; }
        public BaseFlowBlock FlowBlock { get; init; }
        public FlowBlockCategory Category { get; init; }
        public ObservableCollection<ComponentLibraryNodeViewModel> Children { get; } = new();
        public bool IsFlowBlock => FlowBlock != null;
        public bool IsCategory => Category != null;
        public bool IsExpanded { get; set; } = true;
    }
}

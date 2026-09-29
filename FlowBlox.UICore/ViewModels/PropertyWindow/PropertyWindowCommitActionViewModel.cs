using MahApps.Metro.IconPacks;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels.PropertyWindow
{
    public sealed class PropertyWindowCommitActionViewModel
    {
        public required string OptionValue { get; init; }

        public required string DisplayName { get; init; }

        public required PackIconMaterialKind IconKind { get; init; }

        public required string IconColor { get; init; }

        public required ICommand Command { get; init; }
    }
}

using MahApps.Metro.IconPacks;

namespace FlowBlox.UICore.ViewModels.PropertyWindow
{
    public sealed class PropertyWindowSpecialExplanationEntryViewModel
    {
        public string Explanation { get; init; } = string.Empty;

        public PackIconMaterialKind IconKind { get; init; } = PackIconMaterialKind.InformationOutline;

        public string IconForeground { get; init; } = "#3A6EA5";
    }
}

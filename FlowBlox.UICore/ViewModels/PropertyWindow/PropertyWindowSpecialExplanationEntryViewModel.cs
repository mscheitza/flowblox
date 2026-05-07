using FlowBlox.UICore.Commands;
using MahApps.Metro.IconPacks;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels.PropertyWindow
{
    public sealed class PropertyWindowSpecialExplanationEntryViewModel
    {
        private const string ContinueMarker = "$$CONTINUE$$";

        public string Explanation { get; init; } = string.Empty;
        public bool HasContinuationMarker => (Explanation ?? string.Empty).Contains(ContinueMarker, StringComparison.Ordinal);

        public string DisplayText
        {
            get
            {
                var text = Explanation ?? string.Empty;
                var markerIndex = text.IndexOf(ContinueMarker, StringComparison.Ordinal);
                if (markerIndex < 0)
                    return text;

                return text[..markerIndex].TrimEnd();
            }
        }

        public string FullText => (Explanation ?? string.Empty)
            .Replace(ContinueMarker, string.Empty, StringComparison.Ordinal)
            .Trim();

        public ICommand OpenExplanationCommand { get; init; } = new RelayCommand(_ => { });

        public PackIconMaterialKind IconKind { get; init; } = PackIconMaterialKind.InformationOutline;

        public string IconForeground { get; init; } = "#3A6EA5";
    }
}

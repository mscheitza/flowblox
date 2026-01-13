using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels.PropertyWindow;
using SkiaSharp;
using System.Text.RegularExpressions;

namespace FlowBlox.UICore.Provider
{
    public class WpfUIActionsProvider : UIActionsProviderBase<UIActionViewModel>
    {
        private static readonly Regex RemoveAmpersandRegex = new Regex(@"&(?=\w)", RegexOptions.Compiled);

        private static string RemoveMnemonicAmpersand(string input)
            => string.IsNullOrEmpty(input) ? input : RemoveAmpersandRegex.Replace(input, string.Empty);

        protected override UIActionViewModel CreateItem(string displayName, EventHandler clickHandler, bool enabled, SKImage icon16)
        {
            return new UIActionViewModel
            {
                DisplayName = RemoveMnemonicAmpersand(displayName),
                IsEnabled = enabled,
                Icon = icon16 != null ? 
                    SkiaWpfImageHelper.ConvertToImageSource(icon16) : 
                    WpfIconHelper.CreateMaterialIcon(MahApps.Metro.IconPacks.PackIconMaterialKind.CogOutline, 16),
                Command = new RelayCommand(_ => clickHandler.Invoke(null, EventArgs.Empty), _ => enabled)
            };
        }
    }
}

using FlowBlox.UICore.Commands;
using FlowBlox.UICore.ViewModels.PropertyWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Provider
{
    public class WpfUIActionsProvider : UIActionsProviderBase<UIActionViewModel>
    {
        private static readonly Regex RemoveAmpersandRegex = new Regex(@"&(?=\w)", RegexOptions.Compiled);

        private static string RemoveMnemonicAmpersand(string input)
        {
            return string.IsNullOrEmpty(input) ?
                input :
                RemoveAmpersandRegex.Replace(input, string.Empty);
        }

        protected override UIActionViewModel CreateItem(string displayName, EventHandler clickHandler, bool enabled)
        {
            return new UIActionViewModel
            {
                DisplayName = RemoveMnemonicAmpersand(displayName),
                IsEnabled = enabled,
                Command = new RelayCommand(_ => clickHandler.Invoke(null, EventArgs.Empty), _ => enabled)
            };
        }
    }
}

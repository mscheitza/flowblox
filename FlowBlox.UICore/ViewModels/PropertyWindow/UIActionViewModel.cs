using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.ViewModels.PropertyWindow
{
    public class UIActionViewModel
    {
        public string DisplayName { get; set; }
        public ICommand Command { get; set; }
        public bool IsEnabled { get; set; } = true;
        public ImageSource Icon { get; set; }
        public bool HasIcon => Icon != null;
    }
}

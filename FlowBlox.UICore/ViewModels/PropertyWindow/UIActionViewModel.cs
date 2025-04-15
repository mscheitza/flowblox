using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels.PropertyWindow
{
    public class UIActionViewModel
    {
        public string DisplayName { get; set; }
        public ICommand Command { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}

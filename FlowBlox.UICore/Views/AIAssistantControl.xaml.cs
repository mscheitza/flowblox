using System.Windows.Controls;

namespace FlowBlox.UICore.Views
{
    public partial class AIAssistantControl : UserControl
    {
        public event EventHandler? ConfigurationRequested;

        public AIAssistantControl()
        {
            InitializeComponent();
            ChatView.ConfigurationRequested += (_, _) => ConfigurationRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

using FlowBlox.UICore.Views;
using System.Windows;

namespace FlowBlox.UICore.Factory
{
    public static class TransactionMonitorWindowFactory
    {
        private static TransactionMonitorWindow _window;

        public static void Show()
        {
            if (_window == null)
            {
                _window = new TransactionMonitorWindow();
                _window.Closed += (_, _) => _window = null;
                _window.Show();
            }

            if (_window.WindowState == WindowState.Minimized)
                _window.WindowState = WindowState.Normal;

            _window.Activate();
        }
    }
}

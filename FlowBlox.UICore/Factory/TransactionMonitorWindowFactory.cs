using FlowBlox.UICore.Views;
using System.Windows;
using System.Windows.Input;

namespace FlowBlox.UICore.Factory
{
    public static class TransactionMonitorWindowFactory
    {
        private static TransactionMonitorWindow _window;

        public static void Register(Window window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            window.PreviewKeyDown += Window_PreviewKeyDown;
            window.Closed += Window_Closed;
        }

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

        private static void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.T ||
                Keyboard.Modifiers != (ModifierKeys.Control | ModifierKeys.Alt))
            {
                return;
            }

            Show();
            e.Handled = true;
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            if (sender is not Window window)
                return;

            window.PreviewKeyDown -= Window_PreviewKeyDown;
            window.Closed -= Window_Closed;
        }
    }
}

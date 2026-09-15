using System.Windows.Input;
using System.Windows.Threading;

namespace FlowBlox.UICore.Views
{
    internal sealed class ProjectPanelTemporaryConnectionShortcut
    {
        private readonly Action _deactivate;
        private readonly DispatcherTimer _releaseTimer;

        public ProjectPanelTemporaryConnectionShortcut(Action deactivate)
        {
            _deactivate = deactivate;
            _releaseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _releaseTimer.Tick += ReleaseTimer_Tick;
        }

        public void StartReleaseWatcher()
            => _releaseTimer.Start();

        public void StopReleaseWatcher()
            => _releaseTimer.Stop();

        private void ReleaseTimer_Tick(object sender, EventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                return;
            }

            StopReleaseWatcher();
            _deactivate();
        }
    }
}

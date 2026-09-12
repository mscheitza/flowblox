using System.Windows.Input;
using System.Windows.Threading;

namespace FlowBlox.UICore.Views
{
    internal sealed class ProjectPanelTemporaryConnectionShortcut
    {
        private readonly Func<bool> _isContextActive;
        private readonly Func<bool> _isTextInputFocusActive;
        private readonly Func<bool> _canEnterConnectionMode;
        private readonly Action<bool> _setTemporaryConnectionMode;
        private readonly DispatcherTimer _releaseTimer;

        public ProjectPanelTemporaryConnectionShortcut(
            Func<bool> isContextActive,
            Func<bool> isTextInputFocusActive,
            Func<bool> canEnterConnectionMode,
            Action<bool> setTemporaryConnectionMode)
        {
            _isContextActive = isContextActive;
            _isTextInputFocusActive = isTextInputFocusActive;
            _canEnterConnectionMode = canEnterConnectionMode;
            _setTemporaryConnectionMode = setTemporaryConnectionMode;
            _releaseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _releaseTimer.Tick += ReleaseTimer_Tick;
        }

        public void Update()
            => Update(_isContextActive(), IsShortcutPressed());

        public void Update(bool hostContextActive, bool shortcutActive)
        {
            var isShortcutActive =
                hostContextActive &&
                !_isTextInputFocusActive() &&
                shortcutActive &&
                _canEnterConnectionMode();

            _setTemporaryConnectionMode(isShortcutActive);
            if (isShortcutActive)
                _releaseTimer.Start();
            else
                _releaseTimer.Stop();
        }

        public void Clear()
        {
            _releaseTimer.Stop();
            _setTemporaryConnectionMode(false);
        }

        private void ReleaseTimer_Tick(object sender, EventArgs e)
        {
            if (!IsShortcutPressed())
                Clear();
        }

        private static bool IsShortcutPressed()
            => Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
               Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
    }
}

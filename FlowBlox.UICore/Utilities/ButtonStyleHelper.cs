using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowBlox.UICore.Utilities
{
    public static class ButtonStyleHelper
    {
        /// <summary>
        /// Sets a dynamic style on the given button to change Foreground depending on IsEnabled.
        /// </summary>
        /// <param name="button">The target Button</param>
        /// <param name="enabledBrush">Foreground color when enabled</param>
        /// <param name="disabledBrush">Foreground color when disabled</param>
        public static void SetButtonStyle(Button button, Brush enabledBrush, Brush disabledBrush)
        {
            var style = new Style(typeof(Button), button.Style);

            style.Setters.Add(new Setter(Control.ForegroundProperty, enabledBrush));

            var trigger = new Trigger
            {
                Property = UIElement.IsEnabledProperty,
                Value = false
            };
            trigger.Setters.Add(new Setter(Control.ForegroundProperty, disabledBrush));

            style.Triggers.Add(trigger);

            button.Style = style;
        }
    }
}

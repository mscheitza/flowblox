using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowBlox.UICore.Behavior
{
    public static class ToolTipPreferenceBehavior
    {
        public static readonly DependencyProperty PreferChildToolTipsProperty =
            DependencyProperty.RegisterAttached(
                "PreferChildToolTips",
                typeof(bool),
                typeof(ToolTipPreferenceBehavior),
                new PropertyMetadata(false, OnPreferChildToolTipsChanged));

        public static void SetPreferChildToolTips(DependencyObject element, bool value) =>
            element.SetValue(PreferChildToolTipsProperty, value);

        public static bool GetPreferChildToolTips(DependencyObject element) =>
            (bool)element.GetValue(PreferChildToolTipsProperty);

        private static void OnPreferChildToolTipsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                if ((bool)e.NewValue) fe.ToolTipOpening += OnToolTipOpening;
                else fe.ToolTipOpening -= OnToolTipOpening;
            }
        }

        private static void OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (sender is not FrameworkElement container) 
                return;

            DependencyObject current = e.OriginalSource as DependencyObject;
            while (current != null && current != container)
            {
                if (current is FrameworkElement fe &&
                    fe.ReadLocalValue(FrameworkElement.ToolTipProperty) != DependencyProperty.UnsetValue)
                {
                    e.Handled = true;
                    return;
                }

                current = VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
            }
        }
    }

}

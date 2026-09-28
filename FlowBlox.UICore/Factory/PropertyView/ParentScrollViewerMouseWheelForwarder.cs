using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView
{
    internal static class ParentScrollViewerMouseWheelForwarder
    {
        private const double HorizontalScrollStep = 48;

        public static void Register(
            UIElement element,
            Func<double> getHorizontalOffset,
            Action<double> scrollToHorizontalOffset)
        {
            if (element == null)
                return;
            if (getHorizontalOffset == null)
                throw new ArgumentNullException(nameof(getHorizontalOffset));
            if (scrollToHorizontalOffset == null)
                throw new ArgumentNullException(nameof(scrollToHorizontalOffset));

            element.AddHandler(
                UIElement.PreviewMouseWheelEvent,
                new MouseWheelEventHandler((sender, e) => HandleMouseWheel(
                    sender,
                    e,
                    getHorizontalOffset,
                    scrollToHorizontalOffset)),
                true);
        }

        private static void HandleMouseWheel(
            object sender,
            MouseWheelEventArgs e,
            Func<double> getHorizontalOffset,
            Action<double> scrollToHorizontalOffset)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                var wheelSteps = e.Delta / Mouse.MouseWheelDeltaForOneLine;
                scrollToHorizontalOffset(getHorizontalOffset() - wheelSteps * HorizontalScrollStep);
                e.Handled = true;
                return;
            }

            if (sender is not DependencyObject dependencyObject)
                return;

            var parentScrollViewer = FindAncestorScrollViewer(dependencyObject);
            if (parentScrollViewer == null)
                return;

            e.Handled = true;
            parentScrollViewer.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            });
        }

        private static ScrollViewer? FindAncestorScrollViewer(DependencyObject start)
        {
            var current = VisualTreeHelper.GetParent(start);
            while (current != null)
            {
                if (current is ScrollViewer scrollViewer)
                    return scrollViewer;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}

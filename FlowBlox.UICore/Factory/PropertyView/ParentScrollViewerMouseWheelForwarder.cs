using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView
{
    internal static class ParentScrollViewerMouseWheelForwarder
    {
        public static void Register(UIElement element)
        {
            if (element == null)
                return;

            element.AddHandler(
                UIElement.PreviewMouseWheelEvent,
                new MouseWheelEventHandler(ForwardMouseWheelToParent),
                true);
        }

        private static void ForwardMouseWheelToParent(object sender, MouseWheelEventArgs e)
        {
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

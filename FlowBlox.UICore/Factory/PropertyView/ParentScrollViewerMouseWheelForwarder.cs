using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView
{
    internal static class ParentScrollViewerMouseWheelForwarder
    {
        private const double HorizontalScrollStep = 48;

        public static void Register(UIElement element)
        {
            if (element == null)
                return;

            var scrollViewerState = new ScrollViewerResolutionState();
            RegisterScrollViewerResolution(element, scrollViewerState);

            element.AddHandler(
                UIElement.PreviewMouseWheelEvent,
                new MouseWheelEventHandler((sender, e) =>
                {
                    if (!scrollViewerState.IsResolved)
                        ResolveScrollViewer(element, scrollViewerState);

                    HandleMouseWheel(sender, e, scrollViewerState);
                }),
                true);
        }

        private static void RegisterScrollViewerResolution(
            UIElement element,
            ScrollViewerResolutionState state)
        {
            if (element is not FrameworkElement frameworkElement)
                return;

            frameworkElement.Loaded += (_, _) => ResolveScrollViewer(element, state);
            frameworkElement.Unloaded += (_, _) => state.Reset();
        }

        private static void ResolveScrollViewer(
            UIElement element,
            ScrollViewerResolutionState state)
        {
            if (element is FrameworkElement frameworkElement)
                frameworkElement.ApplyTemplate();

            state.Resolve(FindDescendantScrollViewer(element));
        }

        private static void HandleMouseWheel(
            object sender,
            MouseWheelEventArgs e,
            ScrollViewerResolutionState state)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (state.ScrollViewer == null)
                    return;

                var wheelSteps = e.Delta / Mouse.MouseWheelDeltaForOneLine;
                state.ScrollViewer.ScrollToHorizontalOffset(
                    state.ScrollViewer.HorizontalOffset - wheelSteps * HorizontalScrollStep);
                e.Handled = true;
                return;
            }

            // Let controls with their own vertical scroll range process the wheel normally.
            if (state.ScrollViewer?.ScrollableHeight > 0)
                return;

            if (sender is not DependencyObject dependencyObject)
                return;

            if (!state.IsParentResolved)
                state.ResolveParent(FindAncestorScrollViewer(dependencyObject));

            var parentScrollViewer = state.ParentScrollViewer;
            if (parentScrollViewer == null)
                return;

            e.Handled = true;
            parentScrollViewer.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            });
        }

        private static ScrollViewer? FindDescendantScrollViewer(DependencyObject start)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(start);
            for (var index = 0; index < childCount; index++)
            {
                var child = VisualTreeHelper.GetChild(start, index);
                if (child is ScrollViewer scrollViewer)
                    return scrollViewer;

                var descendant = FindDescendantScrollViewer(child);
                if (descendant != null)
                    return descendant;
            }

            return null;
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

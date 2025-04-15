using System.Windows;
using System.Windows.Media;

namespace FlowBlox.UICore.Utilities
{
    public static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// Recursively searches the visual tree for the first child of a given type.
        /// </summary>
        /// <typeparam name="T">The type of the child to find (e.g., TextBox, TabControl).</typeparam>
        /// <param name="parent">The parent element to start the search from.</param>
        /// <returns>The first child of type T, or null if not found.</returns>
        public static T FindFirstChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T match)
                    return match;

                var result = FindFirstChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Recursively searches the visual tree for a child of a given type and (optionally) name.
        /// </summary>
        /// <typeparam name="T">The type of the child to find (e.g., TextBox, TabControl).</typeparam>
        /// <param name="parent">The parent element to start the search from.</param>
        /// <param name="childName">Optional: the name of the child element to match.</param>
        /// <returns>The matching child element, or null if not found.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName = null) where T : DependencyObject
        {
            if (parent == null)
                return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    if (string.IsNullOrEmpty(childName) ||
                        (child is FrameworkElement fe && fe.Name == childName))
                    {
                        return typedChild;
                    }
                }

                var result = FindChild<T>(child, childName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
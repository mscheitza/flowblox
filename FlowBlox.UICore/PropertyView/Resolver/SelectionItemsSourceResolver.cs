using System.Collections;
using System.Reflection;

namespace FlowBlox.UICore.PropertyView.Resolver
{
    public static class SelectionItemsSourceResolver
    {
        public static IList ResolveItemsSource(
            SelectionMethodResolutionResult selectionMethodResolution,
            PropertyInfo property)
        {
            if (selectionMethodResolution?.Method == null)
                return null;

            var originalItems = selectionMethodResolution.Method.Invoke(selectionMethodResolution.InvocationTarget, null) as IList;
            if (originalItems == null || IsRequired(property))
                return originalItems;

            var items = Activator.CreateInstance(originalItems.GetType()) as IList;
            if (items == null)
                return originalItems;

            items.Add(null);

            foreach (var item in originalItems)
                items.Add(item);

            return items;
        }

        private static bool IsRequired(PropertyInfo property)
        {
            return property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null;
        }
    }
}

using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.DeepCopier;
using System.Collections;
using System.Reflection;

namespace FlowBlox.Core.Util.FlowBlocks
{
    internal static class FlowBloxObjectGraphVisitor
    {
        internal static void VisitPropertiesRecursive(
            IEnumerable<object> roots,
            Action<object, object, PropertyInfo, object> visitor)
        {
            if (roots == null)
                throw new ArgumentNullException(nameof(roots));
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));

            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var root in roots.Where(x => x != null))
                VisitPropertiesRecursive(root, root, visitor, visited);
        }

        private static void VisitPropertiesRecursive(
            object root,
            object instance,
            Action<object, object, PropertyInfo, object> visitor,
            ISet<object> visited)
        {
            if (!visited.Add(instance))
                return;

            var properties = instance.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.CanWrite && x.GetIndexParameters().Length == 0)
                .Where(x => x.GetCustomAttribute<DeepCopierIgnoreAttribute>() == null);

            foreach (var property in properties)
            {
                object value;
                try
                {
                    value = property.GetValue(instance);
                }
                catch
                {
                    continue;
                }

                if (value == null)
                    continue;

                visitor(root, instance, property, value);

                if (value is FlowBloxReactiveObject nested &&
                    FlowBloxReactiveObjectHelper.IsReactiveObjectOnly(nested))
                {
                    VisitPropertiesRecursive(root, nested, visitor, visited);
                    continue;
                }

                if (value is IEnumerable enumerable && value is not string)
                {
                    foreach (var item in enumerable
                                 .OfType<FlowBloxReactiveObject>()
                                 .Where(FlowBloxReactiveObjectHelper.IsReactiveObjectOnly))
                    {
                        VisitPropertiesRecursive(root, item, visitor, visited);
                    }
                }
            }
        }
    }
}

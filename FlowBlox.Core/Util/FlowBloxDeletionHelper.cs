using System.Collections;
using System.Reflection;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.Util
{
    public static class FlowBloxDeletionHelper
    {
        public static void DeleteInstance(FlowBloxRegistry registry, object instance)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            if (instance == null)
                return;

            var deletedInstances = new HashSet<object>(ReferenceEqualityComparer.Instance);
            DeleteInstanceRecursive(registry, instance, deletedInstances);
        }

        public static bool IsDeletable(object instance, out List<IFlowBloxComponent> dependencies)
        {
            dependencies = new List<IFlowBloxComponent>();

            if (instance == null)
                return true;

            var methodInfo = instance.GetType().GetMethod(GlobalConstants.IsDeletableMethodName);
            if (methodInfo == null)
                return true;

            var parameters = methodInfo.GetParameters();
            if (methodInfo.ReturnType != typeof(bool)
                || parameters.Length != 1
                || parameters[0].ParameterType != typeof(List<IFlowBloxComponent>).MakeByRefType())
            {
                return true;
            }

            var arguments = new object[] { null! };
            var isDeletable = (bool)methodInfo.Invoke(instance, arguments)!;
            dependencies = (List<IFlowBloxComponent>?)arguments[0] ?? new List<IFlowBloxComponent>();

            return isDeletable;
        }

        private static void DeleteInstanceRecursive(
            FlowBloxRegistry registry,
            object instance,
            ISet<object> deletedInstances)
        {
            if (instance == null || !deletedInstances.Add(instance))
                return;

            if (IsNestedRowDeleteScope(instance))
            {
                foreach (var nestedManagedObject in GetNestedManagedObjects(instance))
                {
                    DeleteInstanceRecursive(registry, nestedManagedObject, deletedInstances);
                }
            }

            registry.Unregister(instance);
        }

        private static bool IsNestedRowDeleteScope(object instance)
        {
            return instance is FlowBloxReactiveObject
                   && instance is not FlowBloxComponent;
        }

        private static IEnumerable<IManagedObject> GetNestedManagedObjects(object instance)
        {
            var instanceType = instance.GetType();
            var properties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                    continue;

                var uiAttribute = property.GetCustomAttribute<FlowBloxUIAttribute>();
                if (!IsRowManagedAssociation(uiAttribute))
                    continue;

                var propertyValue = property.GetValue(instance);
                if (propertyValue is IManagedObject managedObject)
                {
                    yield return managedObject;
                    continue;
                }

                if (propertyValue is IEnumerable enumerable && property.PropertyType != typeof(string))
                {
                    foreach (var childManagedObject in enumerable.OfType<IManagedObject>())
                    {
                        yield return childManagedObject;
                    }
                }
            }
        }

        private static bool IsRowManagedAssociation(FlowBloxUIAttribute? uiAttribute)
        {
            if (uiAttribute?.Factory != UIFactory.Association)
                return false;

            var operations = uiAttribute.Operations;
            var supportsCreate = operations.HasFlag(UIOperations.Create);
            var supportsDelete = operations.HasFlag(UIOperations.Delete);
            var supportsLink = operations.HasFlag(UIOperations.Link);
            var supportsUnlink = operations.HasFlag(UIOperations.Unlink);

            return supportsCreate && supportsDelete && !supportsLink && !supportsUnlink;
        }
    }
}

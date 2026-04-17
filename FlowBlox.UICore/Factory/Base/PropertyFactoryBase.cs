using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using System.Collections;
using System.Reflection;

namespace FlowBlox.UICore.Factory.Base
{
    public abstract class PropertyViewControlFactoryBase<TOwner>
    {
        protected readonly object _target;
        protected readonly PropertyInfo _property;
        protected readonly FlowBloxUIAttribute _uiAttribute;
        protected readonly FlowBloxRegistry _registry;
        protected readonly bool _readOnly;

        protected PropertyViewControlFactoryBase(PropertyInfo property, object target, bool readOnly)
        {
            _property = property;
            _uiAttribute = property.GetCustomAttribute<FlowBloxUIAttribute>();
            _target = target;
            _registry = FlowBloxRegistryProvider.GetRegistry();
            _readOnly = readOnly;
        }

        protected object CreateNewInstance(TOwner owner) => CreateNewInstance(owner, _property.PropertyType);

        protected object CreateNewInstance(TOwner owner, Type propertyType)
        {
            IList<Type> types;
            if (propertyType.IsInterface || propertyType.IsAbstract)
            {
                if (_uiAttribute?.CreatableTypes != null && _uiAttribute.CreatableTypes.Length > 0)
                {
                    types = FlowBloxSupportedTypesResolver.ResolveSupportedTypes(
                        _target,
                        propertyType,
                        _uiAttribute.CreatableTypes);
                }
                else
                {
                    types = FlowBloxSupportedTypesResolver.ResolveSupportedTypes(_target, propertyType);
                }   
            }
            else
            {
                types = new List<Type> { propertyType };
            }

            return CreateNewInstance(owner, propertyType, types);
        }

        protected object CreateNewInstance(TOwner owner, Type propertyType, IList<Type> types)
        {
            object newInstance;
            if (types.Count > 1)
            {
                var selectedType = ShowTypeSelectionDialog(owner, types);
                if (selectedType != null)
                    newInstance = CreateNewInstance(selectedType);
                else
                    return null;
            }
            else if (types.Count == 1)
            {
                newInstance = CreateNewInstance(types.Single());
            }
            else
            {
                throw new InvalidOperationException("No createable type could be determined.");
            }

            if (newInstance is IManagedObject managedObject)
            {
                _registry.PostProcessManagedObjectCreated(managedObject);
            }

            _registry.Register(newInstance);

            return newInstance;
        }

        protected virtual object CreateNewInstance(Type type)
        {
            ConstructorInfo constructor = type.GetConstructor([_target.GetType()]);
            object instance;
            if (constructor != null)
            {
                instance = constructor.Invoke([_target]);
            }
            else
            {
                constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                    instance = constructor.Invoke(null);
                else
                    throw new InvalidOperationException("No suitable constructor found for type " + type.FullName);
            }

            return instance;
        }

        protected virtual void DeleteInstance(object instance)
        {
            if (instance == null)
                return;

            var deletedInstances = new HashSet<object>(ReferenceEqualityComparer.Instance);
            DeleteInstanceRecursive(instance, deletedInstances);
        }

        private void DeleteInstanceRecursive(object instance, ISet<object> deletedInstances)
        {
            if (instance == null || !deletedInstances.Add(instance))
                return;

            if (IsNestedRowDeleteScope(instance))
            {
                foreach (var nestedManagedObject in GetNestedManagedObjects(instance))
                {
                    DeleteInstanceRecursive(nestedManagedObject, deletedInstances);
                }
            }

            _registry.Unregister(instance);
        }

        private static bool IsNestedRowDeleteScope(object instance)
        {
            return instance is FlowBloxReactiveObject && 
                   instance is not FlowBloxComponent;
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

                object propertyValue = property.GetValue(instance);
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

        private static bool IsRowManagedAssociation(FlowBloxUIAttribute uiAttribute)
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

        public async Task<bool> IsDeletableAsync(object instance, TOwner owner, object excludeTarget = null)
        {
            var methodInfo = instance.GetType().GetMethod(GlobalConstants.IsDeletableMethodName);
            if (methodInfo != null)
            {
                var parameters = methodInfo.GetParameters();
                if (methodInfo.ReturnType == typeof(bool) &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType == typeof(List<IFlowBloxComponent>).MakeByRefType())
                {
                    var arguments = new object[] { null };
                    bool isDeletable = (bool)methodInfo.Invoke(instance, arguments);
                    var dependencies = (List<IFlowBloxComponent>)arguments[0];

                    if (!isDeletable && dependencies != null)
                    {
                        var excludedTargets = new HashSet<object>(ReferenceEqualityComparer.Instance)
                        {
                            instance
                        };

                        if (_target != null)
                            excludedTargets.Add(_target);

                        if (excludeTarget != null)
                            excludedTargets.Add(excludeTarget);

                        var allReferences = new List<string>();
                        foreach (var dependency in dependencies.Where(x => !excludedTargets.Contains(x)))
                        {
                            allReferences.Add(string.Format(
                                FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Message_Entry"),
                                instance,
                                dependency));
                        }

                        if (allReferences.Any())
                        {
                            await ShowDependencyViolationDialogAsync(owner, allReferences, dependencies);
                            return false;
                        }
                    }

                    return true;
                }
            }

            return true;
        }

        public MethodInfo GetSelectionFilterMethod(object target, string methodName, Type listType = null)
        {
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentNullException(nameof(methodName), "No selection method was passed.");

            MethodInfo filterMethod;

            // Check instance methods first
            var targetType = target.GetType();
            filterMethod = targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (filterMethod != null)
                return filterMethod;

            // Check static methods next
            filterMethod = listType?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            return filterMethod;
        }

        public virtual void Reload()
        {
        }

        protected abstract Type ShowTypeSelectionDialog(TOwner owner, IList<Type> types);

        protected abstract Task ShowDependencyViolationDialogAsync(TOwner owner, List<string> allReferences, List<IFlowBloxComponent> dependencies);
    }
}

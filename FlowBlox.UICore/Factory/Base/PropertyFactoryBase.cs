using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
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
            FlowBloxDeletionHelper.DeleteInstance(_registry, instance);
        }

        public async Task<bool> IsDeletableAsync(object instance, TOwner owner, object excludeTarget = null)
        {
            var isDeletable = FlowBloxDeletionHelper.IsDeletable(instance, out var dependencies);
            if (isDeletable || dependencies == null)
                return true;

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

            if (!allReferences.Any())
                return true;

            await ShowDependencyViolationDialogAsync(owner, allReferences, dependencies);
            return false;
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

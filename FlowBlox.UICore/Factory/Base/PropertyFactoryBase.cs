using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Factory.Base
{
    public abstract class PropertyViewControlFactoryBase<TOwner>
    {
        protected readonly object _target;
        protected readonly PropertyInfo _property;
        protected readonly FlowBlockUIAttribute _flowBlockUIAttribute;
        protected readonly FlowBloxRegistry _registry;
        protected readonly bool _readOnly;

        protected PropertyViewControlFactoryBase(PropertyInfo property, object target, bool readOnly)
        {
            _property = property;
            _flowBlockUIAttribute = property.GetCustomAttribute<FlowBlockUIAttribute>();
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
                types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => propertyType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    .Where(t =>
                    {
                        var attr = t.GetCustomAttribute<FlowBloxSupportedTypesAttribute>();
                        return attr == null || attr.SupportedTypes.Contains(_target.GetType());
                    })
                    .ToList();
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
            ConstructorInfo constructor = type.GetConstructor(new[] { _target.GetType() });
            object instance;

            if (constructor != null)
            {
                instance = constructor.Invoke(new[] { _target });
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
            _registry.Unregister(instance);
        }

        public bool IsDeletable(object instance, TOwner owner)
        {
            return Task.Run(async () => await IsDeletableAsync(instance, owner)).GetAwaiter().GetResult();
        }

        public async Task<bool> IsDeletableAsync(object instance, TOwner owner)
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
                        var allReferences = new List<string>();
                        foreach (var dependency in dependencies.Where(x => x != _target))
                        {
                            allReferences.Add(string.Format(
                                FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Message_Entry"),
                                FlowBloxComponentHelper.GetDisplayName(instance),
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

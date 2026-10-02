using System.Reflection;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Attributes;
using FlowBlox.UICore.Enums;
using SkiaSharp;
using FlowBlox.UICore.Interfaces;
using UIActionsProviderBaseResources = FlowBlox.UICore.Resources.ClassResources.UIActionsProviderBase;

namespace FlowBlox.UICore.Provider
{
    public abstract class UIActionsProviderBase<TItem>
    {
        protected abstract TItem CreateItem(string displayName, EventHandler clickHandler, bool enabled, SKImage icon16);

        public List<TItem> GetToolStripItemsForComponent<T>(T component, bool includePropertyWindowOnlyActions = true) where T : IFlowBloxComponent
        {
            var items = new List<TItem>();
            var componentType = component.GetType();

            var actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(ComponentUIActions<>))
                .Where(t => t.BaseType.GetGenericArguments()[0].IsAssignableFrom(componentType));

            foreach (var type in actionTypes)
            {
                var constructor = type.GetConstructors()
                    .FirstOrDefault(c =>
                    {
                        var parameters = c.GetParameters();
                        return parameters.Length == 1 &&
                               parameters[0].ParameterType.IsAssignableFrom(componentType);
                    });
                if (constructor == null)
                    continue;

                var instance = constructor.Invoke([component]);

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0);

                foreach (var method in methods)
                {
                    var displayAttribute = method.GetCustomAttribute<DisplayAttribute>();
                    if (displayAttribute == null)
                        continue;

                    var actionMetadata = method.GetCustomAttribute<UIActionMetadataAttribute>();
                    if (!includePropertyWindowOnlyActions && actionMetadata?.OnlyShowInPropertyWindow == true)
                        continue;

                    var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute);
                    if (string.IsNullOrEmpty(displayName))
                        continue;

                    var visibilityMethod = type.GetMethod($"Is{method.Name}Visible", BindingFlags.Public | BindingFlags.Instance);
                    var isVisible = visibilityMethod == null || visibilityMethod.ReturnType != typeof(bool) ||
                        (bool)visibilityMethod.Invoke(instance, null);
                    if (!isVisible)
                        continue;

                    var canExecuteMethod = type.GetMethod($"Can{method.Name}", BindingFlags.Public | BindingFlags.Instance);
                    var enabled = canExecuteMethod != null && canExecuteMethod.ReturnType == typeof(bool)
                        ? (bool)canExecuteMethod.Invoke(instance, null)
                        : true;

                    var icon16 = TryGetIcon(instance, type, $"{method.Name}Icon16");

                    var item = CreateItem(
                        displayName,
                        (sender, e) => ExecuteAction(method, instance),
                        enabled,
                        icon16);
                    items.Add(item);
                }
            }

            return items;
        }

        private static void ExecuteAction(MethodInfo method, object instance)
        {
            try
            {
                method.Invoke(instance, null);
            }
            catch (Exception ex)
            {
                var actionException = ex is TargetInvocationException { InnerException: not null }
                    ? ex.InnerException
                    : ex;
                var actionName = $"{method.DeclaringType?.FullName}.{method.Name}";

                FlowBloxLogManager.Instance.GetLogger().Error(
                    $"An error occurred while executing the UI action '{actionName}'.",
                    actionException);

                FlowBloxServiceLocator.Instance
                    .GetService<IFlowBloxMessageBoxService>()
                    ?.ShowMessageBox(
                        FlowBloxResourceUtil.GetLocalizedString(
                            nameof(UIActionsProviderBaseResources.ActionExecutionFailedMessage),
                            typeof(UIActionsProviderBaseResources)),
                        FlowBloxResourceUtil.GetLocalizedString(
                            nameof(UIActionsProviderBaseResources.ActionExecutionFailedTitle),
                            typeof(UIActionsProviderBaseResources)),
                        FlowBloxMessageBoxTypes.Error);
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        private static SKImage TryGetIcon(object instance, Type type, string propertyName)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return null;

            if (!typeof(SKImage).IsAssignableFrom(prop.PropertyType))
                return null;

            try
            {
                return prop.GetValue(instance) as SKImage;
            }
            catch
            {
                return null;
            }
        }
    }
}

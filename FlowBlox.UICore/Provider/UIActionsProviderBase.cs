using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using FlowBlox.UICore.Interfaces;

namespace FlowBlox.UICore.Provider
{
    public abstract class UIActionsProviderBase<TItem>
    {
        protected abstract TItem CreateItem(string displayName, EventHandler clickHandler, bool enabled, SKImage icon16);

        public List<TItem> GetToolStripItemsForComponent<T>(T component) where T : IFlowBloxComponent
        {
            var items = new List<TItem>();
            var componentType = component.GetType();

            var actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(ComponentUIActions<>))
                .Where(t => t.BaseType.GetGenericArguments()[0].IsAssignableFrom(componentType));

            foreach (var type in actionTypes)
            {
                var constructor = type.GetConstructor([component.GetType()]);
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

                    var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute);
                    if (string.IsNullOrEmpty(displayName))
                        continue;

                    var canExecuteMethod = type.GetMethod($"Can{method.Name}", BindingFlags.Public | BindingFlags.Instance);
                    var enabled = canExecuteMethod != null && canExecuteMethod.ReturnType == typeof(bool)
                        ? (bool)canExecuteMethod.Invoke(instance, null)
                        : true;

                    var icon16 = TryGetIcon(instance, type, $"{method.Name}Icon16");

                    var item = CreateItem(displayName, (sender, e) => method.Invoke(instance, null), enabled, icon16);
                    items.Add(item);
                }
            }

            return items;
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

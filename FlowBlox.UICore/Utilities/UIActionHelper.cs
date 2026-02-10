using FlowBlox.UICore.Interfaces;
using System.Reflection;

namespace FlowBlox.UICore.Utilities
{
    public static class UIActionHelper
    {
        public static object? GetComponentUIActionForType(Type componentType, object component)
        {
            var actionType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .FirstOrDefault(t =>
                    t.BaseType != null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(ComponentUIActions<>) &&
                    t.BaseType.GetGenericArguments()[0].IsAssignableFrom(componentType));

            if (actionType == null)
                return null;

            var constructor = actionType.GetConstructor(new[] { componentType });
            if (constructor == null)
                return null;

            return constructor.Invoke(new object[] { component });
        }

        public static object? InvokeUIActionMethod(object? uiActions, string methodName)
        {
            if (uiActions == null) return null;

            var method = uiActions.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            return method?.Invoke(uiActions, null);
        }

        public static TResult? InvokeUIActionMethod<TResult>(object? uiActions, string methodName)
        {
            var result = InvokeUIActionMethod(uiActions, methodName);
            return result is TResult typedResult ? typedResult : default;
        }
    }
}

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
                .Where(t =>
                    t.BaseType != null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(ComponentUIActions<>) &&
                    t.BaseType.GetGenericArguments()[0].IsAssignableFrom(componentType))
                .Select(t => new
                {
                    ActionType = t,
                    TargetType = t.BaseType!.GetGenericArguments()[0],
                    Distance = GetInheritanceDistance(componentType, t.BaseType!.GetGenericArguments()[0])
                })
                .OrderBy(x => x.Distance)
                .Select(x => x.ActionType)
                .FirstOrDefault();

            if (actionType == null)
                return null;

            var constructor = actionType.GetConstructors()
                .FirstOrDefault(c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.Length == 1 &&
                           parameters[0].ParameterType.IsAssignableFrom(componentType);
                });
            if (constructor == null)
                return null;

            return constructor.Invoke(new object[] { component });
        }

        private static int GetInheritanceDistance(Type derivedType, Type baseType)
        {
            if (!baseType.IsAssignableFrom(derivedType))
                return int.MaxValue;

            if (derivedType == baseType)
                return 0;

            if (baseType.IsInterface)
                return 1;

            int distance = 0;
            var current = derivedType;

            while (current != null && current != baseType)
            {
                distance++;
                current = current.BaseType;
            }

            return current == baseType ? distance : int.MaxValue - 1;
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

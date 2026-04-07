using FlowBlox.Core.Attributes;
using System.Reflection;

namespace FlowBlox.Core.Util
{
    public static class FlowBloxSupportedTypesResolver
    {
        public static IList<Type> ResolveSupportedTypes(
            object target,
            Type assignableToType,
            IEnumerable<Type>? candidateTypes = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return ResolveSupportedTypes(target.GetType(), assignableToType, candidateTypes);
        }

        public static IList<Type> ResolveSupportedTypes(
            Type targetType,
            Type assignableToType,
            IEnumerable<Type>? candidateTypes = null)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (assignableToType == null)
                throw new ArgumentNullException(nameof(assignableToType));

            var sourceTypes = candidateTypes ?? AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetLoadableTypes);

            return sourceTypes
                .Where(t => t != null)
                .Where(t => assignableToType.IsAssignableFrom(t))
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => IsSupportedForTarget(t, targetType))
                .Distinct()
                .ToList();
        }

        private static bool IsSupportedForTarget(Type candidateType, Type targetType)
        {
            var attribute = candidateType.GetCustomAttribute<FlowBloxSupportedTypesAttribute>();
            return attribute == null || attribute.SupportedTypes.Any(x => x.IsAssignableFrom(targetType));
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
        }
    }
}

using System.Reflection;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Util
{
    public static class ReflectionHelper
    {
        public static Type GetTypeByClass(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);

                if (type != null)
                    return type;
            }

            return null;
        }

        public static Type TryMakeGenericType(Type genericTypeDefinition, Type typeArgument)
        {
            if (genericTypeDefinition.IsGenericTypeDefinition &&
                genericTypeDefinition.GetGenericArguments().Length == 1 &&
                genericTypeDefinition.GetGenericArguments()[0].IsAssignableFrom(typeArgument))
            {
                return genericTypeDefinition.MakeGenericType(typeArgument);
            }
            return null;
        }

        public static Type GetImplementationTypeForTypeWithGeneric(Type abstractType, Type elementType)
        {
            Type genericType = abstractType.GetGenericTypeDefinition();

            Type specificType = TryMakeGenericType(genericType, elementType);
            if (specificType == null)
                return null;

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SingleOrDefault(x => specificType.IsAssignableFrom(x));

            return type;
        }

        public static bool HasSpecificTypeWithGeneric(Type abstractType, Type genericType)
        {
            var implType = GetImplementationTypeForTypeWithGeneric(abstractType, genericType);
            return implType != null;
        }

        public static IEnumerable<Type> GetDerivedClasses<T>()
        {
            var derivedTypes = Assembly.GetAssembly(typeof(T))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(T)));

            return derivedTypes;
        }

        public static void CopyValueTypedProperties<T1, T2>(T1 source, T2 destination, bool useInheritanceForInterfaces = true)
        {
            CopyValueTypedProperties(source, typeof(T1), destination, typeof(T2), useInheritanceForInterfaces);
        }

        public static void CopyValueTypedProperties(object source, Type sourceType, object destination, Type destinationType, bool useInheritanceForInterfaces = true)
        {
            var properties = GetProperties(sourceType, BindingFlags.Public | BindingFlags.Instance, useInheritanceForInterfaces);
            foreach (PropertyInfo piSource in properties)
            {
                if ((piSource.PropertyType.IsValueType || piSource.PropertyType == typeof(string)) && piSource.CanRead)
                {
                    PropertyInfo piDestination = destinationType.GetProperty(piSource.Name);
                    if (piDestination != null && piDestination.CanWrite)
                    {
                        piDestination.SetValue(destination, piSource.GetValue(source));
                    }
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags flags, bool useInheritanceForInterfaces = true)
        {
            if (!type.IsInterface || !useInheritanceForInterfaces)
                return type.GetProperties(flags);

            return new[] { type }
                .Concat(type.GetInterfaces())
                .SelectMany(i => i.GetProperties(flags));
        }

        public static Type GetInterfaceTypeMatchingGenericDefinition(Type typeToCheck, Type genericInterfaceDefinition)
        {
            foreach (Type interfaceType in typeToCheck.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == genericInterfaceDefinition)
                {
                    return interfaceType;
                }
            }
            return null;
        }

        public static object CastList(List<object> list, Type listType)
        {
            MethodInfo castMethod = typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(listType);
            MethodInfo toListMethod = typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod(listType);
            var castedObject = castMethod.Invoke(null, [list]);
            var typedList = toListMethod.Invoke(null, [castedObject]);
            return typedList;
        }

        public static PropertyInfo GetPropertyFromType(Type type, string propertyName, bool includeInterfaces = true)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo == null && includeInterfaces)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    propertyInfo = interfaceType.GetProperty(propertyName);
                    if (propertyInfo != null)
                    {
                        break;
                    }
                }
            }
            return propertyInfo;
        }
    }
}

using System.Reflection;

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

        public static Type? GetTypeByFullNameFromLastPart(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            var requested = typeName.Trim();
            var typeNameParts = requested
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (typeNameParts.Length == 0)
                return null;

            for (var partCount = 1; partCount <= typeNameParts.Length; partCount++)
            {
                var ending = string.Join(".", typeNameParts.Skip(typeNameParts.Length - partCount));
                var candidates = GetLoadedTypesSafely()
                    .Where(type => IsFullNameEndingMatch(type, ending))
                    .Take(2)
                    .ToList();

                if (candidates.Count == 1)
                    return candidates[0];
            }

            return null;
        }

        private static bool IsFullNameEndingMatch(Type type, string ending)
        {
            var fullName = type?.FullName;
            return !string.IsNullOrWhiteSpace(fullName) &&
                   (string.Equals(fullName, ending, StringComparison.Ordinal) ||
                    fullName.EndsWith("." + ending, StringComparison.Ordinal));
        }

        private static IEnumerable<Type> GetLoadedTypesSafely()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(x => x != null).ToArray()!;
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type != null)
                        yield return type;
                }
            }
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
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote
{
    public static class AITargetPropertyHandler
    {
        private static bool TryExtractJsonToken(string value, out JToken token)
        {
            token = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (TryParseJson(value, out token))
                return true;

            var fenceMatch = Regex.Match(value, "```(?:json)?\\s*(?<json>[\\s\\S]*?)\\s*```", RegexOptions.IgnoreCase);
            if (fenceMatch.Success)
            {
                var fencedJson = fenceMatch.Groups["json"].Value;
                if (TryParseJson(fencedJson, out token))
                    return true;
            }

            var firstObjectIndex = value.IndexOf('{');
            var firstArrayIndex = value.IndexOf('[');
            var startIndex = firstObjectIndex;
            if (startIndex < 0 || (firstArrayIndex >= 0 && firstArrayIndex < startIndex))
                startIndex = firstArrayIndex;

            if (startIndex >= 0)
            {
                var jsonCandidate = value.Substring(startIndex).Trim();
                if (TryParseJson(jsonCandidate, out token))
                    return true;
            }

            return false;
        }

        private static bool TryParseJson(string value, out JToken token)
        {
            token = null;
            try
            {
                token = JToken.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetTypeName(Type type)
        {
            var nonNullable = Nullable.GetUnderlyingType(type);
            if (nonNullable != null)
                return $"{GetTypeName(nonNullable)}?";

            if (type == typeof(string))
                return "string";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(int))
                return "int";
            if (type == typeof(long))
                return "long";
            if (type == typeof(double))
                return "double";
            if (type == typeof(float))
                return "float";
            if (type == typeof(decimal))
                return "decimal";
            if (type == typeof(DateTime))
                return "DateTime";

            if (type.IsGenericType)
            {
                var baseName = type.Name;
                var tickIndex = baseName.IndexOf('`');
                if (tickIndex >= 0)
                    baseName = baseName.Substring(0, tickIndex);

                var args = type.GetGenericArguments().Select(GetTypeName);
                return $"{baseName}<{string.Join(", ", args)}>";
            }

            return type.FullName ?? type.Name;
        }

        private static Type GetCollectionElementType(Type type)
        {
            if (type == typeof(string))
                return null;

            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
                return type.GetGenericArguments().FirstOrDefault();

            var enumerableInterface = type.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableInterface?.GetGenericArguments().FirstOrDefault();
        }

        
        private static bool IsNullable(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                   || type == typeof(string)
                   || type == typeof(decimal)
                   || type == typeof(DateTime)
                   || type == typeof(Guid)
                   || type == typeof(DateTimeOffset)
                   || type == typeof(TimeSpan);
        }

        public static PropertyInfo GetTargetPropertyInfo(BaseFlowBlock source, string targetPropertyName)
        {
            if (source == null || string.IsNullOrWhiteSpace(targetPropertyName))
                return null;

            return source.GetType().GetProperty(targetPropertyName, BindingFlags.Instance | BindingFlags.Public);
        }

        public static string BuildTargetPropertyDescription(BaseFlowBlock source, string targetPropertyName)
        {
            var targetProperty = GetTargetPropertyInfo(source, targetPropertyName);
            if (targetProperty == null)
                return "Target property is not configured or could not be resolved.";

            var root = new JObject
            {
                ["name"] = targetProperty.Name,
                ["schema"] = BuildTargetPropertyDescription(targetProperty.PropertyType, 0, new HashSet<string>(StringComparer.Ordinal))
            };

            return root.ToString(Formatting.Indented);
        }

        private static IEnumerable<Type> ResolveSupportedTypes(Type baseType)
        {
            var supportedTypes = new Dictionary<string, Type>(StringComparer.Ordinal);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                        continue;
                    if (!baseType.IsAssignableFrom(type))
                        continue;
                    if (typeof(IFlowBloxComponent).IsAssignableFrom(type))
                        continue;

                    supportedTypes[type.FullName ?? type.Name] = type;
                }
            }

            return supportedTypes.Values
                .OrderBy(x => x.FullName ?? x.Name, StringComparer.Ordinal)
                .Take(8)
                .ToList();
        }

        public static bool IsSupportedTargetProperty(PropertyInfo property)
        {
            if (property == null)
                return false;

            if (!property.CanWrite ||
                property.SetMethod == null ||
                !property.SetMethod.IsPublic ||
                property.GetIndexParameters().Length != 0)
            {
                return false;
            }

            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (propertyType == typeof(string) || IsSimpleType(propertyType) || propertyType.IsEnum)
            {
                return true;
            }

            var collectionElementType = GetCollectionElementType(propertyType);
            if (collectionElementType != null)
            {
                var collectionElementNonNullableType = Nullable.GetUnderlyingType(collectionElementType) ?? collectionElementType;
                return !typeof(IFlowBloxComponent).IsAssignableFrom(collectionElementNonNullableType);
            }

            if (propertyType.IsInterface || propertyType.IsAbstract)
                return false;

            return !typeof(IFlowBloxComponent).IsAssignableFrom(propertyType);
        }

        public static JObject BuildTargetPropertyDescription(Type type, int depth, HashSet<string> visited)
        {
            var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
            var descriptor = new JObject
            {
                ["type"] = GetTypeName(type)
            };

            if (nonNullableType.IsEnum)
            {
                descriptor["kind"] = "enum";
                descriptor["enumValues"] = new JArray(Enum.GetNames(nonNullableType));
                return descriptor;
            }

            if (IsSimpleType(nonNullableType))
            {
                descriptor["kind"] = "simple";
                return descriptor;
            }

            var collectionElementType = GetCollectionElementType(nonNullableType);
            if (collectionElementType != null)
            {
                descriptor["kind"] = "collection";
                descriptor["item"] = BuildTargetPropertyDescription(collectionElementType, depth + 1, visited);
                return descriptor;
            }

            descriptor["kind"] = "object";

            var typeKey = nonNullableType.FullName ?? nonNullableType.Name;
            if (visited.Contains(typeKey))
            {
                descriptor["properties"] = new JArray();
                return descriptor;
            }

            visited.Add(typeKey);

            var properties = nonNullableType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanWrite && x.GetIndexParameters().Length == 0)
                .OrderBy(x => x.Name, StringComparer.Ordinal)
                .ToList();

            var propertyArray = new JArray();
            if (depth <= 2)
            {
                foreach (var property in properties)
                {
                    var propertyType = property.PropertyType;
                    var propertyNonNullableType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                    var propertyDescriptor = new JObject
                    {
                        ["name"] = property.Name,
                        ["type"] = GetTypeName(propertyType),
                        ["nullable"] = IsNullable(propertyType)
                    };

                    if (propertyNonNullableType.IsEnum)
                    {
                        propertyDescriptor["enumValues"] = new JArray(Enum.GetNames(propertyNonNullableType));
                    }

                    if (propertyNonNullableType.IsInterface || propertyNonNullableType.IsAbstract)
                    {
                        var supportedTypes = ResolveSupportedTypes(propertyNonNullableType)
                            .Select(x => BuildTargetPropertyDescription(x, depth + 1, visited))
                            .ToList();

                        propertyDescriptor["supportedTypes"] = new JArray(supportedTypes);
                    }
                    else if (!IsSimpleType(propertyNonNullableType))
                    {
                        var elementType = GetCollectionElementType(propertyNonNullableType);
                        if (elementType != null)
                        {
                            propertyDescriptor["item"] = BuildTargetPropertyDescription(elementType, depth + 1, visited);
                        }
                        else
                        {
                            propertyDescriptor["schema"] = BuildTargetPropertyDescription(propertyNonNullableType, depth + 1, visited);
                        }
                    }

                    propertyArray.Add(propertyDescriptor);
                }
            }

            descriptor["properties"] = propertyArray;

            visited.Remove(typeKey);
            return descriptor;
        }

        public static object ParseTargetPropertyValue(PropertyInfo targetProperty, object value)
        {
            var propertyType = targetProperty.PropertyType;
            if (propertyType == typeof(string))
                return value?.ToString();

            if (value == null)
                return null;

            if (propertyType.IsInstanceOfType(value))
                return value;

            var text = value.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (!TryExtractJsonToken(text, out var token))
            {
                throw new InvalidOperationException(
                    $"AI response for target property '{targetProperty.Name}' is not valid JSON. " +
                    "Return JSON only and match the target property schema.");
            }

            var serialized = token.ToString(Formatting.None);
            var deserialized = JsonConvert.DeserializeObject(serialized, propertyType);
            if (deserialized == null && !IsNullable(propertyType))
            {
                throw new InvalidOperationException(
                    $"AI response for target property '{targetProperty.Name}' produced null, but the target property is not nullable.");
            }

            return deserialized;
        }
    }
}

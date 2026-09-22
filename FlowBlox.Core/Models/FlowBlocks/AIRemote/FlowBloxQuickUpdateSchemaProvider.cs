using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote
{
    public static class FlowBloxQuickUpdateSchemaProvider
    {
        private static readonly HashSet<Type> ExcludedFlowBlockBaseTypes =
        [
            typeof(BaseFlowBlock),
            typeof(BaseResultFlowBlock),
            typeof(BaseSingleResultFlowBlock),
            typeof(BasePipeFlowBlock)
        ];

        public static string BuildSchema(BaseFlowBlock target)
        {
            if (target == null)
                return "Target flow block is not configured.";

            var schema = new JObject
            {
                ["JsonContract"] = "FlowBloxQuickUpdateSchema",
                ["FlowBlockType"] = target.GetType().Name,
                ["Title"] = FlowBloxComponentHelper.GetDisplayName(target),
                ["Description"] = FlowBloxComponentHelper.GetDescription(target),
                ["UpdateFormat"] = new JObject
                {
                    ["JsonContract"] = "FlowBloxQuickUpdate",
                    ["Properties"] = new JObject
                    {
                        ["PropertyName"] = "value"
                    }
                },
                ["Properties"] = new JArray(GetConfigurableProperties(target.GetType())
                    .Select(x => BuildPropertySchema(x, 0, new HashSet<string>(StringComparer.Ordinal))))
            };

            return schema.ToString(Formatting.Indented);
        }

        internal static IReadOnlyDictionary<string, QuickUpdatePropertyDefinition> GetConfigurablePropertyMap(Type flowBlockType)
        {
            return GetConfigurableProperties(flowBlockType)
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        }

        internal static IReadOnlyDictionary<string, QuickUpdatePropertyDefinition> GetReactiveObjectPropertyMap(Type reactiveObjectType)
        {
            return GetSettableProperties(reactiveObjectType)
                .Select(CreatePropertyDefinition)
                .Where(x => x != null && x.IsConfigurable)
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<QuickUpdatePropertyDefinition> GetConfigurableProperties(Type flowBlockType)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var type = flowBlockType;
                 type != null && type != typeof(object) && !ExcludedFlowBlockBaseTypes.Contains(type);
                 type = type.BaseType)
            {
                foreach (var property in GetSettableProperties(type))
                {
                    if (!seen.Add(property.Name))
                        continue;

                    var definition = CreatePropertyDefinition(property);
                    if (definition?.IsConfigurable == true)
                        yield return definition;
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetSettableProperties(Type type)
        {
            return type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(x => x.CanWrite &&
                            x.SetMethod != null &&
                            x.SetMethod.IsPublic &&
                            x.GetIndexParameters().Length == 0);
        }

        private static QuickUpdatePropertyDefinition? CreatePropertyDefinition(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            if (propertyType == typeof(string) || IsSimpleType(Nullable.GetUnderlyingType(propertyType) ?? propertyType))
            {
                return new QuickUpdatePropertyDefinition(property, QuickUpdatePropertyKind.Simple);
            }

            var nonNullableType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (nonNullableType.IsEnum)
                return new QuickUpdatePropertyDefinition(property, QuickUpdatePropertyKind.Enum);

            var collectionElementType = GetCollectionElementType(propertyType);
            if (collectionElementType != null)
            {
                var elementType = Nullable.GetUnderlyingType(collectionElementType) ?? collectionElementType;
                if (IsFlowBloxReactiveObjectOnly(elementType))
                {
                    return new QuickUpdatePropertyDefinition(
                        property,
                        QuickUpdatePropertyKind.ReactiveObjectCollection,
                        elementType);
                }

                return null;
            }

            if (IsFlowBloxReactiveObjectOnly(nonNullableType))
                return new QuickUpdatePropertyDefinition(property, QuickUpdatePropertyKind.ReactiveObject);

            return null;
        }

        private static JObject BuildPropertySchema(
            QuickUpdatePropertyDefinition definition,
            int depth,
            HashSet<string> visitedTypes)
        {
            var property = definition.Property;
            var propertyType = property.PropertyType;
            var schema = new JObject
            {
                ["Name"] = property.Name,
                ["Type"] = GetTypeName(propertyType),
                ["Kind"] = definition.Kind.ToString(),
                ["Nullable"] = IsNullable(propertyType)
            };

            AppendDisplayMetadata(schema, property.GetCustomAttribute<DisplayAttribute>());

            if (definition.Kind == QuickUpdatePropertyKind.Enum)
            {
                var enumType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                schema["EnumValues"] = new JArray(Enum.GetNames(enumType));
            }
            else if (definition.Kind == QuickUpdatePropertyKind.ReactiveObjectCollection && definition.ElementType != null)
            {
                schema["Item"] = BuildReactiveObjectSchema(definition.ElementType, depth + 1, visitedTypes);
            }
            else if (definition.Kind == QuickUpdatePropertyKind.ReactiveObject)
            {
                var reactiveType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                schema["Schema"] = BuildReactiveObjectSchema(reactiveType, depth + 1, visitedTypes);
            }

            return schema;
        }

        private static JObject BuildReactiveObjectSchema(Type type, int depth, HashSet<string> visitedTypes)
        {
            var schema = new JObject
            {
                ["Type"] = GetTypeName(type),
                ["Kind"] = "ReactiveObject",
                ["Properties"] = new JArray()
            };

            var typeKey = type.FullName ?? type.Name;
            if (depth > 3 || !visitedTypes.Add(typeKey))
                return schema;

            var configurableProperties = GetReactiveObjectPropertyMap(type).Values
                .Select(x => BuildPropertySchema(x, depth, visitedTypes))
                .ToList();

            schema["Properties"] = new JArray(configurableProperties);

            var nonConfigurable = GetSettableProperties(type)
                .Where(x => !GetReactiveObjectPropertyMap(type).ContainsKey(x.Name))
                .Where(IsRelevantNonConfigurableReference)
                .Select(x =>
                {
                    var item = new JObject
                    {
                        ["Name"] = x.Name,
                        ["Type"] = GetTypeName(x.PropertyType),
                        ["Configurable"] = false,
                        ["Reason"] = "References to FlowBlocks, ManagedObjects, FieldElements or other FlowBlox components are not configured by FlowBlox Quick Update."
                    };
                    AppendDisplayMetadata(item, x.GetCustomAttribute<DisplayAttribute>());
                    return item;
                })
                .ToList();

            if (nonConfigurable.Count > 0)
                schema["NonConfigurableProperties"] = new JArray(nonConfigurable);

            visitedTypes.Remove(typeKey);
            return schema;
        }

        private static bool IsRelevantNonConfigurableReference(PropertyInfo property)
        {
            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var elementType = GetCollectionElementType(type);
            if (elementType != null)
                type = Nullable.GetUnderlyingType(elementType) ?? elementType;

            return typeof(BaseFlowBlock).IsAssignableFrom(type) ||
                   typeof(ManagedObject).IsAssignableFrom(type) ||
                   typeof(FlowBloxComponent).IsAssignableFrom(type);
        }

        internal static Type? GetCollectionElementType(Type type)
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

        internal static bool IsFlowBloxReactiveObjectOnly(Type type)
        {
            return typeof(FlowBloxReactiveObject).IsAssignableFrom(type) &&
                   !typeof(BaseFlowBlock).IsAssignableFrom(type) &&
                   !typeof(ManagedObject).IsAssignableFrom(type) &&
                   !typeof(FlowBloxComponent).IsAssignableFrom(type);
        }

        internal static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                   || type == typeof(string)
                   || type == typeof(decimal)
                   || type == typeof(DateTime)
                   || type == typeof(Guid)
                   || type == typeof(DateTimeOffset)
                   || type == typeof(TimeSpan);
        }

        private static bool IsNullable(Type type)
            => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

        private static string GetTypeName(Type type)
        {
            var nonNullable = Nullable.GetUnderlyingType(type);
            if (nonNullable != null)
                return $"{GetTypeName(nonNullable)}?";

            if (type.IsGenericType)
            {
                var baseName = type.Name;
                var tickIndex = baseName.IndexOf('`');
                if (tickIndex >= 0)
                    baseName = baseName[..tickIndex];

                return $"{baseName}<{string.Join(", ", type.GetGenericArguments().Select(GetTypeName))}>";
            }

            return type.Name;
        }

        private static void AppendDisplayMetadata(JObject target, DisplayAttribute? displayAttribute)
        {
            if (target == null || displayAttribute == null)
                return;

            var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, requireDisplayName: false);
            if (!string.IsNullOrWhiteSpace(displayName))
                target["Title"] = displayName;

            var description = FlowBloxResourceUtil.GetDescription(displayAttribute);
            if (!string.IsNullOrWhiteSpace(description))
                target["Description"] = description;
        }
    }

    internal enum QuickUpdatePropertyKind
    {
        Simple,
        Enum,
        ReactiveObject,
        ReactiveObjectCollection
    }

    internal sealed class QuickUpdatePropertyDefinition
    {
        public QuickUpdatePropertyDefinition(
            PropertyInfo property,
            QuickUpdatePropertyKind kind,
            Type? elementType = null)
        {
            Property = property;
            Kind = kind;
            ElementType = elementType;
        }

        public PropertyInfo Property { get; }

        public QuickUpdatePropertyKind Kind { get; }

        public Type? ElementType { get; }

        public string Name => Property.Name;

        public bool IsConfigurable => true;
    }
}

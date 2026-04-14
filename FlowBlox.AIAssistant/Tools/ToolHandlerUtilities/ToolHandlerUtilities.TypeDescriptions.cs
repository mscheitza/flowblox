using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal static partial class ToolHandlerUtilities
    {
        public static ToolResponse CreateTypeInfoResponse(
            string? fullName,
            Type mustAssignType,
            string label,
            IReadOnlyCollection<Type>? excludedBaseTypes = null)
        {
            var type = ResolveType(fullName);
            if (type == null || !mustAssignType.IsAssignableFrom(type))
            {
                return Fail($"Type '{fullName}' is not a {label} type.");
            }

            var additionalKinds = new Dictionary<string, JObject>(StringComparer.Ordinal);
            var kind = BuildTypeKindInfo(
                type,
                includeChildren: true,
                additionalKinds,
                isTopLevel: true,
                excludedBaseTypes);

            return Ok(new JObject
            {
                ["kind"] = kind,
                ["additionalTypeKindsInfo"] = new JArray(additionalKinds.Values.OrderBy(x => x.Value<string>("fullName")))
            });
        }

        public static ToolResponse CreateUnifiedTypeInfoResponse(
            string? fullName,
            IReadOnlyCollection<Type>? excludedBaseTypes = null)
        {
            var type = ResolveType(fullName);
            if (type == null)
            {
                return Fail($"Type '{fullName}' could not be resolved.");
            }

            if (type.IsEnum)
            {
                return Ok(new JObject
                {
                    ["kind"] = BuildEnumKindInfo(type),
                    ["additionalTypeKindsInfo"] = new JArray()
                });
            }

            if (!typeof(FlowBloxReactiveObject).IsAssignableFrom(type))
            {
                return Fail(
                    $"Type '{fullName}' is not supported for kind info. " +
                    "Only FlowBloxReactiveObject types (including FlowBlock/ManagedObject) and Enums are supported.");
            }

            var additionalKinds = new Dictionary<string, JObject>(StringComparer.Ordinal);
            var kind = BuildTypeKindInfo(
                type,
                includeChildren: true,
                additionalKinds,
                isTopLevel: true,
                excludedBaseTypes);

            return Ok(new JObject
            {
                ["kind"] = kind,
                ["additionalTypeKindsInfo"] = new JArray(additionalKinds.Values.OrderBy(x => x.Value<string>("fullName")))
            });
        }

        public static IEnumerable<JObject> GetSupportedTypeInfos(Type baseType)
        {
            var supportedTypes = new Dictionary<string, JObject>(StringComparer.Ordinal);

            if (!baseType.IsAbstract && !baseType.IsInterface)
            {
                AddSupportedTypeInfo(supportedTypes, baseType);
            }

            foreach (var flowBlock in GetProject().CreateInstances<BaseFlowBlock>())
            {
                var type = flowBlock.GetType();
                if (!baseType.IsAssignableFrom(type) || type.IsAbstract)
                {
                    continue;
                }

                AddSupportedTypeInfo(supportedTypes, type);
            }

            foreach (var managedObject in GetProject().CreateInstances<IManagedObject>())
            {
                var type = managedObject.GetType();
                if (!baseType.IsAssignableFrom(type) || type.IsAbstract)
                {
                    continue;
                }

                AddSupportedTypeInfo(supportedTypes, type);
            }

            foreach (var type in EnumerateLoadableTypes())
            {
                if (!baseType.IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (IsNonComponentReactiveObjectType(baseType) && !IsNonComponentReactiveObjectType(type))
                {
                    continue;
                }

                AddSupportedTypeInfo(supportedTypes, type);
            }

            return supportedTypes.Values
                .OrderBy(x => x.Value<string>("displayName"))
                .ThenBy(x => x.Value<string>("fullName"));
        }

        private static JObject BuildTypeKindInfo(
            Type type,
            bool includeChildren,
            Dictionary<string, JObject> additionalKinds,
            bool isTopLevel,
            IReadOnlyCollection<Type>? excludedBaseTypes = null)
        {
            var typeDisplayMetadata = GetTypeDisplayMetadata(type);
            var usedTypes = new Dictionary<string, JObject>(StringComparer.Ordinal);
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Where(p => ShouldIncludeProperty(type, p, excludedBaseTypes))
                .OrderBy(p => p.Name)
                .ToList();

            var propertyInfos = new JArray();
            foreach (var property in properties)
            {
                propertyInfos.Add(ToPropertyInfo(
                    property,
                    includeChildren,
                    additionalKinds,
                    usedTypes,
                    excludedBaseTypes));
            }

            var result = new JObject
            {
                ["fullName"] = type.FullName ?? type.Name,
                ["displayName"] = typeDisplayMetadata.DisplayName,
                ["description"] = typeDisplayMetadata.Description,
                ["inheritanceHierarchy"] = new JArray(GetTypeHierarchy(type)),
                ["properties"] = propertyInfos,
                ["usedTypes"] = new JArray(usedTypes.Values.OrderBy(x => x.Value<string>("fullName")))
            };

            var specialExplanations = BuildSpecialExplanations(type);
            if (specialExplanations.Count > 0)
            {
                result["specialExplanations"] = specialExplanations;
            }

            if (isTopLevel)
            {
                result["rules"] = new JObject
                {
                    ["fieldPlaceholderSyntax"] = "$FlowBlock::FieldName",
                    ["placeholderLookupTool"] = "GetPlaceholders",
                    ["baseFlowBlockHint"] = "BaseFlowBlock members are excluded by default for derived flow blocks. Query BaseFlowBlock directly to inspect those members.",
                    ["referenceComponentKinds"] = "FlowBlock, ManagedObject (including FieldElement).",
                    ["reactiveObjectOnlyKind"] = "FlowBloxReactiveObject-only (non-reference component) values exist only in their parent context."
                };
            }

            return result;
        }

        private static JObject ToPropertyInfo(
            PropertyInfo property,
            bool includeChildren,
            Dictionary<string, JObject> additionalKinds,
            Dictionary<string, JObject> usedTypes,
            IReadOnlyCollection<Type>? excludedBaseTypes = null)
        {
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            var displayName = displayAttribute != null
                ? FlowBloxResourceUtil.GetDisplayName(displayAttribute, false)
                : property.Name;
            var description = displayAttribute != null
                ? FlowBloxResourceUtil.GetDescription(displayAttribute)
                : string.Empty;

            var propertyType = property.PropertyType;
            var elementType = GetElementType(propertyType);
            var actualType = elementType ?? propertyType;
            var nonNullableType = Nullable.GetUnderlyingType(actualType) ?? actualType;

            var isCollection = elementType != null;
            var isManagedObject = typeof(IManagedObject).IsAssignableFrom(nonNullableType);
            var isFieldElement = typeof(FieldElement).IsAssignableFrom(nonNullableType);
            var isReactiveObject = typeof(FlowBloxReactiveObject).IsAssignableFrom(nonNullableType);
            var isFlowBlockReference = typeof(BaseFlowBlock).IsAssignableFrom(nonNullableType);
            var isEnum = nonNullableType.IsEnum;
            var isSimple = IsSimpleType(nonNullableType);
            var nullable = IsNullable(property);
            var majorTypeDescriptor = "SimpleProperty";
            if (isFieldElement)
                majorTypeDescriptor = "FieldElement";
            else if (isManagedObject)
                majorTypeDescriptor = "ManagedObject";
            else if (isFlowBlockReference)
                majorTypeDescriptor = "FlowBlock";
            else if (isReactiveObject)
                majorTypeDescriptor = "ReactiveObject";

            var flowBlockUi = property.GetCustomAttribute<FlowBloxUIAttribute>();
            var uiOptions = flowBlockUi != null
                ? Enum.GetValues(typeof(UIOptions))
                    .Cast<UIOptions>()
                    .Where(option => option != 0)
                    .Where(option => flowBlockUi.UiOptions.HasFlag(option))
                    .Select(option => option.ToString())
                : Enumerable.Empty<string>();
            var uiOperations = flowBlockUi != null
                ? Enum.GetValues(typeof(UIOperations))
                    .Cast<UIOperations>()
                    .Where(operation => operation != UIOperations.None && operation != UIOperations.All)
                    .Where(operation => flowBlockUi.Operations.HasFlag(operation))
                    .Select(operation => operation.ToString())
                : Enumerable.Empty<string>();

            var propertyInfo = new JObject
            {
                ["name"] = property.Name,
                ["displayName"] = string.IsNullOrWhiteSpace(displayName) ? property.Name : displayName,
                ["description"] = description,
                ["type"] = propertyType.FullName ?? propertyType.Name,
                ["nullable"] = nullable,
                ["canWrite"] = property.CanWrite,
                ["isSimple"] = isSimple,
                ["isEnum"] = isEnum,
                ["isCollection"] = isCollection,
                ["majorTypeDescriptor"] = majorTypeDescriptor,
                ["canUpdateViaParentPath"] = !isManagedObject,
                ["updateRule"] = isManagedObject
                    ? "Do not update through parent path. Update the target managed object directly."
                    : "Can be updated via JSON path from the primary object."
            };

            if (flowBlockUi != null)
            {
                propertyInfo["ui"] = new JObject
                {
                    ["operations"] = new JArray(uiOperations),
                    ["uiOptions"] = new JArray(uiOptions),
                    ["creatableTypes"] = new JArray((flowBlockUi.CreatableTypes ?? Array.Empty<Type>()).Select(x => x.FullName ?? x.Name)),
                    ["readOnly"] = flowBlockUi.ReadOnly,
                    ["toolboxCategory"] = flowBlockUi.ToolboxCategory ?? string.Empty
                };
            }

            propertyInfo["isFlowBlockReference"] = isFlowBlockReference;
            propertyInfo["attributes"] = new JArray(property.GetCustomAttributesData()
                .Where(ShouldExposePropertyAttribute)
                .Select(BuildAttributeInfo));

            if (isEnum)
            {
                propertyInfo["enumMembers"] = new JArray(Enum.GetNames(nonNullableType));
                AddUsedEnumType(usedTypes, nonNullableType);
            }

            if (nonNullableType == typeof(string)
                && flowBlockUi?.UiOptions.HasFlag(UIOptions.EnableFieldSelection) == true)
            {
                propertyInfo["fieldPlaceholderSyntax"] = "$FlowBlock::FieldName";
                propertyInfo["placeholderLookupTool"] = "GetPlaceholders";
                propertyInfo["placeholderKinds"] = new JArray("Field", "Project", "Options");
            }

            if (isManagedObject)
            {
                propertyInfo["resolverHints"] = new JArray
                {
                    "resolveManagedObjectByName",
                    "resolveFieldElementByFQName"
                };
            }

            if (isFlowBlockReference)
            {
                propertyInfo["resolverHints"] = new JArray
                {
                    "resolveFlowBlockByName"
                };
            }

            if ((propertyType.IsInterface || propertyType.IsAbstract || isManagedObject || isFlowBlockReference)
                && !isFlowBlockReference
                && !isSimple)
            {
                var supportedTypes = GetSupportedTypeInfos(nonNullableType);
                propertyInfo["supportedTypes"] = new JArray(supportedTypes);
            }

            if (includeChildren && IsNonComponentReactiveObjectType(nonNullableType) && !isManagedObject && !isSimple)
            {
                var key = nonNullableType.FullName ?? nonNullableType.Name;
                if (!additionalKinds.ContainsKey(key))
                {
                    additionalKinds[key] = BuildTypeKindInfo(
                        nonNullableType,
                        includeChildren: false,
                        additionalKinds,
                        isTopLevel: false,
                        excludedBaseTypes);
                }
            }

            if (IsNonComponentReactiveObjectType(nonNullableType))
            {
                AddUsedReactiveObjectType(usedTypes, nonNullableType);
            }

            return propertyInfo;
        }

        private static JObject BuildAttributeInfo(CustomAttributeData attributeData)
        {
            var ctorArgs = attributeData.ConstructorArguments
                .Select(arg => arg.Value?.ToString() ?? string.Empty);

            var namedArgs = attributeData.NamedArguments
                .ToDictionary(
                    arg => arg.MemberName,
                    arg => ToAttributeValueToken(arg.TypedValue.Value));

            return new JObject
            {
                ["attributeType"] = attributeData.AttributeType.FullName ?? attributeData.AttributeType.Name,
                ["constructorArguments"] = new JArray(ctorArgs),
                ["namedArguments"] = JObject.FromObject(namedArgs)
            };
        }

        private static JToken ToAttributeValueToken(object? value)
        {
            if (value == null)
            {
                return JValue.CreateNull();
            }

            if (value is Type type)
            {
                return new JValue(type.FullName ?? type.Name);
            }

            if (value is IReadOnlyCollection<CustomAttributeTypedArgument> typedArguments)
            {
                return new JArray(typedArguments.Select(x => ToAttributeValueToken(x.Value)));
            }

            return JToken.FromObject(value);
        }

        private static void AddSupportedTypeInfo(IDictionary<string, JObject> target, Type type)
        {
            var key = type.FullName ?? type.Name;
            if (target.ContainsKey(key))
            {
                return;
            }

            var typeDisplayMetadata = GetTypeDisplayMetadata(type);

            target[key] = new JObject
            {
                ["fullName"] = key,
                ["displayName"] = typeDisplayMetadata.DisplayName,
                ["description"] = typeDisplayMetadata.Description,
                ["isFlowBlock"] = typeof(BaseFlowBlock).IsAssignableFrom(type),
                ["isManagedObject"] = typeof(IManagedObject).IsAssignableFrom(type),
                ["isFieldElement"] = typeof(FieldElement).IsAssignableFrom(type)
            };
        }

        private static (string DisplayName, string Description) GetTypeDisplayMetadata(Type type)
        {
            var displayAttribute = type.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute == null)
            {
                return (type.Name, string.Empty);
            }

            var displayName = type.Name;
            var description = string.Empty;

            try
            {
                var localizedDisplayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, false);
                if (!string.IsNullOrWhiteSpace(localizedDisplayName))
                {
                    displayName = localizedDisplayName;
                }
            }
            catch
            {
                // Keep metadata generation resilient.
            }

            try
            {
                description = FlowBloxResourceUtil.GetDescription(displayAttribute) ?? string.Empty;
            }
            catch
            {
                // Keep metadata generation resilient.
            }

            return (displayName, description);
        }

        private static void AddUsedEnumType(IDictionary<string, JObject> usedTypes, Type enumType)
        {
            var key = enumType.FullName ?? enumType.Name;
            if (usedTypes.ContainsKey(key))
            {
                return;
            }

            usedTypes[key] = new JObject
            {
                ["fullName"] = key,
                ["kind"] = "enum",
                ["enumMembers"] = new JArray(Enum.GetNames(enumType))
            };
        }

        private static void AddUsedReactiveObjectType(IDictionary<string, JObject> usedTypes, Type reactiveType)
        {
            var key = reactiveType.FullName ?? reactiveType.Name;
            if (usedTypes.ContainsKey(key))
            {
                return;
            }

            var supported = GetSupportedTypeInfos(reactiveType);
            usedTypes[key] = new JObject
            {
                ["fullName"] = key,
                ["kind"] = "reactiveObject",
                ["inheritanceHierarchy"] = new JArray(GetTypeHierarchy(reactiveType)),
                ["supportedTypes"] = new JArray(supported)
            };
        }

        private static JObject BuildEnumKindInfo(Type enumType)
        {
            return new JObject
            {
                ["fullName"] = enumType.FullName ?? enumType.Name,
                ["displayName"] = enumType.Name,
                ["description"] = $"Enum type '{enumType.Name}'.",
                ["inheritanceHierarchy"] = new JArray(GetTypeHierarchy(enumType)),
                ["properties"] = new JArray(),
                ["usedTypes"] = new JArray
                {
                    new JObject
                    {
                        ["fullName"] = enumType.FullName ?? enumType.Name,
                        ["kind"] = "enum",
                        ["enumMembers"] = new JArray(Enum.GetNames(enumType))
                    }
                }
            };
        }

        private static JArray BuildSpecialExplanations(Type type)
        {
            var entries = type
                .GetCustomAttributes(typeof(FlowBloxSpecialExplanationAttribute), inherit: true)
                .OfType<FlowBloxSpecialExplanationAttribute>()
                .Select(x => new
                {
                    SpecialExplanation = x.GetResolvedSpecialExplanation(),
                    x.Icon,
                    x.Color
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.SpecialExplanation))
                .ToList();

            if (!entries.Any())
            {
                return new JArray();
            }

            var flatEntries = entries
                .Select(entry => new JObject
                {
                    ["icon"] = entry.Icon.ToString(),
                    ["color"] = entry.Color ?? string.Empty,
                    ["explanation"] = entry.SpecialExplanation
                });

            return new JArray(flatEntries);
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                   || type.IsEnum
                   || type == typeof(string)
                   || type == typeof(decimal)
                   || type == typeof(DateTime)
                   || type == typeof(Guid)
                   || type == typeof(DateTimeOffset)
                   || type == typeof(TimeSpan);
        }

        private static bool IsNonComponentReactiveObjectType(Type type)
        {
            return typeof(FlowBloxReactiveObject).IsAssignableFrom(type)
                   && !typeof(FlowBloxComponent).IsAssignableFrom(type);
        }

        private static bool IsNullable(PropertyInfo property)
        {
            var type = property.PropertyType;
            if (Nullable.GetUnderlyingType(type) != null)
            {
                return true;
            }

            if (!type.IsValueType)
            {
                try
                {
                    var info = _nullabilityInfoContext.Create(property);
                    return info.ReadState == NullabilityState.Nullable || info.WriteState == NullabilityState.Nullable;
                }
                catch
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> GetTypeHierarchy(Type type)
        {
            var chain = new List<string>();
            var current = type;
            while (current != null)
            {
                chain.Add(current.FullName ?? current.Name);
                current = current.BaseType;
            }

            return chain;
        }

        public static IReadOnlyCollection<Type> ResolveExcludedBaseTypes(JToken? excludeBaseTypeToken)
        {
            var resolved = new Dictionary<string, Type>(StringComparer.Ordinal);

            void AddResolvedType(string? typeName)
            {
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    return;
                }

                var type = ResolveType(typeName);
                if (type == null)
                {
                    return;
                }

                resolved[type.FullName ?? type.Name] = type;
            }

            switch (excludeBaseTypeToken)
            {
                case JValue value:
                    AddResolvedType(value.Value<string>());
                    break;
                case JArray arr:
                    foreach (var entry in arr)
                    {
                        AddResolvedType(entry?.Value<string>());
                    }

                    break;
                case JObject:
                    // Unsupported shape; ignore to keep metadata calls resilient.
                    break;
                default:
                    AddResolvedType(excludeBaseTypeToken?.ToString());
                    break;
            }

            return resolved.Values.ToList();
        }

        private static bool ShouldIncludeProperty(
            Type ownerType,
            PropertyInfo property,
            IReadOnlyCollection<Type>? excludedBaseTypes = null)
        {
            if (HasJsonIgnore(property))
            {
                return false;
            }

            if (typeof(BaseFlowBlock).IsAssignableFrom(ownerType) && ExcludedBaseFlowBlockProperties.Contains(property.Name))
            {
                return false;
            }

            if (typeof(BaseFlowBlock).IsAssignableFrom(ownerType)
                && ownerType != typeof(BaseFlowBlock)
                && property.DeclaringType == typeof(BaseFlowBlock))
            {
                return false;
            }

            if (excludedBaseTypes != null && excludedBaseTypes.Count > 0)
            {
                var declaringType = property.DeclaringType;
                if (declaringType != null
                    && excludedBaseTypes.Any(excludedBaseType => excludedBaseType == declaringType))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ShouldExposePropertyAttribute(CustomAttributeData attributeData)
        {
            var attributeType = attributeData.AttributeType;
            if (ExcludedAttributeTypesForKindsInfo.Contains(attributeType))
            {
                return false;
            }

            if (string.Equals(attributeType.Name, "JsonIgnoreAttribute", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        private static bool HasJsonIgnore(PropertyInfo property)
        {
            return property.GetCustomAttributesData()
                .Any(x => string.Equals(x.AttributeType.Name, "JsonIgnoreAttribute", StringComparison.Ordinal));
        }

        private static IEnumerable<Type> EnumerateLoadableTypes()
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
                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }
    }
}




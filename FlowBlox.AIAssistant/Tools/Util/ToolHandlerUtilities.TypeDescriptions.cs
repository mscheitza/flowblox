using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Compression;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static partial class ToolHandlerUtilities
    {
        public static ToolResponse CreateTypeInfoResponse(
            string? fullName,
            Type mustAssignType,
            string label,
            bool includeSummarized = false,
            bool includeBaseMembers = false)
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
                includeSummarized,
                includeBaseMembers);

            return Ok(CompressTypeAliases(BuildTypeInfoResponsePayload(kind, additionalKinds)));
        }

        public static ToolResponse CreateUnifiedTypeInfoResponse(
            string? fullName,
            bool includeSummarized = false,
            bool includeBaseMembers = false)
        {
            var type = ResolveType(fullName);
            if (type == null)
            {
                return Fail($"Type '{fullName}' could not be resolved.");
            }

            if (type.IsEnum)
            {
                return Ok(CompressTypeAliases(new JObject
                {
                    ["kind"] = BuildEnumKindInfo(type)
                }));
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
                includeSummarized,
                includeBaseMembers);

            return Ok(CompressTypeAliases(BuildTypeInfoResponsePayload(kind, additionalKinds)));
        }

        private static JObject CompressTypeAliases(JObject payload)
        {
            foreach (var value in payload.DescendantsAndSelf().OfType<JValue>())
            {
                if (value.Type == JTokenType.String && value.Value is string text)
                    value.Value = AiAssistantTypeAliasHelper.CompressTypeName(text);
            }

            return payload;
        }

        private static JObject BuildTypeInfoResponsePayload(JObject kind, Dictionary<string, JObject> additionalKinds)
        {
            var payload = new JObject
            {
                ["kind"] = kind
            };

            var additionalTypeKindsInfo = new JArray(additionalKinds.Values.OrderBy(x => x.Value<string>("fullName")));
            if (additionalTypeKindsInfo.Count > 0)
                payload["additionalTypeKindsInfo"] = additionalTypeKindsInfo;

            return payload;
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
            bool includeSummarized = false,
            bool includeBaseMembers = false)
        {
            var typeDisplayMetadata = GetTypeDisplayMetadata(type);
            var usedTypes = new Dictionary<string, JObject>(StringComparer.Ordinal);
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Where(p => ShouldIncludeProperty(type, p, includeBaseMembers))
                .OrderBy(p => p.Name)
                .ToList();

            var propertyInfos = new JArray();
            var omittedLivePropertyNames = new List<string>();
            var omittedSummarizedPropertyNames = new List<string>();
            foreach (var property in properties)
            {
                if (TryGetAlreadyDescribedInLiveContext(property, out _))
                {
                    omittedLivePropertyNames.Add(property.Name);
                    continue;
                }

                if (!includeSummarized && TryGetAlreadyDescribedInSession(property, out _))
                {
                    omittedSummarizedPropertyNames.Add(property.Name);
                    continue;
                }

                propertyInfos.Add(ToPropertyInfo(
                    property,
                    includeChildren,
                    additionalKinds,
                    usedTypes,
                    includeSummarized,
                    type,
                    includeBaseMembers));
            }

            var result = new JObject
            {
                ["fullName"] = type.FullName ?? type.Name,
                ["displayName"] = typeDisplayMetadata.DisplayName,
                ["description"] = typeDisplayMetadata.Description,
                ["inheritanceHierarchy"] = new JArray(GetTypeHierarchy(type)),
                ["properties"] = propertyInfos
            };

            if (omittedLivePropertyNames.Count > 0 || omittedSummarizedPropertyNames.Count > 0)
            {
                var omittedPropertyMessages = new List<string>();
                if (omittedLivePropertyNames.Count > 0)
                {
                    omittedPropertyMessages.Add(
                        $"Already present in the available message history: {string.Join(", ", omittedLivePropertyNames)}.");
                }

                if (omittedSummarizedPropertyNames.Count > 0)
                {
                    omittedPropertyMessages.Add(
                        $"Sent earlier in this session: {string.Join(", ", omittedSummarizedPropertyNames)}. " +
                        "Use includeSummarized=true only if the summary lacks required details.");
                }

                result["omittedPropertyDetails"] = string.Join(" ", omittedPropertyMessages);
            }

            var usedTypeInfos = new JArray(usedTypes.Values.OrderBy(x => x.Value<string>("fullName")));
            if (usedTypeInfos.Count > 0)
                result["usedTypes"] = usedTypeInfos;

            var specialExplanations = BuildSpecialExplanations(type);
            if (specialExplanations.Count > 0)
            {
                result["specialExplanations"] = specialExplanations;
            }

            if (isTopLevel && propertyInfos.Count > 0)
            {
                result["rules"] = new JObject
                {
                    ["fieldPlaceholderSyntax"] = "$FlowBlock::FieldName",
                    ["placeholderLookupTool"] = "GetPlaceholders",
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
            bool includeSummarized = false,
            Type? describedInType = null,
            bool includeBaseMembers = false)
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
            var majorTypeDescriptor = GetMajorTypeDescriptor(nonNullableType, "SimpleProperty");

            var flowBlockUi = property.GetCustomAttribute<FlowBloxUIAttribute>();

            var propertyInfo = new JObject
            {
                ["name"] = property.Name,
                ["displayName"] = string.IsNullOrWhiteSpace(displayName) ? property.Name : displayName,
                ["type"] = AiAssistantGenericTypeNameHelper.GetCompactTypeName(propertyType),
                ["flags"] = BuildPropertyFlags(nullable, property.CanWrite, isSimple, isEnum, isCollection),
                ["majorTypeDescriptor"] = majorTypeDescriptor,
                ["canUpdateViaParentPath"] = !isManagedObject
            };

            if (!string.IsNullOrWhiteSpace(description))
                propertyInfo["description"] = description;

            if (flowBlockUi != null)
            {
                var uiFlags = BuildUiFlags(flowBlockUi.Operations, flowBlockUi.UiOptions);
                if (!string.IsNullOrEmpty(uiFlags))
                    propertyInfo["uiFlags"] = uiFlags;

                var uiInfo = new JObject();

                var creatableTypes = new JArray((flowBlockUi.CreatableTypes ?? Array.Empty<Type>()).Select(x => x.FullName ?? x.Name));
                if (creatableTypes.Count > 0)
                    uiInfo["creatableTypes"] = creatableTypes;

                if (flowBlockUi.ReadOnly)
                    uiInfo["readOnly"] = true;

                if (!string.IsNullOrWhiteSpace(flowBlockUi.ToolboxCategory))
                    uiInfo["toolboxCategory"] = flowBlockUi.ToolboxCategory;

                if (uiInfo.HasValues)
                    propertyInfo["ui"] = uiInfo;
            }

            if (isFlowBlockReference)
                propertyInfo["isFlowBlockReference"] = true;

            var attributes = AiAssistantAttributeFormatter.Format(property.GetCustomAttributesData());
            if (!string.IsNullOrEmpty(attributes))
                propertyInfo["attributes"] = attributes;

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
                        includeSummarized,
                        includeBaseMembers);
                }
            }

            if (IsNonComponentReactiveObjectType(nonNullableType))
            {
                AddUsedReactiveObjectType(usedTypes, nonNullableType);
            }

            MarkPropertyAsDescribed(property, describedInType?.FullName ?? describedInType?.Name ?? "unknown");

            return propertyInfo;
        }

        private static string BuildPropertyFlags(
            bool nullable,
            bool canWrite,
            bool isSimple,
            bool isEnum,
            bool isCollection)
        {
            var flags = string.Empty;

            if (nullable)
                flags += "N";
            if (canWrite)
                flags += "W";
            if (isSimple)
                flags += "S";
            if (isEnum)
                flags += "E";
            if (isCollection)
                flags += "C";

            return flags;
        }

        internal static string BuildUiFlags(UIOperations operations, UIOptions options)
        {
            var flags = string.Empty;

            if (operations.HasFlag(UIOperations.Create))
                flags += "C";
            if (operations.HasFlag(UIOperations.Link))
                flags += "L";
            if (operations.HasFlag(UIOperations.Unlink))
                flags += "U";
            if (operations.HasFlag(UIOperations.Edit))
                flags += "E";
            if (operations.HasFlag(UIOperations.Delete))
                flags += "D";

            if (options.HasFlag(UIOptions.EnableFieldSelection))
                flags += "[FLD]";
            if (options.HasFlag(UIOptions.EnableFileSelection))
                flags += "[FL]";
            if (options.HasFlag(UIOptions.EnableFolderSelection))
                flags += "[DI]";

            return flags;
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
                ["majorTypeDescriptor"] = GetMajorTypeDescriptor(type)
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
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Error(
                    "Failed to resolve a localized display name for AI Assistant type metadata.",
                    ex);
            }

            try
            {
                description = FlowBloxResourceUtil.GetDescription(displayAttribute) ?? string.Empty;
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Error(
                    "Failed to resolve a localized description for AI Assistant type metadata.",
                    ex);
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
                    x.Icon
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
                catch (Exception ex)
                {
                    FlowBloxLogManager.Instance.GetLogger().Error(
                        $"Failed to resolve nullability metadata for property '{property.Name}'.",
                        ex);
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
                if (current == typeof(object))
                {
                    break;
                }

                chain.Add(current.FullName ?? current.Name);
                current = current.BaseType;
            }

            return chain;
        }

        private static string? GetMajorTypeDescriptor(Type type, string? defaultMajorTypeDescriptor = null)
        {
            if (typeof(FieldElement).IsAssignableFrom(type))
                return "FieldElement";

            if (typeof(IManagedObject).IsAssignableFrom(type))
                return "ManagedObject";

            if (typeof(BaseFlowBlock).IsAssignableFrom(type))
                return "FlowBlock";

            if (typeof(FlowBloxReactiveObject).IsAssignableFrom(type))
                return "ReactiveObject";

            return defaultMajorTypeDescriptor;
        }

        public static string GetMajorTypeDescriptorForType(Type type, string defaultMajorTypeDescriptor = "ReactiveObject")
        {
            return GetMajorTypeDescriptor(type, defaultMajorTypeDescriptor) ?? defaultMajorTypeDescriptor;
        }

        private static bool ShouldIncludeProperty(
            Type ownerType,
            PropertyInfo property,
            bool includeBaseMembers)
        {
            if (HasJsonIgnore(property))
            {
                return false;
            }

            if (!includeBaseMembers
                && typeof(BaseFlowBlock).IsAssignableFrom(ownerType)
                && property.DeclaringType != ownerType
                && property.DeclaringType != null
                && FlowBloxFrameworkBaseFlowBlockTypes.Contains(property.DeclaringType))
            {
                return false;
            }

            if (typeof(FlowBloxComponent).IsAssignableFrom(ownerType)
                && ExcludedFlowBloxComponentProperties.Contains(property.Name))
            {
                return false;
            }

            if (typeof(BaseFlowBlock).IsAssignableFrom(ownerType)
                && ExcludedBaseFlowBlockProperties.Contains(property.Name))
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
                    FlowBloxLogManager.Instance.GetLogger().Error(
                        $"Some types could not be loaded from assembly '{assembly.FullName}' while resolving AI Assistant type metadata.",
                        ex);
                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }
                catch (Exception ex)
                {
                    FlowBloxLogManager.Instance.GetLogger().Error(
                        $"Failed to enumerate types from assembly '{assembly.FullName}' while resolving AI Assistant type metadata.",
                        ex);
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

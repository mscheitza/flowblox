using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Reflection;
using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Grid.Elements.Util;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal static partial class ToolHandlerUtilities
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<PropertyCacheKey, string>> PropertyDescriptionCacheBySession =
            new(StringComparer.Ordinal);
        private static volatile string? _currentSessionGuid;

        private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

        private static readonly HashSet<string> ExcludedBaseFlowBlockProperties = new(StringComparer.Ordinal)
        {
            nameof(BaseFlowBlock.Parent),
            nameof(BaseFlowBlock.TestDefinitions),
            nameof(BaseFlowBlock.GenerationStrategies),
            nameof(BaseFlowBlock.BreakPoint),
            nameof(FlowBloxReactiveObject.Icon16),
            nameof(FlowBloxReactiveObject.Icon32),
            nameof(BaseFlowBlock.NotificationTypes),
            nameof(BaseFlowBlock.OverriddenNotificationEntries),
            nameof(BaseFlowBlock.HasInputReference),
            nameof(BaseFlowBlock.DefinedManagedObjects),
            nameof(BaseFlowBlock.HandleRequirements),
            nameof(BaseFlowBlock.HasErrors)
        };

        private static readonly HashSet<Type> ExcludedAttributeTypesForKindsInfo = new()
        {
            typeof(DisplayAttribute),
            typeof(FlowBloxUIAttribute),
            typeof(FlowBloxListViewAttribute),
            typeof(FlowBloxDataGridAttribute),
            typeof(FlowBloxTextBoxAttribute)
        };

        public static ToolDefinition CreateDefinition(string name, string description, JObject? argumentsSchema = null)
        {
            return new ToolDefinition
            {
                Name = name,
                Description = description,
                ArgumentsSchema = argumentsSchema ?? new JObject()
            };
        }

        public static ToolResponse Ok(JObject result)
        {
            return new ToolResponse
            {
                Ok = true,
                Result = result
            };
        }

        public static ToolResponse Fail(string error, JObject? result = null)
        {
            return new ToolResponse
            {
                Ok = false,
                Error = error,
                Result = result ?? new JObject()
            };
        }

        public static void SetCurrentSessionGuid(string? sessionGuid)
        {
            _currentSessionGuid = string.IsNullOrWhiteSpace(sessionGuid)
                ? null
                : sessionGuid.Trim();
        }

        public static void ClearSessionCache(string? sessionGuid = null)
        {
            var key = string.IsNullOrWhiteSpace(sessionGuid)
                ? _currentSessionGuid
                : sessionGuid.Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                PropertyDescriptionCacheBySession.Clear();
                return;
            }

            PropertyDescriptionCacheBySession.TryRemove(key, out _);
        }

        private static string GetCurrentSessionCacheKey()
        {
            if (!string.IsNullOrWhiteSpace(_currentSessionGuid))
                return _currentSessionGuid!;

            return "default-session";
        }

        private static bool TryGetAlreadyDescribedInType(PropertyInfo property, out string describedInType)
        {
            describedInType = string.Empty;

            var sessionKey = GetCurrentSessionCacheKey();
            if (!PropertyDescriptionCacheBySession.TryGetValue(sessionKey, out var sessionCache))
                return false;

            return sessionCache.TryGetValue(PropertyCacheKey.FromProperty(property), out describedInType);
        }

        private static void MarkPropertyAsDescribed(PropertyInfo property, string describedInType)
        {
            if (string.IsNullOrWhiteSpace(describedInType))
                return;

            var sessionKey = GetCurrentSessionCacheKey();
            var sessionCache = PropertyDescriptionCacheBySession.GetOrAdd(
                sessionKey,
                _ => new ConcurrentDictionary<PropertyCacheKey, string>());

            sessionCache.TryAdd(PropertyCacheKey.FromProperty(property), describedInType);
        }

        public static FlowBloxRegistry GetRegistry()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                   ?? throw new InvalidOperationException("FlowBlox registry is not available.");
        }

        public static FlowBloxProject GetProject()
        {
            return FlowBloxProjectManager.Instance.ActiveProject
                   ?? throw new InvalidOperationException("No active project is loaded.");
        }

        public static string[] PathOf(FlowBlockCategory category)
        {
            var stack = new Stack<string>();
            var current = category;
            while (current != null)
            {
                stack.Push(current.DisplayName);
                current = current.ParentCategory;
            }

            return stack.ToArray();
        }

        public static Type? ResolveType(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            var requested = fullName.Trim();

            var direct = Type.GetType(requested, throwOnError: false);
            if (direct != null)
                return direct;

            var commaIndex = requested.IndexOf(',');
            var typeNameOnly = commaIndex > 0
                ? requested[..commaIndex].Trim()
                : requested;

            var resolved = ReflectionHelper.GetTypeByClass(typeNameOnly);
            if (resolved != null)
                return resolved;

            return ReflectionHelper.GetTypeByClass(requested);
        }

        public static JObject ToTypeInfo(FlowBloxReactiveObject instance)
        {
            return new JObject
            {
                ["fullName"] = instance.GetType().FullName ?? instance.GetType().Name,
                ["displayName"] = FlowBloxComponentHelper.GetDisplayName(instance),
                ["description"] = FlowBloxComponentHelper.GetDescription(instance)
            };
        }

        public static IManagedObject? ResolveManagedObjectByName(FlowBloxRegistry registry, string? managedObjectName)
        {
            if (string.IsNullOrWhiteSpace(managedObjectName))
            {
                return null;
            }

            return registry.GetManagedObjects()
                .FirstOrDefault(x => string.Equals(x.Name, managedObjectName, StringComparison.OrdinalIgnoreCase));
        }

        public static FieldElement? ResolveFieldElementByFQName(FlowBloxRegistry registry, string? fullyQualifiedFieldName)
        {
            if (string.IsNullOrWhiteSpace(fullyQualifiedFieldName))
            {
                return null;
            }

            return registry.GetFieldElementOrNull(fullyQualifiedFieldName);
        }

        public static BaseFlowBlock? ResolveFlowBlockByName(FlowBloxRegistry registry, string? flowBlockName)
        {
            if (string.IsNullOrWhiteSpace(flowBlockName))
            {
                return null;
            }

            return registry.GetFlowBlocks()
                .FirstOrDefault(x => string.Equals(x.Name, flowBlockName, StringComparison.OrdinalIgnoreCase));
        }

        public static Type? GetElementType(Type type)
        {
            if (type == typeof(string))
            {
                return null;
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsGenericType
                && type.GetGenericArguments().Length == 1
                && typeof(IEnumerable).IsAssignableFrom(type))
            {
                return type.GetGenericArguments()[0];
            }

            var enumerableInterface = type
                .GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableInterface?.GetGenericArguments()[0];
        }

        private sealed class PathSegment
        {
            public bool IsProperty { get; set; }
            public string? PropertyName { get; set; }
            public int Index { get; set; }
            public string? ExplicitTypeFullName { get; set; }
        }

        private sealed class UpdateOperation
        {
            public string Path { get; set; } = string.Empty;
            public JToken Value { get; set; } = JValue.CreateNull();
        }

        private sealed class PropertyCacheKey : IEquatable<PropertyCacheKey>
        {
            public string PropertyName { get; init; } = string.Empty;
            public string DeclaringTypeName { get; init; } = string.Empty;

            public static PropertyCacheKey FromProperty(PropertyInfo property)
            {
                return new PropertyCacheKey
                {
                    PropertyName = property.Name ?? string.Empty,
                    DeclaringTypeName = property.DeclaringType?.FullName ?? property.DeclaringType?.Name ?? string.Empty
                };
            }

            public bool Equals(PropertyCacheKey? other)
            {
                if (other == null)
                    return false;

                return string.Equals(PropertyName, other.PropertyName, StringComparison.Ordinal) && 
                       string.Equals(DeclaringTypeName, other.DeclaringTypeName, StringComparison.Ordinal);
            }

            public override bool Equals(object? obj) => Equals(obj as PropertyCacheKey);

            public override int GetHashCode() => HashCode.Combine(PropertyName, DeclaringTypeName);
        }
    }
}

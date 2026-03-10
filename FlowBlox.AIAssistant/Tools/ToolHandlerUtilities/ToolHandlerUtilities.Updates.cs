using System.Collections;
using System.Reflection;
using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Registry;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal static partial class ToolHandlerUtilities
    {
        public static ToolResponse ApplyUpdates(JObject args, object target, FlowBloxRegistry registry, string kind, string name)
        {
            var updates = CollectUpdates(args);
            if (updates.Count == 0)
            {
                return Fail("No update operations provided. Use path/value or updates[].");
            }

            var applied = new JArray();

            foreach (var update in updates)
            {
                try
                {
                    SetByJsonPath(target, update.Path, update.Value, registry);
                    applied.Add(new JObject
                    {
                        ["path"] = update.Path,
                        ["ok"] = true
                    });
                }
                catch (Exception ex)
                {
                    applied.Add(new JObject
                    {
                        ["path"] = update.Path,
                        ["ok"] = false,
                        ["error"] = ex.Message
                    });

                    var payload = new JObject
                    {
                        ["updated"] = false,
                        ["kind"] = kind,
                        ["name"] = name,
                        ["applied"] = applied
                    };

                    return Fail($"Update failed at path '{update.Path}': {ex.Message}", payload);
                }
            }

            return Ok(new JObject
            {
                ["updated"] = true,
                ["kind"] = kind,
                ["name"] = name,
                ["applied"] = applied,
                ["count"] = applied.Count
            });
        }

        private static List<UpdateOperation> CollectUpdates(JObject args)
        {
            var updates = new List<UpdateOperation>();

            var singlePath = args.Value<string>("path");
            if (!string.IsNullOrWhiteSpace(singlePath) && args["value"] != null)
            {
                updates.Add(new UpdateOperation
                {
                    Path = singlePath,
                    Value = args["value"]!
                });
            }

            if (args["updates"] is JArray updatesArray)
            {
                foreach (var updateItem in updatesArray.OfType<JObject>())
                {
                    var updatePath = updateItem.Value<string>("path");
                    if (string.IsNullOrWhiteSpace(updatePath) || updateItem["value"] == null)
                    {
                        continue;
                    }

                    updates.Add(new UpdateOperation
                    {
                        Path = updatePath,
                        Value = updateItem["value"]!
                    });
                }
            }

            return updates;
        }

        private static void SetByJsonPath(object root, string path, JToken value, FlowBloxRegistry registry)
        {
            var pathSegments = ParsePath(path);
            if (pathSegments.Count == 0)
            {
                throw new InvalidOperationException("Path is empty.");
            }

            object current = root;
            for (var i = 0; i < pathSegments.Count; i++)
            {
                var segment = pathSegments[i];
                var isLast = i == pathSegments.Count - 1;

                if (segment.IsProperty)
                {
                    var property = current.GetType().GetProperty(segment.PropertyName!, BindingFlags.Public | BindingFlags.Instance)
                                   ?? throw new InvalidOperationException($"Property '{segment.PropertyName}' not found on '{current.GetType().FullName}'.");

                    var propertyType = property.PropertyType;
                    var propertyTargetType = GetElementType(propertyType) ?? propertyType;
                    if (!isLast && typeof(IManagedObject).IsAssignableFrom(propertyTargetType))
                    {
                        throw new InvalidOperationException(
                            $"Path segment '{segment.PropertyName}' targets a managed object. " +
                            "Indirect updates are not allowed. Update that managed object directly with UpdateManagedObject.");
                    }

                    if (isLast)
                    {
                        EnsureNoDirectReactiveObjectJsonAssignment(property.PropertyType, value, path);
                        property.SetValue(current, ConvertToken(value, property.PropertyType, registry));
                        return;
                    }

                    var propertyValue = property.GetValue(current);
                    if (propertyValue == null)
                    {
                        propertyValue = CreateInstance(property.PropertyType);
                        property.SetValue(current, propertyValue);
                    }

                    current = propertyValue;
                    continue;
                }

                if (current is not IList list)
                {
                    throw new InvalidOperationException($"Index segment '{segment.Index}' requires list-like target.");
                }

                var elementType = GetElementType(current.GetType())
                                  ?? throw new InvalidOperationException("Could not infer list element type.");
                if (!isLast && typeof(IManagedObject).IsAssignableFrom(elementType))
                {
                    throw new InvalidOperationException(
                        $"Path segment '[{segment.Index}]' targets a managed object. " +
                        "Indirect updates are not allowed. Update that managed object directly with UpdateManagedObject.");
                }

                while (list.Count <= segment.Index)
                {
                    list.Add(CreateCollectionElementInstance(elementType));
                }

                if (isLast)
                {
                    EnsureNoDirectReactiveObjectJsonAssignment(elementType, value, path);
                    list[segment.Index] = ConvertToken(value, elementType, registry);
                    return;
                }

                var indexedValue = list[segment.Index];
                if (indexedValue == null)
                {
                    indexedValue = CreateInstance(elementType);
                    list[segment.Index] = indexedValue;
                }

                current = indexedValue;
            }
        }

        private static List<PathSegment> ParsePath(string rawPath)
        {
            var path = rawPath?.Trim() ?? string.Empty;
            var segments = new List<PathSegment>();

            if (string.IsNullOrWhiteSpace(path))
            {
                return segments;
            }

            if (path.StartsWith('/'))
            {
                foreach (var token in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (int.TryParse(token, out var index))
                    {
                        segments.Add(new PathSegment { IsProperty = false, Index = index });
                    }
                    else
                    {
                        segments.Add(new PathSegment { IsProperty = true, PropertyName = token });
                    }
                }

                return segments;
            }

            foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedPart = part.Trim();
                var bracketStart = trimmedPart.IndexOf('[');

                if (bracketStart < 0)
                {
                    segments.Add(new PathSegment
                    {
                        IsProperty = true,
                        PropertyName = trimmedPart
                    });
                    continue;
                }

                segments.Add(new PathSegment
                {
                    IsProperty = true,
                    PropertyName = trimmedPart.Substring(0, bracketStart)
                });

                var remaining = trimmedPart.Substring(bracketStart);
                while (remaining.StartsWith("[", StringComparison.Ordinal))
                {
                    var bracketEnd = remaining.IndexOf(']');
                    var indexText = remaining.Substring(1, bracketEnd - 1);
                    if (!int.TryParse(indexText, out var index))
                    {
                        throw new InvalidOperationException($"Invalid index '{indexText}'.");
                    }

                    segments.Add(new PathSegment
                    {
                        IsProperty = false,
                        Index = index
                    });

                    remaining = remaining.Substring(bracketEnd + 1);
                }
            }

            return segments;
        }

        private static object? ConvertToken(JToken token, Type targetType, FlowBloxRegistry registry)
        {
            if (token.Type == JTokenType.Null)
            {
                return null;
            }

            var nonNullableTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (nonNullableTargetType == typeof(string))
            {
                return token.Value<string>();
            }

            if (nonNullableTargetType.IsEnum)
            {
                return ParseEnumValue(token, nonNullableTargetType);
            }

            if (typeof(BaseFlowBlock).IsAssignableFrom(nonNullableTargetType))
            {
                var resolvedFlowBlock = ResolveFlowBlockReference(token, registry);
                if (resolvedFlowBlock == null)
                {
                    throw new InvalidOperationException(
                        "Flow block reference could not be resolved. " +
                        "Use a flow block name string or {\"resolveFlowBlockByName\":\"FlowBlockName\"}.");
                }

                return resolvedFlowBlock;
            }

            if (typeof(IManagedObject).IsAssignableFrom(nonNullableTargetType))
            {
                var resolvedManagedObject = ResolveManagedObjectReference(token, registry);
                if (resolvedManagedObject == null)
                {
                    throw new InvalidOperationException(
                        "Managed object reference could not be resolved. " +
                        "Use {\"resolveManagedObjectByName\":\"...\"} or {\"resolveFieldElementByFQName\":\"$FlowBlock::FieldName\"}.");
                }

                if (!nonNullableTargetType.IsAssignableFrom(resolvedManagedObject.GetType()))
                {
                    throw new InvalidOperationException(
                        $"Resolved reference type '{resolvedManagedObject.GetType().FullName}' is not assignable to '{nonNullableTargetType.FullName}'.");
                }

                return resolvedManagedObject;
            }

            if (token.Type == JTokenType.Object && token is JObject objectToken)
            {
                var explicitTypeFullName = objectToken.Value<string>("$type")
                                           ?? objectToken.Value<string>("typeFullName");
                if (!string.IsNullOrWhiteSpace(explicitTypeFullName))
                {
                    var explicitType = ResolveType(explicitTypeFullName);
                    if (explicitType == null || !nonNullableTargetType.IsAssignableFrom(explicitType))
                    {
                        throw new InvalidOperationException(
                            $"Explicit type '{explicitTypeFullName}' is not assignable to '{nonNullableTargetType.FullName}'.");
                    }

                    return objectToken.ToObject(explicitType);
                }
            }

            return token.ToObject(targetType);
        }

        private static object ParseEnumValue(JToken token, Type enumType)
        {
            if (token.Type == JTokenType.Integer)
            {
                var underlyingType = Enum.GetUnderlyingType(enumType);
                var numericValue = Convert.ChangeType(token.ToObject(underlyingType), underlyingType);
                return Enum.ToObject(enumType, numericValue!);
            }

            if (token.Type == JTokenType.String)
            {
                var enumText = token.Value<string>() ?? string.Empty;
                return Enum.Parse(enumType, enumText, true);
            }

            if (token.Type == JTokenType.Object && token is JObject enumObject)
            {
                var enumText = enumObject.Value<string>("value")
                               ?? enumObject.Value<string>("name");
                if (!string.IsNullOrWhiteSpace(enumText))
                {
                    return Enum.Parse(enumType, enumText, true);
                }
            }

            throw new InvalidOperationException(
                $"Unsupported enum token '{token.Type}'. Use enum member name string, integer value, or {{\"value\":\"Member\"}}.");
        }

        private static IManagedObject? ResolveManagedObjectReference(JToken token, FlowBloxRegistry registry)
        {
            if (token.Type == JTokenType.Object && token is JObject objToken)
            {
                var managedObjectName = objToken.Value<string>("resolveManagedObjectByName");
                if (!string.IsNullOrWhiteSpace(managedObjectName))
                    return ResolveManagedObjectByName(registry, managedObjectName);

                var fieldFQName = objToken.Value<string>("resolveFieldElementByFQName");
                if (!string.IsNullOrWhiteSpace(fieldFQName))
                    return ResolveFieldElementByFQName(registry, fieldFQName);
            }

            if (token.Type == JTokenType.String)
            {
                var raw = token.Value<string>();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return null;
                }

                if (raw.StartsWith("$", StringComparison.Ordinal))
                {
                    return ResolveFieldElementByFQName(registry, raw);
                }

                return ResolveManagedObjectByName(registry, raw);
            }

            return null;
        }

        private static BaseFlowBlock? ResolveFlowBlockReference(JToken token, FlowBloxRegistry registry)
        {
            if (token.Type == JTokenType.Object && token is JObject objToken)
            {
                var flowBlockName = objToken.Value<string>("resolveFlowBlockByName");
                return ResolveFlowBlockByName(registry, flowBlockName);
            }

            if (token.Type == JTokenType.String)
            {
                return ResolveFlowBlockByName(registry, token.Value<string>());
            }

            return null;
        }

        private static object CreateInstance(Type type)
        {
            if (type.IsInterface
                && type.IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(IList<>)
                    || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                var elementType = type.GetGenericArguments()[0];
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type)!;
            }

            return Activator.CreateInstance(type)
                   ?? throw new InvalidOperationException($"Could not create instance of '{type.FullName}'.");
        }

        private static object CreateCollectionElementInstance(Type elementType)
        {
            if (!elementType.IsAbstract && !elementType.IsInterface)
            {
                return CreateInstance(elementType);
            }

            if (IsNonComponentReactiveObjectType(elementType))
            {
                var supportedTypes = GetSupportedTypeInfos(elementType)
                    .Select(x => x.Value<string>("fullName"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (supportedTypes.Count == 1)
                {
                    var resolvedType = ResolveType(supportedTypes[0]!);
                    if (resolvedType != null)
                    {
                        return CreateInstance(resolvedType);
                    }
                }

                throw new InvalidOperationException(
                    $"Collection element type '{elementType.FullName}' is abstract/interface. " +
                    "Use GetSupportedTypes for this base type and set the element explicitly with a typed object: " +
                    "{\"$type\":\"Full.Type.Name\", ...}.");
            }

            throw new InvalidOperationException(
                $"Collection element type '{elementType.FullName}' is abstract/interface and cannot be auto-created.");
        }

        private static void EnsureNoDirectReactiveObjectJsonAssignment(Type targetType, JToken value, string path)
        {
            var nonNullableTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var elementType = GetElementType(nonNullableTargetType);
            var valueTargetType = Nullable.GetUnderlyingType(elementType ?? nonNullableTargetType) ?? (elementType ?? nonNullableTargetType);

            if (!IsNonComponentReactiveObjectType(valueTargetType))
                return;

            if (value.Type != JTokenType.Object && value.Type != JTokenType.Array)
                return;

            throw new InvalidOperationException(
                $"Path '{path}' targets a FlowBloxReactiveObject-only type ('{valueTargetType.FullName}'). " +
                "Do not assign JSON objects/arrays directly in 'value'. " +
                "Use path-based updates for sub-properties so Get-or-Create can materialize intermediate objects, " +
                "e.g. '/MappingEntries/0/ColumnName' and '/MappingEntries/0/Field' with " +
                "{\"resolveFieldElementByFQName\":\"$ReadExcelRows::Last Name\"}.");
        }
    }
}

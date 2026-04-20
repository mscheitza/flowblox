using System.Collections;
using System.Reflection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal static partial class ToolHandlerUtilities
    {
        public static JObject CreateSnapshot(string kind, string name, string? typeFullName, object root, IEnumerable<string>? referencedFlowBlockNames)
        {
            var snapshot = BuildSnapshotToken(root, new HashSet<object>(ReferenceEqualityComparer.Instance), 0) as JObject
                           ?? new JObject();

            if (referencedFlowBlockNames != null)
            {
                snapshot["ReferencedFlowBlocks"] = new JArray(referencedFlowBlockNames);
            }

            return new JObject
            {
                ["kind"] = kind,
                ["name"] = name,
                ["typeFullName"] = typeFullName,
                ["snapshot"] = snapshot
            };
        }

        private static JToken BuildSnapshotToken(object? value, HashSet<object> visited, int depth)
        {
            if (value == null)
            {
                return JValue.CreateNull();
            }

            var type = value.GetType();
            if (IsSimpleType(type))
            {
                return JToken.FromObject(value);
            }

            if (value is BaseFlowBlock flowBlock)
            {
                return flowBlock.Name;
            }

            if (value is FieldElement fieldElement)
            {
                return fieldElement.FullyQualifiedName;
            }

            if (value is IManagedObject managedObject)
            {
                return managedObject.Name;
            }

            if (depth > 8 || !visited.Add(value))
            {
                return new JObject { ["$ref"] = "circular-or-maxdepth" };
            }

            if (value is IEnumerable enumerable && value is not string)
            {
                var array = new JArray();
                foreach (var item in enumerable)
                {
                    array.Add(BuildSnapshotToken(item, visited, depth + 1));
                }

                return array;
            }

            if (value is FlowBloxReactiveObject reactiveObject)
            {
                var result = new JObject
                {
                    ["$type"] = type.FullName ?? type.Name
                };

                var properties = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

                foreach (var property in properties)
                {
                    try
                    {
                        result[property.Name] = BuildSnapshotToken(property.GetValue(reactiveObject), visited, depth + 1);
                    }
                    catch
                    {
                        // Snapshot generation should stay resilient for inaccessible properties.
                    }
                }

                return result;
            }

            return JToken.FromObject(
                value,
                JsonSerializer.Create(new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.None
                }));
        }
    }
}

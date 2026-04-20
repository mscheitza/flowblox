using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal static partial class ToolHandlerUtilities
    {
        public static JArray BuildBlockedBy(IEnumerable<IFlowBloxComponent>? dependencies)
        {
            var blockedBy = new JArray();
            if (dependencies == null)
                return blockedBy;

            var emittedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dependency in dependencies.Where(x => x != null))
            {
                var majorTypeDescriptor = GetMajorTypeDescriptorForType(dependency.GetType());
                var dependencyName = GetDependencyName(dependency);
                var entryKey = $"{majorTypeDescriptor}|{dependencyName}";
                if (!emittedKeys.Add(entryKey))
                    continue;

                blockedBy.Add(new JObject
                {
                    ["majorTypeDescriptor"] = majorTypeDescriptor,
                    ["name"] = dependencyName
                });
            }

            return blockedBy;
        }

        private static string GetDependencyName(IFlowBloxComponent dependency)
        {
            if (dependency is FieldElement fieldElement
                && !string.IsNullOrWhiteSpace(fieldElement.FullyQualifiedName))
            {
                return fieldElement.FullyQualifiedName;
            }

            return string.IsNullOrWhiteSpace(dependency.Name)
                ? dependency.GetType().Name
                : dependency.Name;
        }
    }
}

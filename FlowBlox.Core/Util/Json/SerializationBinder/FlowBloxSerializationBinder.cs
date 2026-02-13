using FlowBlox.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Loader;

namespace FlowBlox.Core.Util.Json.SerializationBinder
{
    public class FlowBloxSerializationBinder : DefaultSerializationBinder
    {
        private readonly Dictionary<string, AssemblyLoadContext> _loadContexts;

        private static readonly Dictionary<string, string> _typeAliases = new(StringComparer.Ordinal)
        {
            {
                "FlowBlox.Core.Models.FlowBlocks.Additions.Condition, FlowBlox.Core",
                "FlowBlox.Core.Models.FlowBlocks.Additions.ComparisonCondition, FlowBlox.Core"
            },
            {
                "FlowBlox.Core.Models.FlowBlocks.Additions.FieldCondition, FlowBlox.Core",
                "FlowBlox.Core.Models.FlowBlocks.Additions.FieldComparisonCondition, FlowBlox.Core"
            },
            {
                "FlowBlox.Core.Models.FlowBlocks.Additions.SummarizationCondition, FlowBlox.Core",
                "FlowBlox.Core.Models.FlowBlocks.Additions.LogicalGroupCondition, FlowBlox.Core"
            }
        };

        private static readonly ILogger _logger = FlowBloxLogManager.Instance.GetLogger();

        public FlowBloxSerializationBinder(Dictionary<string, AssemblyLoadContext> loadContexts)
        {
            _loadContexts = loadContexts ?? new Dictionary<string, AssemblyLoadContext>();
        }

        private static string ApplyTypeAliases(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return typeName;

            foreach (var kvp in _typeAliases)
            {
                if (typeName.Contains(kvp.Key, StringComparison.Ordinal))
                    typeName = typeName.Replace(kvp.Key, kvp.Value, StringComparison.Ordinal);
            }

            return typeName;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            // Try custom AssemblyLoadContexts with full type name
            foreach (var context in _loadContexts.Values)
            {
                var assembly = context.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName);
                if (assembly != null)
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                        return type;

                    // Try simple-name fallback in the target assembly loaded in the load context
                    var bySimpleName = TypeNameHelper.FindTypeBySimpleName(assembly, TypeNameHelper.GetSimpleTypeName(typeName));
                    if (bySimpleName != null)
                        return bySimpleName;

                }
            }

            typeName = ApplyTypeAliases(typeName);

            Type? resolved = null;
            try
            {
                resolved = base.BindToType(assemblyName, typeName);
                if (resolved != null)
                    return resolved;
            }
            catch (JsonSerializationException ex)
            {
                _logger.Warn(
                    $"FlowBloxSerializationBinder: Failed to bind type '{typeName}' from assembly '{assemblyName}'. Falling back to simple-name resolution.", ex);
            }

            // Try simple-name fallback in the target assembly loaded in the default context
            if (!string.IsNullOrEmpty(assemblyName))
            {
                var assembly = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == assemblyName);

                if (assembly != null)
                {
                    var bySimpleName = TypeNameHelper.FindTypeBySimpleName(assembly, TypeNameHelper.GetSimpleTypeName(typeName));
                    if (bySimpleName != null)
                        return bySimpleName;
                }
            }

            throw new JsonSerializationException(
                $"FlowBloxSerializationBinder: Could not resolve type '{typeName}' in assembly '{assemblyName}' even after namespace fallback search by simple type name.");
        }
    }
}

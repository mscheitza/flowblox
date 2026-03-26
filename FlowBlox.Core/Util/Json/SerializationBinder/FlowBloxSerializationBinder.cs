using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Loader;

namespace FlowBlox.Core.Util.Json.SerializationBinder
{
    public class FlowBloxSerializationBinder : DefaultSerializationBinder
    {
        private readonly Dictionary<string, AssemblyLoadContext> _loadContexts;
        private readonly Dictionary<string, string> _typeAliases;

        private static readonly ILogger _logger = FlowBloxLogManager.Instance.GetLogger();

        public FlowBloxSerializationBinder(
            Dictionary<string, AssemblyLoadContext> loadContexts,
            IEnumerable<FlowBloxLegacyTypeMapping>? legacyTypeMappings = null)
        {
            _loadContexts = loadContexts ?? new Dictionary<string, AssemblyLoadContext>();
            _typeAliases = BuildTypeAliasMap(legacyTypeMappings);
        }

        private static Dictionary<string, string> BuildTypeAliasMap(IEnumerable<FlowBloxLegacyTypeMapping>? legacyTypeMappings)
        {
            var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
            if (legacyTypeMappings == null)
                return aliases;

            foreach (var mapping in legacyTypeMappings)
            {
                if (mapping?.TargetType == null)
                    continue;

                var targetTypeName = mapping.TargetAssemblyQualifiedTypeName;
                foreach (var legacyTypeName in mapping.LegacyTypeNames)
                {
                    if (string.IsNullOrWhiteSpace(legacyTypeName))
                        continue;

                    aliases[legacyTypeName] = targetTypeName;
                }
            }

            return aliases;
        }

        private string ApplyTypeAliases(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return typeName;

            foreach (var kvp in _typeAliases)
            {
                if (typeName.Contains(kvp.Key, StringComparison.Ordinal))
                    typeName = typeName.Replace(kvp.Key, kvp.Value, StringComparison.Ordinal);

                var normalizedKey = NormalizeTypeName(kvp.Key);
                var normalizedValue = NormalizeTypeName(kvp.Value);

                if (typeName.Contains(normalizedKey, StringComparison.Ordinal))
                    typeName = typeName.Replace(normalizedKey, normalizedValue, StringComparison.Ordinal);
            }

            return typeName;
        }

        private static string NormalizeTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return typeName;

            var index = typeName.IndexOf(',');
            return index > 0 ? typeName.Substring(0, index) : typeName;
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

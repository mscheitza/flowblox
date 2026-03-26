namespace FlowBlox.Core.Models.Serialization
{
    /// <summary>
    /// Defines one legacy type mapping to a target CLR type.
    /// Legacy type names must be provided as assembly-qualified type names
    /// in the format "Namespace.TypeName, AssemblyName".
    /// </summary>
    public sealed class FlowBloxLegacyTypeMapping
    {
        public Type TargetType { get; }

        public IReadOnlyCollection<string> LegacyTypeNames { get; }

        public FlowBloxLegacyTypeMapping(Type targetType, IEnumerable<string> legacyTypeNames)
        {
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            LegacyTypeNames = legacyTypeNames?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToArray()
                ?? throw new ArgumentNullException(nameof(legacyTypeNames));
        }

        public string TargetAssemblyQualifiedTypeName =>
            $"{TargetType.FullName}, {TargetType.Assembly.GetName().Name}";
    }
}

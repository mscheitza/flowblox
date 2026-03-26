using FlowBlox.Core.Models.Serialization;

namespace FlowBlox.Core.Interfaces
{
    /// <summary>
    /// Provides legacy type mapping entries for JSON serialization binder resolution.
    /// Legacy names must use assembly-qualified type names:
    /// "Namespace.TypeName, AssemblyName".
    /// </summary>
    public interface IFlowBloxLegacyTypeMappingService
    {
        IEnumerable<FlowBloxLegacyTypeMapping> GetLegacyTypeMappings();
    }
}

using Newtonsoft.Json.Serialization;
using FlowBlox.Core.Util.Json.ContractResolver;

namespace FlowBlox.Core.Util.Json.SerializationBinder
{
    internal sealed class AiAssistantTypeAliasSerializationBinder : DefaultSerializationBinder
    {
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            base.BindToName(serializedType, out assemblyName, out typeName);
            assemblyName = AiAssistantJsonPropertySerializationRules.CompactTypeName(assemblyName);
            typeName = AiAssistantJsonPropertySerializationRules.CompactTypeName(typeName);
        }
    }
}
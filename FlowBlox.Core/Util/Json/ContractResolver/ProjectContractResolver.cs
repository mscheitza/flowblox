using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace FlowBlox.Core.Util.Json.ContractResolver
{
    internal class ProjectContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (!property.Writable)
                property.Ignored = true;

            return property;
        }
    }
}

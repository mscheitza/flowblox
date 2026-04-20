using FlowBlox.Core.Models.FlowBlocks.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace FlowBlox.Core.Util.Json.ContractResolver
{
    internal sealed class ProjectImportContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType == typeof(BaseFlowBlock) && 
                string.Equals(property.PropertyName, nameof(BaseFlowBlock.Size), StringComparison.Ordinal))
            {
                property.Ignored = true;
            }

            return property;
        }
    }
}

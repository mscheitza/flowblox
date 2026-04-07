using FlowBlox.Core.Models.Project;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace FlowBlox.Core.Util.Json.ContractResolver
{
    internal sealed class AiAssistantProjectContractResolver : ProjectContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType == typeof(FlowBloxProject) &&
                (string.Equals(property.PropertyName, "InputFiles", StringComparison.Ordinal) ||
                 string.Equals(property.PropertyName, "InputTemplates", StringComparison.Ordinal)))
            {
                property.Ignored = true;
            }

            return property;
        }
    }
}

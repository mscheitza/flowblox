using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using FlowBlox.Core.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FlowBlox.Core.Util.Json.ContractResolver
{
    internal class ProjectContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            if (type == typeof(FlowBloxProject))
            {
                var projectTopLevelOrder = new Dictionary<string, int>(StringComparer.Ordinal)
                {
                    [nameof(FlowBloxProject.UserFields)] = 0,
                    [nameof(FlowBloxProject.ManagedObjects)] = 1,
                    [nameof(FlowBloxProject.FlowBlocks)] = 2
                };

                return properties
                    .Select((property, index) => new { property, index })
                    .OrderBy(x => projectTopLevelOrder.TryGetValue(x.property.PropertyName, out var order) ? order : int.MaxValue)
                    .ThenBy(x => x.index)
                    .Select(x => x.property)
                    .ToList();
            }

            return properties;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (!property.Writable)
                property.Ignored = true;

            return property;
        }
    }
}

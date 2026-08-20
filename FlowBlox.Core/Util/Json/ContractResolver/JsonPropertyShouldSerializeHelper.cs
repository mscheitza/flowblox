using Newtonsoft.Json.Serialization;

namespace FlowBlox.Core.Util.Json.ContractResolver
{
    internal static class JsonPropertyShouldSerializeHelper
    {
        public static void ChainShouldSerialize(JsonProperty property, Predicate<object> predicate)
        {
            var existingShouldSerialize = property.ShouldSerialize;
            property.ShouldSerialize = instance =>
                (existingShouldSerialize?.Invoke(instance) ?? true) &&
                predicate(instance);
        }
    }
}

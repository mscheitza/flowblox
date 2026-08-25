using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.Json;
using FlowBlox.Core.Util.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Reflection;
using FlowBlox.Core.Util.Json.ValueProvider;

namespace FlowBlox.Core.Util.Json.ContractResolver
{
    internal static class AiAssistantJsonPropertySerializationRules
    {
        private static readonly HashSet<string> IgnoredComponentPropertyNames = new(StringComparer.Ordinal)
        {
            nameof(FlowBloxComponent.Version)
        };

        public static string CompactTypeName(string typeName)
            => AiAssistantTypeAliasHelper.CompressTypeName(typeName);

        public static bool IsIgnoredComponentProperty(MemberInfo member, JsonProperty property)
        {
            return member.DeclaringType == typeof(FlowBloxComponent) &&
                IgnoredComponentPropertyNames.Contains(property.PropertyName);
        }

        public static void WriteEmptyEnumerablesAsKeyword(JsonProperty property)
        {
            if (property.Ignored ||
                property.PropertyType == typeof(string) ||
                !typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                return;
            }

            property.ValueProvider = new EmptyEnumerableKeywordValueProvider(property.ValueProvider);
        }

        public static void WriteEnumerableTypeAsCollectionKeyword(JsonProperty property)
        {
            if (property.Ignored ||
                property.PropertyType == typeof(string) ||
                !typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                return;
            }

            property.Converter = new CollectionKeywordJsonConverter();
        }
    }
}
using FlowBlox.Core.Models.Base;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Reflection;

namespace FlowBlox.Core.Util.Json.ContractResolver
{
    internal static class AiAssistantJsonPropertySerializationRules
    {
        public const string EmptyCollectionKeyword = "EMPTY_COLLECTION";

        private static readonly HashSet<string> IgnoredComponentPropertyNames = new(StringComparer.Ordinal)
        {
            nameof(FlowBloxComponent.Version)
        };

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

        private sealed class EmptyEnumerableKeywordValueProvider : IValueProvider
        {
            private readonly IValueProvider _inner;

            public EmptyEnumerableKeywordValueProvider(IValueProvider inner)
            {
                _inner = inner;
            }

            public object GetValue(object target)
            {
                var value = _inner.GetValue(target);
                if (value is not IEnumerable enumerable || value is string)
                    return value;

                var enumerator = enumerable.GetEnumerator();
                try
                {
                    return enumerator.MoveNext()
                        ? value
                        : EmptyCollectionKeyword;
                }
                finally
                {
                    if (enumerator is IDisposable disposable)
                        disposable.Dispose();
                }
            }

            public void SetValue(object target, object value)
            {
                _inner.SetValue(target, value);
            }
        }
    }
}

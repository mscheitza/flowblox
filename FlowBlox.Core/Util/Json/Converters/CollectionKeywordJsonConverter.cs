using FlowBlox.Core.Util.Json.ValueProvider;
using Microsoft.IdentityModel.Tokens.Experimental;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.ObjectModel;

namespace FlowBlox.Core.Util.Json.Converters
{
    internal sealed class CollectionKeywordJsonConverter : JsonConverter
    {
        public const string CollectionTypeKeyword = "LIST";

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) =>
            objectType.IsGenericType &&
            (objectType.GetGenericTypeDefinition() == typeof(List<>) ||
             objectType.GetGenericTypeDefinition() == typeof(ObservableCollection<>));

        public override void WriteJson(
            JsonWriter writer,
            object? value,
            JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            if (value is string s &&
                s == EmptyEnumerableKeywordValueProvider.EmptyCollectionKeyword)
            {
                writer.WriteValue(s);
                return;
            }

            var innerType = value.GetType().GetGenericArguments()[0];

            writer.WriteStartObject();

            writer.WritePropertyName("$type");
            writer.WriteValue($"{CollectionTypeKeyword}[{innerType.FullName}]");

            writer.WritePropertyName("$values");
            writer.WriteStartArray();

            foreach (var item in (IEnumerable)value)
                serializer.Serialize(writer, item);

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer) =>
            throw new NotSupportedException();
    }
}
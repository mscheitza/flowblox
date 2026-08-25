using FlowBlox.Core.Util.Json.ValueProvider;
using Newtonsoft.Json;
using FlowBlox.Core.Util.Json.ContractResolver;
using System.Collections;
using System.Collections.ObjectModel;

namespace FlowBlox.Core.Util.Json.Converters
{
    internal sealed class CollectionKeywordJsonConverter : JsonConverter
    {
        public const string CollectionTypeKeyword = "Collection";

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => TryGetElementType(objectType, out _);

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

            if (!TryGetElementType(value.GetType(), out var innerType))
            {
                serializer.Serialize(writer, value);
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName("$type");
            writer.WriteValue($"{CollectionTypeKeyword}[{AiAssistantJsonPropertySerializationRules.CompactTypeName(innerType.FullName)}]");

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

        internal static bool TryGetElementType(Type collectionType, out Type elementType)
        {
            elementType = null!;

            if (!collectionType.IsGenericType)
                return false;

            var genericTypeDefinition = collectionType.GetGenericTypeDefinition();
            if (genericTypeDefinition != typeof(List<>) &&
                genericTypeDefinition != typeof(ObservableCollection<>))
            {
                return false;
            }

            elementType = collectionType.GetGenericArguments()[0];
            return true;
        }
    }
}
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FlowBlox.Core.Util.Json.Converters
{
    internal sealed class ComponentReferenceJsonConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(FieldElement).IsAssignableFrom(objectType)
                   || typeof(BaseFlowBlock).IsAssignableFrom(objectType)
                   || typeof(IManagedObject).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // Keep the requested snapshot root object fully expanded and only collapse nested references.
            if (string.IsNullOrEmpty(writer.Path))
            {
                WriteRootObject(writer, value, serializer);
                return;
            }

            if (value is FieldElement fieldElement)
            {
                writer.WriteValue(fieldElement.FullyQualifiedName);
                return;
            }

            if (value is BaseFlowBlock flowBlock)
            {
                writer.WriteValue(flowBlock.Name);
                return;
            }

            if (value is IManagedObject managedObject)
            {
                writer.WriteValue(managedObject.Name);
                return;
            }

            serializer.Serialize(writer, value);
        }

        private static void WriteRootObject(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (serializer.ContractResolver.ResolveContract(value.GetType()) is not JsonObjectContract contract)
            {
                serializer.Serialize(writer, value);
                return;
            }

            writer.WriteStartObject();

            foreach (var property in contract.Properties.Where(p => !p.Ignored && p.Readable))
            {
                if (property.ShouldSerialize != null && !property.ShouldSerialize(value))
                    continue;

                var propertyValue = property.ValueProvider.GetValue(value);
                if (propertyValue == null && serializer.NullValueHandling == NullValueHandling.Ignore)
                    continue;

                if (propertyValue != null && serializer.DefaultValueHandling == DefaultValueHandling.Ignore && Equals(propertyValue, property.DefaultValue))
                    continue;

                writer.WritePropertyName(property.PropertyName!);
                serializer.Serialize(writer, propertyValue);
            }

            writer.WriteEndObject();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("Component snapshot converter is write-only.");
        }
    }
}

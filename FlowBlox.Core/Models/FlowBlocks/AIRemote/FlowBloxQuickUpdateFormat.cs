using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Services;
using FlowBlox.Grid.Elements.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote
{
    public sealed class FlowBloxQuickUpdateFormat
    {
        public const string ContractName = "FlowBloxQuickUpdate";

        private readonly BaseFlowBlock _target;
        private readonly IAiResponseInstructionParserService _instructionParserService;

        public FlowBloxQuickUpdateFormat(BaseFlowBlock target, IAiResponseInstructionParserService instructionParserService)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _instructionParserService = instructionParserService ?? throw new ArgumentNullException(nameof(instructionParserService));
        }

        public void Apply(string json)
        {
            Apply(json, null);
        }

        public void Apply(string json, AIProviderBase provider)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("FlowBlox Quick Update JSON is empty.");

            var token = ExtractJsonToken(json, provider);
            if (token is not JObject root)
                throw new InvalidOperationException("FlowBlox Quick Update must be a JSON object.");

            var contract = GetStringProperty(root, "JsonContract") ?? GetStringProperty(root, "contract");
            if (!string.Equals(contract, ContractName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"FlowBlox Quick Update must specify \"JsonContract\": \"{ContractName}\".");
            }

            var propertiesToken = GetProperty(root, "Properties");
            if (propertiesToken is not JObject propertiesObject)
                throw new InvalidOperationException("FlowBlox Quick Update must contain a JSON object named \"Properties\".");

            ApplyProperties(_target, propertiesObject, FlowBloxQuickUpdateSchemaProvider.GetConfigurablePropertyMap(_target.GetType()));
        }

        private JToken ExtractJsonToken(string value, AIProviderBase provider)
        {
            var parseResult = _instructionParserService.Parse(value, provider);
            if (parseResult.JsonObject != null)
                return parseResult.JsonObject;

            throw new InvalidOperationException("FlowBlox Quick Update response is not valid JSON.", parseResult.Exception);
        }

        private static void ApplyProperties(
            object target,
            JObject propertiesObject,
            IReadOnlyDictionary<string, QuickUpdatePropertyDefinition> propertyMap)
        {
            foreach (var jsonProperty in propertiesObject.Properties())
            {
                if (!propertyMap.TryGetValue(jsonProperty.Name, out var definition))
                    throw new InvalidOperationException($"Property \"{jsonProperty.Name}\" is not supported by FlowBlox Quick Update for \"{target.GetType().Name}\".");

                var value = ConvertValue(definition, jsonProperty.Value);
                definition.Property.SetValue(target, value);
                FlowBloxComponentHelper.RaisePropertyChanged(target, definition.Property.Name);
            }
        }

        private static object? ConvertValue(QuickUpdatePropertyDefinition definition, JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            var propertyType = definition.Property.PropertyType;
            return definition.Kind switch
            {
                QuickUpdatePropertyKind.Simple => ConvertSimpleValue(propertyType, token),
                QuickUpdatePropertyKind.Enum => ConvertJsonValue(propertyType, token),
                QuickUpdatePropertyKind.ReactiveObject => ConvertReactiveObject(propertyType, token),
                QuickUpdatePropertyKind.ReactiveObjectCollection => ConvertReactiveObjectCollection(propertyType, definition.ElementType!, token),
                _ => throw new NotSupportedException($"Unsupported FlowBlox Quick Update property kind \"{definition.Kind}\".")
            };
        }

        private static object? ConvertSimpleValue(Type targetType, JToken token)
        {
            if (targetType == typeof(string))
            {
                return token.Type == JTokenType.String
                    ? token.Value<string>()
                    : token.ToString(Formatting.None);
            }

            return ConvertJsonValue(targetType, token);
        }

        private static object? ConvertJsonValue(Type targetType, JToken token)
        {
            var serialized = token.ToString(Formatting.None);
            return JsonConvert.DeserializeObject(serialized, targetType);
        }

        private static object ConvertReactiveObject(Type targetType, JToken token)
        {
            if (token is not JObject itemObject)
                throw new InvalidOperationException($"Value for \"{targetType.Name}\" must be a JSON object.");

            var instance = Activator.CreateInstance(targetType)
                           ?? throw new InvalidOperationException($"Could not create instance of \"{targetType.Name}\".");

            ApplyProperties(instance, itemObject, FlowBloxQuickUpdateSchemaProvider.GetReactiveObjectPropertyMap(targetType));
            return instance;
        }

        private static object ConvertReactiveObjectCollection(Type collectionType, Type elementType, JToken token)
        {
            if (token is not JArray array)
                throw new InvalidOperationException($"Value for collection \"{collectionType.Name}\" must be a JSON array.");

            var collection = CreateCollection(collectionType, elementType);
            var itemPropertyMap = FlowBloxQuickUpdateSchemaProvider.GetReactiveObjectPropertyMap(elementType);

            foreach (var itemToken in array)
            {
                if (itemToken is not JObject itemObject)
                    throw new InvalidOperationException($"Items for collection \"{collectionType.Name}\" must be JSON objects.");

                var item = Activator.CreateInstance(elementType)
                           ?? throw new InvalidOperationException($"Could not create instance of \"{elementType.Name}\".");
                ApplyProperties(item, itemObject, itemPropertyMap);
                collection.Add(item);
            }

            return collection;
        }

        private static IList CreateCollection(Type collectionType, Type elementType)
        {
            if (!collectionType.IsInterface && !collectionType.IsAbstract && Activator.CreateInstance(collectionType) is IList concrete)
                return concrete;

            var observableCollectionType = typeof(ObservableCollection<>).MakeGenericType(elementType);
            if (collectionType.IsAssignableFrom(observableCollectionType) &&
                Activator.CreateInstance(observableCollectionType) is IList observableCollection)
            {
                return observableCollection;
            }

            var listType = typeof(List<>).MakeGenericType(elementType);
            if (collectionType.IsAssignableFrom(listType) &&
                Activator.CreateInstance(listType) is IList list)
            {
                return list;
            }

            throw new InvalidOperationException($"Could not create assignable collection for \"{collectionType.Name}\".");
        }

        private static JToken? GetProperty(JObject obj, string propertyName)
        {
            return obj.Properties()
                .FirstOrDefault(x => string.Equals(x.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }

        private static string? GetStringProperty(JObject obj, string propertyName)
            => GetProperty(obj, propertyName)?.Value<string>();
    }
}

using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Util.Json.ContractResolver;
using FlowBlox.Core.Util.Json.SerializationBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Loader;

namespace FlowBlox.Core.Util.Json
{
    public class JsonSettings
    {
        private static JsonSerializerSettings _default;

        public static JsonSerializerSettings Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.All
                    };
                }

                return _default;
            }
        }

        public static JsonSerializerSettings ProjectImport(Dictionary<string, AssemblyLoadContext> loadContexts)
        {
            var legacyTypeMappings = FlowBloxServiceLocator.Instance
                .GetServices<IFlowBloxLegacyTypeMappingService>()
                .SelectMany(x => x.GetLegacyTypeMappings())
                .ToArray();

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                SerializationBinder = new FlowBloxSerializationBinder(loadContexts, legacyTypeMappings)
            };
            jsonSettings.Converters.Add(new StringEnumConverter
            {
                AllowIntegerValues = true
            });
            jsonSettings.Error = (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"JSON error at path '{args.ErrorContext.Path}': {args.ErrorContext.Error}");
            };
            return jsonSettings;
        }

        public static JsonSerializerSettings ProjectExport()
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.Formatting = Formatting.Indented;
            jsonSettings.TypeNameHandling = TypeNameHandling.All;
            jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            jsonSettings.ContractResolver = new ProjectContractResolver();
            jsonSettings.Converters.Add(new StringEnumConverter
            {
                AllowIntegerValues = true
            });
            return jsonSettings;
        }

        public static JsonSerializerSettings ProjectExportForAiAssistant()
        {
            JsonSerializerSettings jsonSettings = ProjectExport();
            jsonSettings.Formatting = Formatting.None;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
            jsonSettings.ContractResolver = new AiAssistantProjectContractResolver();
            return jsonSettings;
        }

        private JsonSettings()
        {

        }
    }
}

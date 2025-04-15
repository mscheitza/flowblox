using Newtonsoft.Json;
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
            var projectImport = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                SerializationBinder = new FlowBloxSerializationBinder(loadContexts)
            };
            return projectImport;
        }

        private JsonSettings() 
        {
            
        }
    }
}

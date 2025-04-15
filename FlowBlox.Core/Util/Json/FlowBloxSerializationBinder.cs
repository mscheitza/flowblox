using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Newtonsoft.Json.Serialization;

namespace FlowBlox.Core.Util.Json
{
    public class FlowBloxSerializationBinder : DefaultSerializationBinder
    {
        private readonly Dictionary<string, AssemblyLoadContext> _loadContexts;

        public FlowBloxSerializationBinder(Dictionary<string, AssemblyLoadContext> loadContexts)
        {
            _loadContexts = loadContexts;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            foreach (var context in _loadContexts.Values)
            {
                var assembly = context.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName);
                if (assembly != null)
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return base.BindToType(assemblyName, typeName);
        }
    }

}
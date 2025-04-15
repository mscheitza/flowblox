using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Util.Json
{
    public static class JsonHelper
    {
        public static T DeserializeJsonFromFile<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("Die angegebene Datei wurde nicht gefunden.", filePath);

            string fileContent = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<T>(fileContent, JsonSettings.Default);
        }

        public static void SerializeToFile<T>(string filePath, T content, JsonSerializerSettings serializerSettings = null)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (serializerSettings == null)
                serializerSettings = JsonSettings.Default;

            var serializedContent = JsonConvert.SerializeObject(content, serializerSettings);
            File.WriteAllText(filePath, serializedContent);
        }
    }
}

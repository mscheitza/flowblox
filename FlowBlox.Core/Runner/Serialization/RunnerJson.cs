using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowBlox.Core.Runner.Serialization
{
    public static class RunnerJson
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static T ReadFile<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, Options);
        }

        public static void WriteFile<T>(string path, T obj)
        {
            var json = JsonSerializer.Serialize(obj, Options);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path, json);
        }

        public static string Serialize<T>(T obj, bool pretty = true)
        {
            var opts = new JsonSerializerOptions(Options) { WriteIndented = pretty };
            return JsonSerializer.Serialize(obj, opts);
        }
    }
}

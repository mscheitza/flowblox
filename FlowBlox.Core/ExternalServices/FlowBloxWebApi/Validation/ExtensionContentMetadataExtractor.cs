using System.Text.Json;
using System.IO.Compression;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Validation
{
    public class ExtensionMetadataDependency
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class ExtensionMetadata
    {
        public string ExtensionVersion { get; set; }
        public string RuntimeVersion { get; set; }
        public List<ExtensionMetadataDependency> Dependencies { get; set; } = new List<ExtensionMetadataDependency>();
    }

    public static class ExtensionContentMetadataExtractor
    {
        public static ExtensionMetadata GetMetadataFromDepsJson(byte[] content, string extensionName)
        {
            using var memoryStream = new MemoryStream(content);
            using var archive = new System.IO.Compression.ZipArchive(
                memoryStream,
                System.IO.Compression.ZipArchiveMode.Read);

            // Collect all .deps.json entries from the archive
            var depsEntries = archive.Entries
                .Where(e => e.FullName.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // No deps.json found at all
            if (depsEntries.Count == 0)
                return null;

            ZipArchiveEntry selectedEntry = null;

            // If multiple deps.json files exist, try to select the one that matches the extension name (<extensionName>.deps.json)
            if (depsEntries.Count > 1 && !string.IsNullOrWhiteSpace(extensionName))
            {
                selectedEntry = depsEntries.FirstOrDefault(e => 
                    string.Equals(Path.GetFileName(e.FullName), extensionName + ".deps.json", StringComparison.OrdinalIgnoreCase));
            }

            // Fallback: if no specific match was found, use the first entry
            selectedEntry ??= depsEntries.First();

            // Read and parse the selected deps.json file
            using var stream = selectedEntry.Open();
            using var reader = new StreamReader(stream);

            var jsonContent = reader.ReadToEnd();
            return ParseDepsJson(jsonContent);
        }


        public static ExtensionMetadata ParseDepsJson(string jsonContent)
        {
            var metadata = new ExtensionMetadata();
            using (JsonDocument document = JsonDocument.Parse(jsonContent))
            {
                JsonElement root = document.RootElement;

                // Ermitteln des targetName von runtimeTarget
                string targetName = null;
                if (root.TryGetProperty("runtimeTarget", out JsonElement runtimeTargetElement))
                    targetName = runtimeTargetElement.GetProperty("name").GetString();

                if (string.IsNullOrEmpty(targetName))
                    return metadata;

                // Abhängigkeiten und Runtime-Version aus dem richtigen Zielbereich extrahieren
                if (root.TryGetProperty("targets", out JsonElement targetsElement) &&
                    targetsElement.TryGetProperty(targetName, out JsonElement specificTargetElement))
                {
                    // Die erste Assembly ist die Hauptassembly
                    bool isMainAssembly = true;

                    // Durch alle Assemblies im spezifischen Target durchgehen
                    foreach (JsonProperty assembly in specificTargetElement.EnumerateObject())
                    {
                        if (assembly.Value.TryGetProperty("dependencies", out JsonElement dependenciesElement))
                        {
                            string assemblyName = assembly.Name.Split('/')[0];
                            string assemblyVersion = assembly.Name.Split('/')[1];

                            if (isMainAssembly)
                            {
                                metadata.ExtensionVersion = assemblyVersion;

                                foreach (JsonProperty dependency in dependenciesElement.EnumerateObject())
                                {
                                    var dependencyName = dependency.Name;
                                    var dependencyVersion = dependency.Value.GetString();

                                    if (dependencyName.Equals("FlowBlox.Core", StringComparison.OrdinalIgnoreCase))
                                        metadata.RuntimeVersion = dependencyVersion;
                                }

                                isMainAssembly = false;
                            }
                            else
                            {

                                if (dependenciesElement.EnumerateObject().Any(dep => dep.Name.Equals("FlowBlox.Core", StringComparison.OrdinalIgnoreCase)))
                                {
                                    // Es wurde eine Abhängigkeit zu FlowBlox.Core gefunden, damit handelt es sich hier um eine abhängige Extension:
                                    metadata.Dependencies.Add(new ExtensionMetadataDependency
                                    {
                                        Name = assemblyName,
                                        Version = assemblyVersion
                                    });
                                }
                            }
                        }   
                    }
                }
            }

            return metadata;
        }
    }
}
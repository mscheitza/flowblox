using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Provider.Toolbox
{
    public static class FlowBloxToolboxResourceProvider
    {
        public static string? GetToolboxElementContent(string toolboxCategory, string name)
        {
            if (string.IsNullOrWhiteSpace(toolboxCategory) || string.IsNullOrWhiteSpace(name))
                return null;

            foreach (var service in FlowBloxServiceLocator.Instance.GetServices<IFlowBlockToolboxRegistrationService>())
            {
                var assembly = service.GetType().Assembly;
                foreach (var resourceName in service.GetAllToolboxResourcesInModule())
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null)
                        continue;

                    using var reader = new StreamReader(stream);
                    var content = reader.ReadToEnd();
                    var root = JObject.Parse(content);
                    var elements = root["ToolboxElements"] as JArray;
                    if (elements == null)
                        continue;

                    var match = elements
                        .OfType<JObject>()
                        .FirstOrDefault(x =>
                            string.Equals(x.Value<string>("ToolboxCategory"), toolboxCategory, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.Value<string>("Name"), name, StringComparison.OrdinalIgnoreCase));

                    var toolboxContent = match?.Value<string>("Content");
                    if (!string.IsNullOrWhiteSpace(toolboxContent))
                        return toolboxContent;
                }
            }

            return null;
        }
    }
}

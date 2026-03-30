using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Project;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetInputFileTemplatesHandler : ToolHandlerBase
    {
        public override string Name => "GetInputFileTemplates";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns project input file templates (key is relative to $Project::InputDirectory). Returns metadata only.",
            new JObject());

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var project = ToolHandlerUtilities.GetProject();
            var templates = project.InputTemplates ?? new List<FlowBloxInputFileTemplate>();

            var items = new JArray(
                templates
                    .Where(x => x != null)
                    .OrderBy(x => NormalizeKey(x.RelativePath), StringComparer.OrdinalIgnoreCase)
                    .Select(ToTemplateObject));

            var result = new JObject
            {
                ["projectInputDirectory"] = project.ProjectInputDirectory ?? string.Empty,
                ["placeholderHint"] = "$Project::InputDirectory",
                ["count"] = items.Count,
                ["templates"] = items
            };

            return Task.FromResult(ToolHandlerUtilities.Ok(result));
        }

        private static JObject ToTemplateObject(FlowBloxInputFileTemplate template)
        {
            var normalizedKey = NormalizeKey(template.RelativePath);
            var bytes = template.ContentBytes ?? Array.Empty<byte>();

            return new JObject
            {
                ["key"] = normalizedKey,
                ["relativePath"] = normalizedKey,
                ["syncMode"] = template.SyncMode.ToString(),
                ["sizeBytes"] = bytes.LongLength,
                ["hasContent"] = bytes.LongLength > 0
            };
        }

        private static string NormalizeKey(string key)
        {
            return FlowBloxInputTemplateHelper.NormalizeRelativePath(key ?? string.Empty);
        }
    }
}

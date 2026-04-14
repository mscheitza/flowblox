using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Project;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetInputFilesHandler : ToolHandlerBase
    {
        public override string Name => "GetInputFiles";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns project managed input files (key is relative to $Project::InputDirectory). Returns metadata only.",
            new JObject());

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var project = ToolHandlerUtilities.GetProject();
            var templates = project.InputFiles ?? new List<FlowBloxInputFile>();

            var items = new JArray(
                templates
                    .Where(x => x != null)
                    .OrderBy(x => NormalizeKey(x.RelativePath), StringComparer.OrdinalIgnoreCase)
                    .Select(ToTemplateObject));

            var result = new JObject
            {
                ["projectInputDirectory"] = project.ProjectInputDirectory ?? string.Empty,
                ["placeholderHint"] = "$Project::InputDirectory",
                ["commandPlaceholderHint"] = "$InputFile:Path",
                ["count"] = items.Count,
                ["templates"] = items
            };

            return Task.FromResult(ToolHandlerUtilities.Ok(result));
        }

        private static JObject ToTemplateObject(FlowBloxInputFile template)
        {
            var normalizedKey = NormalizeKey(template.RelativePath);
            var bytes = template.ContentBytes ?? Array.Empty<byte>();

            return new JObject
            {
                ["key"] = normalizedKey,
                ["relativePath"] = normalizedKey,
                ["syncMode"] = template.SyncMode.ToString(),
                ["command"] = template.Command ?? string.Empty,
                ["executeBeforeRuntime"] = template.ExecuteBeforeRuntime,
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




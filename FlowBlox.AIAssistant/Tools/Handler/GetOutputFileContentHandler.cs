using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Project;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler
{
    internal sealed class GetOutputFileContentHandler : ToolHandlerBase
    {
        public override string Name => "GetOutputFileContent";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns output file content for one key (relative to $Project::OutputDirectory). Response fields: mimeType, key, textContent, converter, message.",
            new JObject
            {
                ["key"] = "string (relative path under $Project::OutputDirectory)",
                ["usageHint"] = "Returns textContent when possible. XLSX is auto-converted to CSV via Xlsx2CsvConverter."
            });

        public override async Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var key = (args.Value<string>("key") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(key))
                    return ToolHandlerUtilities.Fail("key is required.");

                var project = ToolHandlerUtilities.GetProject();
                var normalizedKey = FlowBloxInputFileHelper.NormalizeRelativePath(key);
                var filePath = ContainedFileResolver.ResolveExistingFile(project.ProjectOutputDirectory, key);
                var bytes = await File.ReadAllBytesAsync(filePath, ct).ConfigureAwait(false);
                return ToolHandlerUtilities.Ok(
                    FileContentResponseBuilder.Build(bytes, normalizedKey, "Output file", filePath));
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Error(
                    "Failed to read AI Assistant output file content.",
                    ex);
                return ToolHandlerUtilities.Fail(ex.Message);
            }
        }
    }
}

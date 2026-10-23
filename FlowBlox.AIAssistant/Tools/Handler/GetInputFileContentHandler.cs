using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Project;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler
{
    internal sealed class GetInputFileContentHandler : ToolHandlerBase
    {
        public override string Name => "GetInputFileContent";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns managed input file content for a single key (relative to $Project::InputDirectory). Response fields: mimeType, key, textContent, converter, message.",
            new JObject
            {
                ["key"] = "string (relative path under $Project::InputDirectory)",
                ["usageHint"] = "Returns textContent when possible. XLSX is auto-converted to CSV via Xlsx2CsvConverter."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var project = ToolHandlerUtilities.GetProject();
                var inputFiles = project.InputFiles ?? new List<FlowBloxInputFile>();

                var key = (args.Value<string>("key") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(key))
                    return Task.FromResult(ToolHandlerUtilities.Fail("key is required."));

                FlowBloxInputFileHelper.ValidateRelativePathOrThrow(key);
                var normalizedKey = FlowBloxInputFileHelper.NormalizeRelativePath(key);

                var inputFile = inputFiles.FirstOrDefault(x =>
                    string.Equals(
                        FlowBloxInputFileHelper.NormalizeRelativePath(x?.RelativePath ?? string.Empty),
                        normalizedKey,
                        StringComparison.OrdinalIgnoreCase));

                if (inputFile == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail($"Input file '{normalizedKey}' was not found."));

                var result = FileContentResponseBuilder.Build(
                    inputFile.ContentBytes ?? Array.Empty<byte>(),
                    normalizedKey,
                    "Input file",
                    inputFile.RelativePath);
                return Task.FromResult(ToolHandlerUtilities.Ok(result));
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Error(
                    "Failed to read AI Assistant input file content.",
                    ex);
                return Task.FromResult(ToolHandlerUtilities.Fail(ex.Message));
            }
        }

    }
}

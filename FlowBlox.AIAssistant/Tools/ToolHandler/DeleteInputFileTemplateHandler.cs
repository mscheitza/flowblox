using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Project;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class DeleteInputFileTemplateHandler : ToolHandlerBase
    {
        public override string Name => "DeleteInputFileTemplate";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Deletes a managed input file entry by key (relative to $Project::InputDirectory).",
            new JObject
            {
                ["key"] = "string (relative path under $Project::InputDirectory)"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var project = ToolHandlerUtilities.GetProject();
                project.InputFiles ??= new List<FlowBloxInputFileTemplate>();

                var key = (args.Value<string>("key") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(key))
                    return Task.FromResult(ToolHandlerUtilities.Fail("key is required."));

                FlowBloxInputTemplateHelper.ValidateRelativePathOrThrow(key);
                var normalizedKey = FlowBloxInputTemplateHelper.NormalizeRelativePath(key);

                var existing = project.InputFiles.FirstOrDefault(x =>
                    string.Equals(
                        FlowBloxInputTemplateHelper.NormalizeRelativePath(x?.RelativePath ?? string.Empty),
                        normalizedKey,
                        StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail($"Input file '{normalizedKey}' was not found."));

                project.InputFiles.Remove(existing);

                // Keep behavior aligned with project-load synchronization path.
                FlowBloxInputTemplateHelper.EnsureInputFilesExist(project);

                return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                {
                    ["deleted"] = true,
                    ["key"] = normalizedKey,
                    ["projectInputDirectory"] = project.ProjectInputDirectory ?? string.Empty,
                    ["note"] = "Existing file on disk is not removed automatically; only the managed input file entry is deleted."
                }));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(ex.Message));
            }
        }
    }
}



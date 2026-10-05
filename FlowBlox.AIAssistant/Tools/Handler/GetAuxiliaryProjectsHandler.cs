using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler
{
    internal sealed class GetAuxiliaryProjectsHandler : ToolHandlerBase
    {
        public override string Name => "GetAuxiliaryProjects";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Lists auxiliary projects in the current session with description, inferred inputs/outputs, path and open state.",
            new JObject());

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var sessionId = ToolHandlerUtilities.CurrentSessionGuid;
            var projects = AuxiliaryProjectSessionStore
                .GetProjects(sessionId)
                .Select(x => ToJson(AuxiliaryProjectMetadataReader.Read(x.ProjectFile), isOpen: false))
                .ToList();

            if (AuxiliaryProjectSessionStore.TryGetActiveProjectInfo(
                    sessionId,
                    out var activeProject,
                    out var activeProjectFile))
            {
                projects.RemoveAll(x => string.Equals(
                    x.Value<string>("projectFile"),
                    activeProjectFile,
                    StringComparison.OrdinalIgnoreCase));
                projects.Add(ToJson(
                    AuxiliaryProjectMetadataReader.Read(activeProject, activeProjectFile),
                    isOpen: true));
            }

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["projects"] = new JArray(projects.OrderBy(
                    x => x.Value<string>("projectName"),
                    StringComparer.OrdinalIgnoreCase))
            }));
        }

        private static JObject ToJson(AuxiliaryProjectMetadata metadata, bool isOpen) =>
            new()
            {
                ["projectName"] = metadata.ProjectName,
                ["description"] = metadata.Description,
                ["inputFields"] = new JArray(metadata.InputFields),
                ["outputFields"] = new JArray(metadata.OutputFields),
                ["projectFile"] = metadata.ProjectFile,
                ["isOpen"] = isOpen
            };
    }
}

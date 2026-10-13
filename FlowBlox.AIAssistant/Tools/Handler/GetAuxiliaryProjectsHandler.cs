using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools.Resolver;
using FlowBlox.Core.Util.DistributedData;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler
{
    internal sealed class GetAuxiliaryProjectsHandler : ToolHandlerBase
    {
        public override string Name => "GetAuxiliaryProjects";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Lists auxiliary projects from the current session or distributed FlowBlox data, optionally filtered by project name or description.",
            new JObject
            {
                ["mode"] = "string? (session|distributedData; default: session)",
                ["filter"] = "string? (comma/space-separated terms; case-insensitive contains OR match against project name or description)"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var mode = (args.Value<string>("mode") ?? "session").Trim();
            var filter = (args.Value<string>("filter") ?? string.Empty).Trim();
            var filterTerms = ToolHandlerUtilities.ParseContainsSearchTerms(args["filter"]);
            if (string.Equals(mode, "distributedData", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(ListDistributedProjects(filter, filterTerms));
            if (!string.Equals(mode, "session", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    $"Auxiliary project mode '{mode}' is not supported. Supported modes: session, distributedData."));
            }

            var sessionId = ToolHandlerUtilities.CurrentSessionGuid;
            var projects = AuxiliaryProjectSessionStore
                .GetProjects(sessionId)
                .Select(x => ToSessionJson(AuxiliaryProjectMetadataReader.Read(x.ProjectFile), isOpen: false))
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
                projects.Add(ToSessionJson(
                    AuxiliaryProjectMetadataReader.Read(activeProject, activeProjectFile),
                    isOpen: true));
            }

            projects = FilterProjects(projects, filterTerms).ToList();
            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["mode"] = "session",
                ["filter"] = filter,
                ["filterTerms"] = new JArray(filterTerms),
                ["matchMode"] = "CaseInsensitiveContainsOR",
                ["projects"] = new JArray(projects.OrderBy(
                    x => x.Value<string>("projectName"),
                    StringComparer.OrdinalIgnoreCase))
            }));
        }

        private static ToolResponse ListDistributedProjects(
            string filter,
            IReadOnlyCollection<string> filterTerms)
        {
            var projects = DistributedDataCatalog.GetDataTypes()
                .Single(x => string.Equals(
                    x.Name,
                    DistributedDataCatalog.AuxiliaryProjectType,
                    StringComparison.OrdinalIgnoreCase))
                .FileNames
                .Select(fileName => new
                {
                    FileName = fileName,
                    ProjectFile = DistributedDataCatalog.ResolvePath(
                        DistributedDataCatalog.AuxiliaryProjectType,
                        fileName)
                })
                .Where(x => x.ProjectFile != null)
                .Select(x => ToDistributedDataJson(
                    AuxiliaryProjectMetadataReader.Read(x.ProjectFile!),
                    x.FileName));

            return ToolHandlerUtilities.Ok(new JObject
            {
                ["mode"] = "distributedData",
                ["filter"] = filter,
                ["filterTerms"] = new JArray(filterTerms),
                ["matchMode"] = "CaseInsensitiveContainsOR",
                ["projects"] = new JArray(FilterProjects(projects, filterTerms).OrderBy(
                    x => x.Value<string>("projectName"),
                    StringComparer.OrdinalIgnoreCase))
            });
        }

        private static IEnumerable<JObject> FilterProjects(
            IEnumerable<JObject> projects,
            IReadOnlyCollection<string> filterTerms)
        {
            return projects.Where(x => ToolHandlerUtilities.ContainsAnySearchTerm(
                [x.Value<string>("projectName"), x.Value<string>("description")],
                filterTerms));
        }

        private static JObject ToSessionJson(AuxiliaryProjectMetadata metadata, bool isOpen)
        {
            var result = ToJson(metadata);
            result["projectFile"] = metadata.ProjectFile;
            result["isOpen"] = isOpen;
            return result;
        }

        private static JObject ToDistributedDataJson(AuxiliaryProjectMetadata metadata, string fileName)
        {
            var result = ToJson(metadata);
            result["fileName"] = fileName;
            return result;
        }

        private static JObject ToJson(AuxiliaryProjectMetadata metadata) =>
            new()
            {
                ["projectName"] = metadata.ProjectName,
                ["description"] = metadata.Description,
                ["inputFields"] = new JArray(metadata.InputFields),
                ["outputFields"] = new JArray(metadata.OutputFields)
            };
    }
}

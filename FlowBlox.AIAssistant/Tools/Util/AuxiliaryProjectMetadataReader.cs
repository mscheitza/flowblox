using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static class AuxiliaryProjectMetadataReader
    {
        public static AuxiliaryProjectMetadata Read(string projectFile)
        {
            return Read(AuxiliaryProjectSerializer.FromFile(projectFile), projectFile);
        }

        public static AuxiliaryProjectMetadata Read(FlowBloxProject project, string projectFile) =>
            new(
                project.ProjectName?.Trim() ?? Path.GetFileNameWithoutExtension(projectFile),
                project.ProjectDescription?.Trim() ?? string.Empty,
                project.UserFields
                    .Where(x => x.UserFieldType == UserFieldTypes.Input)
                    .Select(x => x.Name)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                project.FlowBlocks
                    .OfType<ProjectOutputFlowBlock>()
                    .SelectMany(x => x.MappingEntries)
                    .Select(x => x.OutputPropertyName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Path.GetFullPath(projectFile));
    }

    internal sealed record AuxiliaryProjectMetadata(
        string ProjectName,
        string Description,
        IReadOnlyList<string> InputFields,
        IReadOnlyList<string> OutputFields,
        string ProjectFile);
}

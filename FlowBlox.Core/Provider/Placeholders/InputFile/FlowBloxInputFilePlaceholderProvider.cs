using FlowBlox.Core.Models.Project;

namespace FlowBlox.Core.Provider.Placeholders.InputFile
{
    public static class FlowBloxInputFilePlaceholderProvider
    {
        public static IReadOnlyList<FlowBloxInputFilePlaceholderElement> GetElements(
            string projectInputDirectory,
            FlowBloxInputFile inputFile = null)
        {
            var relativePath = FlowBloxInputTemplateHelper.NormalizeRelativePath(inputFile?.RelativePath ?? string.Empty);
            var absolutePath = string.Empty;

            if (!string.IsNullOrWhiteSpace(relativePath) && !string.IsNullOrWhiteSpace(projectInputDirectory))
            {
                try
                {
                    absolutePath = FlowBloxInputTemplateHelper.BuildAbsoluteTargetPath(projectInputDirectory, relativePath);
                }
                catch
                {
                    absolutePath = string.Empty;
                }
            }

            return new List<FlowBloxInputFilePlaceholderElement>
            {
                new FlowBloxInputFilePlaceholderElement
                {
                    Key = "Path",
                    DisplayName = "Input file path",
                    Description = "Absolute path to the current managed input file.",
                    Value = absolutePath
                },
                new FlowBloxInputFilePlaceholderElement
                {
                    Key = "RelativePath",
                    DisplayName = "Input file relative path",
                    Description = "Relative path of the current managed input file inside the project input directory.",
                    Value = relativePath
                }
            };
        }
    }
}


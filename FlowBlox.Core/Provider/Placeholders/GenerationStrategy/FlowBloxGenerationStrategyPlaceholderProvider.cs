using FlowBlox.Core.Models.Project;

namespace FlowBlox.Core.Provider.Placeholders.GenerationStrategy
{
    public static class FlowBloxGenerationStrategyPlaceholderProvider
    {
        public static IReadOnlyList<FlowBloxGenerationStrategyPlaceholderElement> GetElements()
        {
            return new List<FlowBloxGenerationStrategyPlaceholderElement>
            {
                new FlowBloxGenerationStrategyPlaceholderElement
                {
                    Key = "TargetFlowBlockInputValues",
                    DisplayName = "Target flow block input values",
                    Description = "Fully qualified names and current per-test values of all fields referenced by the target flow block. Password values are hidden; known field names can be used directly as placeholders in configuration values."
                },
                new FlowBloxGenerationStrategyPlaceholderElement
                {
                    Key = "TestExpectations",
                    DisplayName = "Test expectations",
                    Description = "Resolved expectations text derived from linked test definitions."
                },
                new FlowBloxGenerationStrategyPlaceholderElement
                {
                    Key = "TestResults",
                    DisplayName = "Test results",
                    Description = "Resolved test result summary across linked test definitions."
                },
                new FlowBloxGenerationStrategyPlaceholderElement
                {
                    Key = "FlowBloxQuickUpdateSchema",
                    DisplayName = "FlowBlox Quick Update schema",
                    Description = "Minimal schema of configurable target flow block properties for the FlowBlox Quick Update JSON format."
                },
                new FlowBloxGenerationStrategyPlaceholderElement
                {
                    Key = "FlowBloxQuickUpdateFormat",
                    DisplayName = "FlowBlox Quick Update format",
                    Description = "Output format for updating one or more configurable target flow block properties."
                },
                new FlowBloxGenerationStrategyPlaceholderElement
                {
                    Key = "FlowBlockDescriptions",
                    DisplayName = "Flow block descriptions",
                    Description = "Description of the target flow block plus special explanations (listed as bullet points). Useful for AI prompt context."
                }
            };
        }
    }
}

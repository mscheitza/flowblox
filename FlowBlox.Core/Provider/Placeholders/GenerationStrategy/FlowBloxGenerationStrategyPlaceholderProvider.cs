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
                    Key = "InputFieldValue",
                    DisplayName = "Input field value",
                    Description = "Resolved input value of the generation strategy input field."
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
                    Description = "Minimal schema of configurable source flow block properties for the FlowBlox Quick Update JSON format."
                },
                new FlowBloxGenerationStrategyPlaceholderElement
                {
                    Key = "FlowBloxQuickUpdateFormat",
                    DisplayName = "FlowBlox Quick Update format",
                    Description = "Output format for updating one or more configurable source flow block properties."
                },
                new FlowBloxGenerationStrategyPlaceholderElement
                {
                    Key = "FlowBlockDescriptions",
                    DisplayName = "Flow block descriptions",
                    Description = "Description of the source flow block plus special explanations (listed as bullet points). Useful for AI prompt context."
                }
            };
        }
    }
}

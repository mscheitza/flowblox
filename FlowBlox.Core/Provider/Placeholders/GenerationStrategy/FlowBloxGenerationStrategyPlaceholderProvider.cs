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
                    Key = "TargetPropertyDescription",
                    DisplayName = "Target property description",
                    Description = "Data structure description of the target property (collection/class, properties, property types, supported types). Important for structured properties or lists."
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

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
                }
            };
        }
    }
}

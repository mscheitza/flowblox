using FlowBlox.Core.Models.Project;

namespace FlowBlox.UICore.ViewModels.FieldSelection
{
    public sealed class GenerationStrategyDataRowViewModel
    {
        public FlowBloxGenerationStrategyPlaceholderElement GenerationStrategyDataElement { get; }

        public string Key => GenerationStrategyDataElement?.Key ?? string.Empty;
        public string Name => GenerationStrategyDataElement?.DisplayName ?? string.Empty;
        public string Description => GenerationStrategyDataElement?.Description ?? string.Empty;

        public GenerationStrategyDataRowViewModel(FlowBloxGenerationStrategyPlaceholderElement element)
        {
            GenerationStrategyDataElement = element;
        }
    }
}

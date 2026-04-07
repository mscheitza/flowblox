using FlowBlox.Core.Models.Project;

namespace FlowBlox.UICore.ViewModels.FieldSelection
{
    public sealed class InputFileRowViewModel
    {
        public FlowBloxInputFilePlaceholderElement InputFileElement { get; }

        public string Key => InputFileElement?.Key ?? string.Empty;
        public string Name => InputFileElement?.DisplayName ?? string.Empty;
        public string Description => InputFileElement?.Description ?? string.Empty;
        public string Value => InputFileElement?.Value ?? string.Empty;

        public InputFileRowViewModel(FlowBloxInputFilePlaceholderElement element)
        {
            InputFileElement = element;
        }
    }
}

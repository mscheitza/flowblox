namespace FlowBlox.Core.Models.Project
{
    public sealed class FlowBloxInputFilePlaceholderElement
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Placeholder => $"$InputFile:{Key}";
    }
}

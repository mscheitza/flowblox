namespace FlowBlox.Core.Models.FlowBlocks.StartEndPatternSelector
{
    public interface ISelectionAlgorithm
    {
        List<string> Select(string content, string startPattern, string endPattern, bool enableMultiline);
    }
}

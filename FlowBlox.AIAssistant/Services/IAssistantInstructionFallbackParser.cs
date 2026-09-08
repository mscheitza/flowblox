namespace FlowBlox.AIAssistant.Services
{
    /// <summary>
    /// Parses provider-specific assistant outputs that leaked as plain text after the primary JSON protocol parser failed.
    /// This is a compatibility bridge while the assistant still asks models to emit FlowBlox tool requests as JSON.
    /// In the long run, provider-native tool calling should replace these fallback parsers so tool calls arrive as structured
    /// Semantic Kernel function-call content instead of model-specific text envelopes.
    /// </summary>
    public interface IAssistantInstructionFallbackParser
    {
        bool TryParse(
            string output,
            Exception? primaryParseException,
            out AssistantInstructionParseResult result);
    }
}
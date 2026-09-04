using System.Text;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.AIAssistant.Builder
{
    internal static class AssistantSummaryRequestBuilder
    {
        public static AIChatRequest Build(string currentSummary, IReadOnlyList<AssistantSessionMessage> messagesToSummarize)
        {
            var request = new AIChatRequest
            {
                Source = "FlowBloxAIAssistantSummary"
            };

            request.SystemMessages.Add(new AIChatMessage
            {
                Role = "system",
                Content =
                    "You maintain a compact, durable structured summary for the FlowBlox AI Assistant conversation. " +
                    "Merge only the provided new messages into the current summary and return the full updated summary. " +
                    "Use exactly these Markdown sections: Goals, Decisions, Completed Changes, Open Points, User Preferences, Provider And Configuration Constraints, Important Identifiers, Tool API Working Memory. " +
                    "Use concise bullet points. Write '(none)' for empty sections. Do not include chat messages verbatim unless an exact identifier, property path, type name, resolver value, tool name, or error message is important. " +
                    "In Tool API Working Memory, preserve reusable technical facts learned from tool responses, especially GetTypeKindsInfo, GetManagedObjectKindsInfo, GetComponentSnapshot, GetFlowBlockSnapshot, UpdateFlowBlock, UpdateManagedObject, ConnectFlowBlocks, and failed tool calls. " +
                    "Keep exact FlowBlox type names, property/update paths, collection paths, supported enum values, placeholder names, option names, resolver syntax and concrete resolver values like {\"resolveFieldElementByFQName\":\"$FlowBlock::Field\"}, referenced flow block names, connection direction, selection-filter constraints, and design hints needed for later updates or connections. " +
                    "Prefer compact grouped bullets by FlowBlock/ManagedObject/type. " +
                    "Remove obsolete technical facts when newer tool responses clearly supersede them."
            });

            var sb = new StringBuilder();
            sb.AppendLine("Current structured summary:");
            sb.AppendLine(string.IsNullOrWhiteSpace(currentSummary) ? "(empty)" : currentSummary.Trim());
            sb.AppendLine();
            sb.AppendLine("New messages to merge into the summary:");

            foreach (var message in messagesToSummarize ?? Array.Empty<AssistantSessionMessage>())
            {
                if (string.IsNullOrWhiteSpace(message?.CompleteMessage))
                    continue;

                sb.AppendLine();
                sb.AppendLine(message switch
                {
                    AssistantMessagePair => "MessagePair (AssistantRequest + ToolApiResponse):",
                    AssistantSingleMessage single when string.Equals(single.Role, "assistant", StringComparison.OrdinalIgnoreCase) => "SingleMessage (Assistant):",
                    _ => "SingleMessage (User):"
                });
                sb.AppendLine(message.CompleteMessage.Trim());
            }

            request.Messages.Add(new AIChatMessage
            {
                Role = "user",
                Content = sb.ToString().TrimEnd()
            });

            return request;
        }
    }
}
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
                    "Use exactly these Markdown sections: Goals, Decisions, Completed Changes, Open Points, User Preferences, Provider And Configuration Constraints, Important Identifiers. " +
                    "Use concise bullet points. Write '(none)' for empty sections. Do not include chat messages verbatim unless an exact identifier or name is important."
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

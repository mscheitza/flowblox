using System.Text;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.AIAssistant.Builder
{
    internal static class AssistantSummaryRequestBuilder
    {
        public static AIChatRequest Build(string currentSummary, IReadOnlyList<AssistantConversationMessage> messagesToSummarize)
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

            foreach (var message in messagesToSummarize ?? Array.Empty<AssistantConversationMessage>())
            {
                if (string.IsNullOrWhiteSpace(message?.Content))
                    continue;

                sb.AppendLine();
                var source = string.IsNullOrWhiteSpace(message.Source)
                    ? string.Empty
                    : $" ({message.Source.Trim()})";
                sb.AppendLine(string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                    ? $"Assistant{source}:"
                    : $"User{source}:");
                sb.AppendLine(message.Content.Trim());
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

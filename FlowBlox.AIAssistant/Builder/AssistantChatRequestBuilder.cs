using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.AIAssistant.Builder
{
    internal static class AssistantChatRequestBuilder
    {
        public static AIChatRequest Build(
            string systemPrompt,
            string sessionBootstrapPrompt,
            string conversationSummary,
            IReadOnlyList<AssistantConversationMessage> sessionMessages,
            string currentUserPrompt,
            int maxLatestMessages,
            AssistantTokenBudget tokenBudget)
        {
            var request = new AIChatRequest();
            request.SystemMessages.Add(new AIChatMessage
            {
                Role = "system",
                Content = systemPrompt,
                CacheBehavior = AIChatCacheBehavior.PreferCache
            });
            request.SystemMessages.Add(new AIChatMessage
            {
                Role = "system",
                Content = sessionBootstrapPrompt,
                CacheBehavior = AIChatCacheBehavior.PreferCache
            });

            if (!string.IsNullOrWhiteSpace(conversationSummary))
            {
                request.SystemMessages.Add(new AIChatMessage
                {
                    Role = "system",
                    Content = "Conversation Summary:\n" + conversationSummary.Trim()
                });
            }

            var remainingHistoryTokens = CalculateRemainingHistoryTokens(request.SystemMessages, currentUserPrompt, tokenBudget);
            foreach (var message in SelectLatestMessages(sessionMessages, maxLatestMessages, remainingHistoryTokens, tokenBudget))
            {
                request.Messages.Add(new AIChatMessage
                {
                    Role = string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                        ? "assistant"
                        : "user",
                    Content = message.Content.Trim()
                });
            }

            request.Messages.Add(new AIChatMessage
            {
                Role = "user",
                Content = currentUserPrompt
            });

            return request;
        }

        private static int CalculateRemainingHistoryTokens(
            IReadOnlyList<AIChatMessage> systemMessages,
            string currentUserPrompt,
            AssistantTokenBudget tokenBudget)
        {
            var maxContextTokens = Math.Max(0, tokenBudget.MaxContextTokens);
            if (maxContextTokens == 0)
                return int.MaxValue;

            var reservedResponseTokens = Math.Max(0, tokenBudget.ReservedResponseTokens);
            var fixedTokens = tokenBudget.EstimateTokens(currentUserPrompt);
            foreach (var systemMessage in systemMessages ?? Array.Empty<AIChatMessage>())
                fixedTokens += tokenBudget.EstimateTokens(systemMessage?.Content ?? string.Empty);

            return Math.Max(0, maxContextTokens - reservedResponseTokens - fixedTokens);
        }

        private static IReadOnlyList<AssistantConversationMessage> SelectLatestMessages(
            IReadOnlyList<AssistantConversationMessage> sessionMessages,
            int maxLatestMessages,
            int maxHistoryTokens,
            AssistantTokenBudget tokenBudget)
        {
            var candidates = (sessionMessages ?? Array.Empty<AssistantConversationMessage>())
                .Where(x => !string.IsNullOrWhiteSpace(x?.Content))
                .TakeLast(Math.Clamp(maxLatestMessages, 0, 50))
                .Reverse()
                .ToList();

            var selected = new List<AssistantConversationMessage>();
            var usedTokens = 0;

            foreach (var message in candidates)
            {
                var messageTokens = tokenBudget.EstimateTokens(message.Content);
                if (usedTokens + messageTokens > maxHistoryTokens)
                    break;

                selected.Add(message);
                usedTokens += messageTokens;
            }

            selected.Reverse();
            return selected;
        }
    }
}

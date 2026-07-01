using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.AIAssistant.Builder
{
    internal static class AssistantChatRequestBuilder
    {
        public static AssistantChatRequestBuildResult Build(
            string systemPrompt,
            string sessionBootstrapPrompt,
            string conversationSummary,
            IReadOnlyList<AssistantConversationMessage> sessionMessages,
            string currentUserPrompt,
            int maxLatestMessages,
            int minLatestMessages,
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
            var latestSelection = SelectLatestMessages(
                sessionMessages,
                maxLatestMessages,
                minLatestMessages,
                remainingHistoryTokens,
                tokenBudget);

            foreach (var message in latestSelection.Messages)
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

            return new AssistantChatRequestBuildResult
            {
                Request = request,
                FirstIncludedHistoryMessageIndex = latestSelection.FirstIncludedHistoryMessageIndex,
                IncludedHistoryMessageCount = latestSelection.Messages.Count
            };
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

        private static LatestMessageSelection SelectLatestMessages(
            IReadOnlyList<AssistantConversationMessage> sessionMessages,
            int maxLatestMessages,
            int minLatestMessages,
            int maxHistoryTokens,
            AssistantTokenBudget tokenBudget)
        {
            var messages = sessionMessages ?? Array.Empty<AssistantConversationMessage>();
            var maxMessages = Math.Clamp(maxLatestMessages, 0, 50);
            var minMessages = Math.Clamp(minLatestMessages, 0, maxMessages);

            var indexedCandidates = messages
                .Select((message, index) => new IndexedConversationMessage(message, index))
                .Where(x => !string.IsNullOrWhiteSpace(x.Message?.Content))
                .TakeLast(maxMessages)
                .Reverse()
                .ToList();

            var selected = new List<IndexedConversationMessage>();
            var usedTokens = 0;

            foreach (var candidate in indexedCandidates)
            {
                var messageTokens = tokenBudget.EstimateTokens(candidate.Message.Content);
                if (selected.Count >= minMessages && usedTokens + messageTokens > maxHistoryTokens)
                    break;

                selected.Add(candidate);
                usedTokens += messageTokens;
            }

            selected.Reverse();
            return new LatestMessageSelection
            {
                Messages = selected.Select(x => x.Message).ToList(),
                FirstIncludedHistoryMessageIndex = selected.Count == 0
                    ? messages.Count
                    : selected.Min(x => x.Index)
            };
        }

        private sealed class LatestMessageSelection
        {
            public List<AssistantConversationMessage> Messages { get; init; } = new();
            public int FirstIncludedHistoryMessageIndex { get; init; }
        }

        private sealed class IndexedConversationMessage
        {
            public IndexedConversationMessage(AssistantConversationMessage message, int index)
            {
                Message = message;
                Index = index;
            }

            public AssistantConversationMessage Message { get; }
            public int Index { get; }
        }
    }
}

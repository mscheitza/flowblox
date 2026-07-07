using FlowBlox.AIAssistant.Constants;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.AIAssistant.Builder
{
    internal static class AssistantChatRequestBuilder
    {
        public static AssistantChatRequestBuildResult Build(
            string systemPrompt,
            string sessionBootstrapPrompt,
            string conversationSummary,
            IReadOnlyList<AssistantSessionMessage> sessionMessages,
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
                    Content = message.CompleteMessage.Trim()
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
            ArgumentNullException.ThrowIfNull(tokenBudget);

            var maxContextTokens = Math.Max(AssistantConfigurationLimits.MinContextTokens, tokenBudget.MaxContextTokens);
            if (maxContextTokens == 0)
                return int.MaxValue;

            var reservedResponseTokens = Math.Max(AssistantConfigurationLimits.MinReservedResponseTokens, tokenBudget.ReservedResponseTokens);
            var fixedTokens = tokenBudget.EstimateTokens(currentUserPrompt);
            foreach (var systemMessage in systemMessages ?? Array.Empty<AIChatMessage>())
                fixedTokens += tokenBudget.EstimateTokens(systemMessage?.Content ?? string.Empty);

            return Math.Max(AssistantConfigurationLimits.MinContextTokens, maxContextTokens - reservedResponseTokens - fixedTokens);
        }

        private static LatestMessageSelection SelectLatestMessages(
            IReadOnlyList<AssistantSessionMessage> sessionMessages,
            int maxLatestMessages,
            int minLatestMessages,
            int maxHistoryTokens,
            AssistantTokenBudget tokenBudget)
        {
            ArgumentNullException.ThrowIfNull(tokenBudget);

            var messages = sessionMessages ?? Array.Empty<AssistantSessionMessage>();
            var maxMessages = Math.Clamp(
                maxLatestMessages,
                AssistantConfigurationLimits.MinLatestMessages,
                AssistantConfigurationLimits.MaxLatestMessages);
            var minMessages = Math.Clamp(
                minLatestMessages,
                AssistantConfigurationLimits.MinLatestMessages,
                maxMessages);

            if (maxMessages == 0)
            {
                return new LatestMessageSelection
                {
                    FirstIncludedHistoryMessageIndex = messages.Count
                };
            }

            var indexedCandidates = messages
                .Select((message, index) => new IndexedConversationMessage(message, index))
                .Where(x => !string.IsNullOrWhiteSpace(x.Message?.CompleteMessage))
                .Reverse()
                .ToList();

            var selected = new List<IndexedConversationMessage>();
            var selectedMessageCount = 0;
            var usedTokens = 0;

            foreach (var candidate in indexedCandidates)
            {
                if (selectedMessageCount > 0 && selectedMessageCount + 1 > maxMessages)
                    break;

                var messageTokens = tokenBudget.EstimateTokens(candidate.Message.CompleteMessage);
                if (selectedMessageCount >= minMessages && usedTokens + messageTokens > maxHistoryTokens)
                    break;

                selected.Add(candidate);
                selectedMessageCount++;
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
            public List<AssistantSessionMessage> Messages { get; init; } = new();
            public int FirstIncludedHistoryMessageIndex { get; init; }
        }

        private sealed class IndexedConversationMessage
        {
            public IndexedConversationMessage(AssistantSessionMessage message, int index)
            {
                Message = message;
                Index = index;
            }

            public AssistantSessionMessage Message { get; }
            public int Index { get; }
        }
    }
}

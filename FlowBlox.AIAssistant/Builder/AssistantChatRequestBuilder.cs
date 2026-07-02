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
            IReadOnlyList<AssistantConversationMessage> sessionMessages,
            int maxLatestMessages,
            int minLatestMessages,
            int maxHistoryTokens,
            AssistantTokenBudget tokenBudget)
        {
            ArgumentNullException.ThrowIfNull(tokenBudget);

            var messages = sessionMessages ?? Array.Empty<AssistantConversationMessage>();
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

            var units = BuildContextUnits(messages
                .Select((message, index) => new IndexedConversationMessage(message, index))
                .Where(x => !string.IsNullOrWhiteSpace(x.Message?.Content))
                .ToList());

            var selectedUnits = new List<ContextMessageUnit>();
            var selectedMessageCount = 0;
            var usedTokens = 0;

            foreach (var unit in units.AsEnumerable().Reverse())
            {
                if (selectedMessageCount > 0 && selectedMessageCount + unit.Messages.Count > maxMessages)
                    break;

                var unitTokens = unit.Messages.Sum(x => tokenBudget.EstimateTokens(x.Message.Content));
                if (selectedMessageCount >= minMessages && usedTokens + unitTokens > maxHistoryTokens)
                    break;

                selectedUnits.Add(unit);
                selectedMessageCount += unit.Messages.Count;
                usedTokens += unitTokens;
            }

            selectedUnits.Reverse();
            var selected = selectedUnits
                .SelectMany(x => x.Messages)
                .ToList();

            return new LatestMessageSelection
            {
                Messages = selected.Select(x => x.Message).ToList(),
                FirstIncludedHistoryMessageIndex = selected.Count == 0
                    ? messages.Count
                    : selected.Min(x => x.Index)
            };
        }

        private static List<ContextMessageUnit> BuildContextUnits(List<IndexedConversationMessage> messages)
        {
            var units = new List<ContextMessageUnit>();
            for (var index = 0; index < messages.Count; index++)
            {
                var current = messages[index];
                var pairId = current.Message.PairId?.Trim() ?? string.Empty;
                var unitMessages = new List<IndexedConversationMessage> { current };

                if (!string.IsNullOrWhiteSpace(pairId))
                {
                    while (index + 1 < messages.Count &&
                           string.Equals(messages[index + 1].Message.PairId?.Trim(), pairId, StringComparison.Ordinal))
                    {
                        index++;
                        unitMessages.Add(messages[index]);
                    }
                }
                else if (IsUserMessage(current.Message) &&
                         index + 1 < messages.Count &&
                         string.IsNullOrWhiteSpace(messages[index + 1].Message.PairId) &&
                         IsAssistantMessage(messages[index + 1].Message))
                {
                    index++;
                    unitMessages.Add(messages[index]);
                }

                units.Add(new ContextMessageUnit(unitMessages));
            }

            return units;
        }

        private static bool IsUserMessage(AssistantConversationMessage message)
        {
            return !IsAssistantMessage(message);
        }

        private static bool IsAssistantMessage(AssistantConversationMessage message)
        {
            return string.Equals(message?.Role, "assistant", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class LatestMessageSelection
        {
            public List<AssistantConversationMessage> Messages { get; init; } = new();
            public int FirstIncludedHistoryMessageIndex { get; init; }
        }

        private sealed class ContextMessageUnit
        {
            public ContextMessageUnit(List<IndexedConversationMessage> messages)
            {
                Messages = messages ?? new List<IndexedConversationMessage>();
            }

            public List<IndexedConversationMessage> Messages { get; }
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

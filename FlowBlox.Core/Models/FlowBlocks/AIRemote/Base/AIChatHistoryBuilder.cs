using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    internal static class AIChatHistoryBuilder
    {
        public static ChatHistory Build(AIChatRequest request)
        {
            var history = new ChatHistory();

            foreach (var systemMessage in request?.SystemMessages ?? Enumerable.Empty<AIChatMessage>())
            {
                var content = ToChatMessageContent(systemMessage);
                if (content != null)
                    history.Add(content);
            }

            foreach (var message in request?.Messages ?? Enumerable.Empty<AIChatMessage>())
            {
                var content = ToChatMessageContent(message);
                if (content != null)
                    history.Add(content);
            }

            return history;
        }

        private static ChatMessageContent? ToChatMessageContent(AIChatMessage message)
        {
            if (message?.SemanticContent != null)
                return message.SemanticContent;

            if (string.IsNullOrWhiteSpace(message?.Content))
                return null;

            return new ChatMessageContent(ToAuthorRole(message.Role), message.Content.Trim());
        }

        private static AuthorRole ToAuthorRole(string role)
        {
            if (string.Equals(role, "system", StringComparison.OrdinalIgnoreCase))
                return AuthorRole.System;

            if (string.Equals(role, "assistant", StringComparison.OrdinalIgnoreCase))
                return AuthorRole.Assistant;

            if (string.Equals(role, "tool", StringComparison.OrdinalIgnoreCase))
                return AuthorRole.Tool;

            return AuthorRole.User;
        }
    }
}

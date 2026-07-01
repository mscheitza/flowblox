using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    [Display(Name = "AnthropicAIProvider_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("AnthropicAIProvider_DisplayName_Plural", typeof(FlowBloxTexts))]
    public sealed class AnthropicAIProvider : AIProviderBase
    {
        public override string ProviderType => "Anthropic";

        public AnthropicAIProvider()
        {
            BaseUrl = "https://api.anthropic.com/v1";
            DefaultModel = "claude-opus-4-6";
            TimeoutSeconds = 60;
        }

        protected override async Task<AIResponse> ExecuteChatCoreAsync(AIChatRequest request, CancellationToken ct)
        {
            var resolvedApiKey = FlowBloxFieldHelper.ReplaceFieldsInString(ApiKey);
            if (string.IsNullOrWhiteSpace(resolvedApiKey))
            {
                return new AIResponse
                {
                    Success = false,
                    Error = "Anthropic API key is empty after field resolution."
                };
            }

            var resolvedBaseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl);
            if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
                throw new InvalidOperationException("Anthropic base URL is empty after field resolution.");

            var response = await new AnthropicClient(new APIAuthentication(resolvedApiKey))
                .Messages
                .GetClaudeMessageAsync(BuildMessageParameters(request), ct)
                .ConfigureAwait(false);

            return new AIResponse
            {
                Success = true,
                Text = response?.Message?.ToString() ?? string.Empty
            };
        }

        private static MessageParameters BuildMessageParameters(AIChatRequest request)
        {
            var parameters = new MessageParameters
            {
                Model = request.Model,
                MaxTokens = request.MaxTokens.GetValueOrDefault(1024),
                Stream = false,
                System = BuildSystemMessages(request),
                Messages = BuildMessages(request),
                PromptCaching = PromptCacheType.FineGrained
            };

            if (request.Temperature is >= 0 and <= 1)
                parameters.Temperature = (decimal)request.Temperature.Value;

            return parameters;
        }

        private static List<SystemMessage> BuildSystemMessages(AIChatRequest request)
        {
            var systemMessages = new List<SystemMessage>();

            foreach (var systemMessage in request?.SystemMessages ?? Enumerable.Empty<AIChatMessage>())
            {
                if (string.IsNullOrWhiteSpace(systemMessage?.Content))
                    continue;

                var content = systemMessage.Content.Trim();
                if (systemMessage.CacheBehavior == AIChatCacheBehavior.PreferCache)
                    systemMessages.Add(new SystemMessage(content, new CacheControl { Type = CacheControlType.ephemeral }));
                else
                    systemMessages.Add(new SystemMessage(content));
            }

            return systemMessages;
        }

        private static List<Message> BuildMessages(AIChatRequest request)
        {
            var messages = new List<Message>();

            foreach (var message in request?.Messages ?? Enumerable.Empty<AIChatMessage>())
            {
                if (string.IsNullOrWhiteSpace(message?.Content))
                    continue;

                var role = string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                    ? RoleType.Assistant
                    : RoleType.User;

                messages.Add(new Message(role, message.Content.Trim()));
            }

            return messages;
        }
    }
}

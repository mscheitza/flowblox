using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Util.Fields;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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
            DefaultModel = "claude-opus-5";
            EstimatedSystemPromptCacheSavingsRate = 0.70d;
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

            var resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(request.Model);
            if (string.IsNullOrWhiteSpace(resolvedModel))
                resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(DefaultModel);

            var timeoutSeconds = ResolveTimeoutSeconds(request);
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
            var apiUrlFormat = BuildApiUrlFormat(resolvedBaseUrl);
            using var client = new AnthropicClient(new APIAuthentication(resolvedApiKey), httpClient, requestInterceptor: null)
            {
                ApiUrlFormat = apiUrlFormat
            };

            var messageParameters = BuildMessageParameters(request, resolvedModel);

            MessageResponse response;
            try
            {
                response = await client
                    .Messages
                    .GetClaudeMessageAsync(messageParameters, ct)
                    .ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException(
                    $"Anthropic request failed. Model: '{resolvedModel}', BaseUrl: '{resolvedBaseUrl}', ApiUrlFormat: '{apiUrlFormat}'. Details: {ex.Message}",
                    ex,
                    ex.StatusCode);
            }

            var responseText = response?.Message?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(responseText) &&
                string.Equals(Convert.ToString(response?.StopReason), "max_tokens", StringComparison.OrdinalIgnoreCase))
            {
                return new AIResponse
                {
                    Success = false,
                    Error =
                        $"Anthropic returned no text because the response stopped at the max_tokens limit ({messageParameters.MaxTokens}). " +
                        "Claude may have used the complete output budget for thinking before producing the required JSON response. " +
                        "Increase the assistant MaxTokens setting and retry."
                };
            }

            LogPromptCachingUsage(response);

            return new AIResponse
            {
                Success = true,
                Text = responseText,
                PromptTokens = response?.Usage?.InputTokens,
                CompletionTokens = response?.Usage?.OutputTokens
            };
        }

        private static void LogPromptCachingUsage(MessageResponse? response)
        {
            var usage = response?.Usage;
            if (usage == null)
                return;

            FlowBloxLogManager.Instance.GetLogger().Info(
                "Anthropic usage: " +
                $"input_tokens={usage.InputTokens}, " +
                $"output_tokens={usage.OutputTokens}, " +
                $"cache_creation_input_tokens={usage.CacheCreationInputTokens}, " +
                $"cache_read_input_tokens={usage.CacheReadInputTokens}");
        }

        private static string BuildApiUrlFormat(string resolvedBaseUrl)
        {
            var trimmedBaseUrl = resolvedBaseUrl.Trim().TrimEnd('/');

            if (trimmedBaseUrl.Contains("{0}", StringComparison.Ordinal) &&
                trimmedBaseUrl.Contains("{1}", StringComparison.Ordinal))
                return trimmedBaseUrl;

            if (trimmedBaseUrl.Contains("{0}", StringComparison.Ordinal))
                return trimmedBaseUrl.Replace("{0}", "{0}/{1}", StringComparison.Ordinal);

            var versionSuffix = Regex.Match(trimmedBaseUrl, @"/v\d+$", RegexOptions.IgnoreCase);
            if (versionSuffix.Success)
                return trimmedBaseUrl.Substring(0, versionSuffix.Index) + "/{0}/{1}";

            return trimmedBaseUrl + "/{0}/{1}";
        }

        private static MessageParameters BuildMessageParameters(AIChatRequest request, string resolvedModel)
        {
            var parameters = new MessageParameters
            {
                Model = resolvedModel,
                MaxTokens = request.MaxTokens.GetValueOrDefault(AiProviderDefaults.Anthropic.DefaultMaxTokens),
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
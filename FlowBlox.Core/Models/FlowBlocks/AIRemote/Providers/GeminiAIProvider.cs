using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Util.Fields;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    [Display(Name = "GeminiAIProvider_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("GeminiAIProvider_DisplayName_Plural", typeof(FlowBloxTexts))]
    public sealed class GeminiAIProvider : AIProviderBase
    {
        public override string ProviderType => "Gemini";

        protected override bool SupportsReasoningEffort => true;

        public GeminiAIProvider()
        {
            BaseUrl = "https://generativelanguage.googleapis.com/v1";
            DefaultModel = "gemini-3.7-flash";
            EstimatedSystemPromptCacheSavingsRate = 0.60d;
        }

        protected override async Task<AIResponse> ExecuteChatCoreAsync(AIChatRequest request, CancellationToken ct)
        {
            var resolvedApiKey = FlowBloxFieldHelper.ReplaceFieldsInString(ApiKey);
            if (string.IsNullOrWhiteSpace(resolvedApiKey))
            {
                return new AIResponse
                {
                    Success = false,
                    Error = "Gemini API key is empty after field resolution."
                };
            }

            var resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(request.Model);
            if (string.IsNullOrWhiteSpace(resolvedModel))
                resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(DefaultModel);

            var resolvedBaseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl);
            if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
                throw new InvalidOperationException("Gemini base URL is empty after field resolution.");

            var timeoutSeconds = ResolveTimeoutSeconds(request);
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };

#pragma warning disable SKEXP0070
            var chatService = new GoogleAIGeminiChatCompletionService(
                resolvedModel,
                resolvedApiKey,
                ResolveApiVersion(resolvedBaseUrl),
                httpClient,
                loggerFactory: null);
#pragma warning restore SKEXP0070

            var response = await chatService.GetChatMessageContentAsync(
                AIChatHistoryBuilder.Build(request),
                BuildExecutionSettings(request),
                kernel: null,
                cancellationToken: ct).ConfigureAwait(false);

            return new AIResponse
            {
                Success = true,
                Text = response?.Content ?? string.Empty
            };
        }

        private GeminiPromptExecutionSettings BuildExecutionSettings(AIChatRequest request)
        {
            var settings = new GeminiPromptExecutionSettings();
            if (request.Temperature is >= 0 and <= 1)
                settings.Temperature = request.Temperature;

            if (request.MaxTokens is > 0)
                settings.MaxTokens = request.MaxTokens;

            settings.ThinkingConfig = new GeminiThinkingConfig
            {
                ThinkingLevel = ToReasoningEffortValue(ReasoningEffort)
            };

            return settings;
        }

        private static GoogleAIVersion ResolveApiVersion(string resolvedBaseUrl)
        {
            var trimmedBaseUrl = resolvedBaseUrl.Trim().TrimEnd('/');
            if (trimmedBaseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                return GoogleAIVersion.V1;

            return GoogleAIVersion.V1_Beta;
        }

    }
}

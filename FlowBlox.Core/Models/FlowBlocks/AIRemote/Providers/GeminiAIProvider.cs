using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
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

        public GeminiAIProvider()
        {
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta";
            DefaultModel = "gemini-3-flash";
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
                    Error = "Gemini API key is empty after field resolution."
                };
            }

            var resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(request.Model);
            if (string.IsNullOrWhiteSpace(resolvedModel))
                resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(DefaultModel);

            var resolvedBaseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl);
            if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
                throw new InvalidOperationException("Gemini base URL is empty after field resolution.");

#pragma warning disable SKEXP0070
            var chatService = new GoogleAIGeminiChatCompletionService(
                resolvedModel,
                resolvedApiKey,
                GoogleAIVersion.V1_Beta);
#pragma warning restore SKEXP0070

            var response = await chatService.GetChatMessageContentAsync(
                BuildChatHistory(request),
                BuildExecutionSettings(request),
                kernel: null,
                cancellationToken: ct).ConfigureAwait(false);

            return new AIResponse
            {
                Success = true,
                Text = response?.Content ?? string.Empty
            };
        }

        private static GeminiPromptExecutionSettings BuildExecutionSettings(AIChatRequest request)
        {
            var settings = new GeminiPromptExecutionSettings();
            if (request.Temperature is >= 0 and <= 1)
                settings.Temperature = request.Temperature;

            if (request.MaxTokens is > 0)
                settings.MaxTokens = request.MaxTokens;

            return settings;
        }

        private static ChatHistory BuildChatHistory(AIChatRequest request)
        {
            var history = new ChatHistory();

            foreach (var systemMessage in request?.SystemMessages ?? Enumerable.Empty<AIChatMessage>())
            {
                if (!string.IsNullOrWhiteSpace(systemMessage?.Content))
                    history.AddSystemMessage(systemMessage.Content);
            }

            foreach (var message in request?.Messages ?? Enumerable.Empty<AIChatMessage>())
            {
                if (string.IsNullOrWhiteSpace(message?.Content))
                    continue;

                if (string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                    history.AddAssistantMessage(message.Content);
                else
                    history.AddUserMessage(message.Content);
            }

            return history;
        }
    }
}

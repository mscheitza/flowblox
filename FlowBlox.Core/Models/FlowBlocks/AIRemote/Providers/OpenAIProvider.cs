using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    [Display(Name = "OpenAIProvider_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("OpenAIProvider_DisplayName_Plural", typeof(FlowBloxTexts))]
    public sealed class OpenAIProvider : AIProviderBase
    {
        [Display(Name = "OpenAIProvider_OrganizationId", Description = "OpenAIProvider_OrganizationId_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 10)]
        [FlowBloxUI(Factory = UIFactory.Default)]
        public string OrganizationId { get; set; }


        public override string ProviderType => "OpenAI";

        public OpenAIProvider()
        {
            BaseUrl = "https://api.openai.com/v1";
            DefaultModel = "gpt-5.6-terra";
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
                    Error = "OpenAI API key is empty after field resolution."
                };
            }

            var resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(request.Model);
            if (string.IsNullOrWhiteSpace(resolvedModel))
                resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(DefaultModel);

            var resolvedBaseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl);
            if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
                throw new InvalidOperationException("OpenAI base URL is empty after field resolution.");

            var resolvedOrganizationId = FlowBloxFieldHelper.ReplaceFieldsInString(OrganizationId);

#pragma warning disable SKEXP0010
            var chatService = string.Equals(resolvedBaseUrl.TrimEnd('/'), "https://api.openai.com/v1", StringComparison.OrdinalIgnoreCase)
                ? new OpenAIChatCompletionService(resolvedModel, resolvedApiKey, resolvedOrganizationId)
                : new OpenAIChatCompletionService(resolvedModel, new Uri(resolvedBaseUrl.TrimEnd('/')), resolvedApiKey, resolvedOrganizationId);
#pragma warning restore SKEXP0010

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

        private static OpenAIPromptExecutionSettings BuildExecutionSettings(AIChatRequest request)
        {
            var settings = new OpenAIPromptExecutionSettings();
            if (request.Temperature is >= 0 and <= 2)
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

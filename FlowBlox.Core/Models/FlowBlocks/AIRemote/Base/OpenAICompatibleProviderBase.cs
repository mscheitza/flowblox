using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    [PluralDisplayName("OpenAICompatibleProviderBase_DisplayName_Plural", typeof(FlowBloxTexts))]
    public abstract class OpenAICompatibleProviderBase : AIProviderBase
    {
        protected abstract string ProviderDisplayName { get; }

        protected virtual string? OrganizationIdForRequest => null;

        protected override bool SupportsReasoningEffort => true;

        protected OpenAICompatibleProviderBase(string baseUrl, string defaultModel)
        {
            BaseUrl = baseUrl;
            DefaultModel = defaultModel;
        }

        protected override async Task<AIResponse> ExecuteChatCoreAsync(AIChatRequest request, CancellationToken ct)
        {
            var resolvedApiKey = FlowBloxFieldHelper.ReplaceFieldsInString(ApiKey);
            if (string.IsNullOrWhiteSpace(resolvedApiKey))
            {
                return new AIResponse
                {
                    Success = false,
                    Error = $"{ProviderDisplayName} API key is empty after field resolution."
                };
            }

            var resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(request.Model);
            if (string.IsNullOrWhiteSpace(resolvedModel))
                resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(DefaultModel);

            var resolvedBaseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl);
            if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
                throw new InvalidOperationException($"{ProviderDisplayName} base URL is empty after field resolution.");

            var resolvedOrganizationId = FlowBloxFieldHelper.ReplaceFieldsInString(OrganizationIdForRequest);
            var timeoutSeconds = ResolveTimeoutSeconds(request);
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };

#pragma warning disable SKEXP0010
            var chatService = string.Equals(resolvedBaseUrl.TrimEnd('/'), "https://api.openai.com/v1", StringComparison.OrdinalIgnoreCase)
                ? new OpenAIChatCompletionService(resolvedModel, resolvedApiKey, resolvedOrganizationId, httpClient, loggerFactory: null)
                : new OpenAIChatCompletionService(resolvedModel, new Uri(resolvedBaseUrl.TrimEnd('/')), resolvedApiKey, resolvedOrganizationId, httpClient, loggerFactory: null);
#pragma warning restore SKEXP0010

            var response = await chatService.GetChatMessageContentAsync(
                BuildChatHistory(request),
                BuildExecutionSettings(request),
                kernel: null,
                cancellationToken: ct).ConfigureAwait(false);

            var usage = new OpenAICompatibleUsageReporter(ProviderDisplayName).ReportUsage(response);

            return new AIResponse
            {
                Success = true,
                Text = response?.Content ?? string.Empty,
                PromptTokens = usage.PromptTokens,
                CompletionTokens = usage.CompletionTokens
            };
        }

        protected virtual OpenAIPromptExecutionSettings BuildExecutionSettings(AIChatRequest request)
        {
            var settings = new OpenAIPromptExecutionSettings();
            if (request.Temperature is >= 0 and <= 2)
                settings.Temperature = request.Temperature;

            if (request.MaxTokens is > 0)
                settings.MaxTokens = request.MaxTokens;

            settings.ReasoningEffort = GetReasoningEffortValue();
            ConfigureExecutionSettings(settings, request);

            return settings;
        }

        protected virtual string GetReasoningEffortValue()
        {
            return ToReasoningEffortValue(ReasoningEffort);
        }

        protected virtual void ConfigureExecutionSettings(OpenAIPromptExecutionSettings settings, AIChatRequest request)
        {
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
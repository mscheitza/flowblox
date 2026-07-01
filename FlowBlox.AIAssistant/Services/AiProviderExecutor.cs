using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using FlowBlox.Core.Provider.Project;

namespace FlowBlox.AIAssistant.Services
{
    public class AiProviderExecutor : IAiExecutor
    {
        public async Task<AiExecutorResult> ExecuteChatAsync(
            AIChatRequest request,
            AssistantConfiguration configuration,
            CancellationToken ct)
        {
            try
            {
                var project = FlowBloxProjectManager.Instance.ActiveProject;
                if (project == null)
                {
                    return new AiExecutorResult
                    {
                        Success = false,
                        Error = "No active project is loaded."
                    };
                }

                var provider = configuration?.Provider ?? new OpenAIProvider();
                request ??= new AIChatRequest();
                request.Model = string.IsNullOrWhiteSpace(request.Model)
                    ? provider.DefaultModel
                    : request.Model;

                if (configuration?.Temperature.HasValue == true)
                    request.Temperature = configuration.Temperature;

                if (configuration?.MaxTokens is > 0)
                    request.MaxTokens = configuration.MaxTokens;

                if (string.IsNullOrWhiteSpace(request.Source))
                    request.Source = "FlowBloxAIAssistant";

                provider.PrepareExecution();
                try
                {
                    var response = await provider.ExecuteChatAsync(request, ct).ConfigureAwait(false);
                    return new AiExecutorResult
                    {
                        Success = response.Success,
                        OutputText = response.Text ?? string.Empty,
                        RawOutput = response.Text ?? string.Empty,
                        Error = response.Error ?? string.Empty,
                        ResponseId = response.ResponseId ?? string.Empty
                    };
                }
                finally
                {
                    provider.CompleteExecution();
                }
            }
            catch (Exception ex)
            {
                return new AiExecutorResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}

using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using FlowBlox.Core.Provider.Project;

namespace FlowBlox.AIAssistant.Services
{
    public class AiProviderExecutor : IAiExecutor
    {
        public async Task<AiExecutorResult> ExecutePromptAsync(
            string systemPrompt,
            string userPrompt,
            AssistantConfiguration configuration,
            string previousResponseId,
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

                provider.PrepareExecution();
                try
                {
                    var request = new AIRequest
                    {
                        Prompt = userPrompt,
                        SystemInstruction = systemPrompt,
                        Model = provider.DefaultModel,
                        Temperature = configuration?.Temperature ?? 0.0,
                        MaxTokens = configuration?.MaxTokens,
                        PreviousResponseId = provider.SupportsNativeResponseContinuation ? previousResponseId : string.Empty
                    };

                    request.Meta["Source"] = "FlowBloxAIAssistant";
                    request.Meta["RequireResponseStorage"] = true;

                    var response = await provider.ExecuteAsync(request, ct).ConfigureAwait(false);
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

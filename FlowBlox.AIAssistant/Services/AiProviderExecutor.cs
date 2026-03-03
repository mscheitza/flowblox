using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider.Project;

namespace FlowBlox.AIAssistant.Services
{
    public class AiProviderExecutor : IAiExecutor
    {
        public async Task<AiExecutorResult> ExecutePromptAsync(
            string systemPrompt,
            string userPrompt,
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

                var runtime = new TransientRuntime(project);
                var provider = configuration?.Provider ?? new OpenAIProvider();

                provider.RuntimeStarted(runtime);
                try
                {
                    var request = new AIRequest
                    {
                        Prompt = userPrompt,
                        SystemInstruction = systemPrompt,
                        Model = provider.DefaultModel,
                        Temperature = configuration?.Temperature ?? 0.0,
                        MaxTokens = configuration?.MaxTokens
                    };

                    request.Meta["Source"] = "FlowBloxAIAssistant";

                    var response = await provider.ExecuteAsync(runtime, request, ct).ConfigureAwait(false);
                    return new AiExecutorResult
                    {
                        Success = response.Success,
                        OutputText = response.Text ?? string.Empty,
                        RawOutput = response.Text ?? string.Empty,
                        Error = response.Error ?? string.Empty
                    };
                }
                finally
                {
                    provider.RuntimeFinished(runtime);
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

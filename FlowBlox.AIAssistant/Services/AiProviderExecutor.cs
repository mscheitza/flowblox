using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;

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
                var provider = ResolveProvider(configuration);
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
                provider.RuntimeStarted(runtime);
                try
                {
                    var request = new AIRequest
                    {
                        Prompt = userPrompt,
                        SystemInstruction = systemPrompt,
                        Model = string.IsNullOrWhiteSpace(configuration.Model) ? provider.DefaultModel : configuration.Model,
                        Temperature = configuration.Temperature ?? 0.0,
                        MaxTokens = configuration.MaxTokens
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

        private static AIProviderBase ResolveProvider(AssistantConfiguration configuration)
        {
            var providerType = configuration.Provider?.Type;
            if (!string.IsNullOrWhiteSpace(configuration.Provider?.Name))
            {
                var named = FlowBloxRegistryProvider.GetRegistry()
                    .GetManagedObjects<AIProviderBase>()
                    .FirstOrDefault(x => string.Equals(x.Name, configuration.Provider.Name, StringComparison.OrdinalIgnoreCase));
                if (named != null)
                    return named;
            }

            AIProviderBase provider = providerType?.Trim().ToLowerInvariant() switch
            {
                "openai" => new OpenAIProvider(),
                "openaiprovider" => new OpenAIProvider(),
                _ => new OpenAIProvider()
            };

            if (!string.IsNullOrWhiteSpace(configuration.Endpoint))
                provider.BaseUrl = configuration.Endpoint;

            if (!string.IsNullOrWhiteSpace(configuration.ApiKey))
                provider.ApiKey = configuration.ApiKey;

            if (!string.IsNullOrWhiteSpace(configuration.Model))
                provider.DefaultModel = configuration.Model;

            return provider;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    [Display(Name = "TypeNames_AIProvider", ResourceType = typeof(FlowBloxTexts))]
    public abstract class AIProviderBase : ManagedObject, IAIProvider
    {
        [Required]
        [Display(Name = "AIProvider_ApiKey", Description = "AIProvider_ApiKey_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(PasswordChar = '*')]
        public string ApiKey { get; set; }

        [Display(Name = "AIProvider_DefaultModel", Description = "AIProvider_DefaultModel_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string DefaultModel { get; set; }

        [Display(Name = "AIProvider_TimeoutSeconds", Description = "AIProvider_TimeoutSeconds_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public int TimeoutSeconds { get; set; }

        [Display(Name = "AIProvider_BaseUrl", Description = "AIProvider_BaseUrl_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string BaseUrl { get; set; }

        public abstract string ProviderType { get; }

        protected AIProviderBase()
        {
            TimeoutSeconds = 60;
        }

        public async Task<AIResponse> ExecuteAsync(BaseRuntime runtime, AIRequest request, CancellationToken ct)
        {
            if (runtime == null)
                throw new ArgumentNullException(nameof(runtime));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return new AIResponse
                {
                    Success = false,
                    Error = "Prompt is empty."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Model))
                request.Model = DefaultModel;

            var timeoutSeconds = request.TimeoutSecondsOverride ?? TimeoutSeconds;
            if (timeoutSeconds <= 0)
                timeoutSeconds = 60;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                return await ExecuteCoreAsync(request, cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException ex)
            {
                runtime.Report($"AI request cancelled or timed out.", FlowBloxLogLevel.Error, ex);

                if (ct.IsCancellationRequested)
                {
                    return new AIResponse
                    {
                        Success = false,
                        Error = "AI request was cancelled."
                    };
                }
                else
                {
                    return new AIResponse
                    {
                        Success = false,
                        Error = "AI request timed out."
                    };
                }   
            }
            catch (HttpRequestException ex)
            {
                runtime.Report($"HTTP error during AI request.", FlowBloxLogLevel.Error, ex);

                return new AIResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
            catch (Exception ex)
            {
                runtime.Report($"Unexpected error during AI request.", FlowBloxLogLevel.Error, ex);

                return new AIResponse
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }

        protected abstract Task<AIResponse> ExecuteCoreAsync(AIRequest request, CancellationToken ct);

        public override void OptionsInit(List<OptionElement> defaults)
        {
            defaults.Add(new OptionElement(
                "AI.DefaultTimeoutSeconds",
                "60",
                "Default timeout for AI provider calls.",
                OptionElement.OptionType.Integer));
        }
    }
}

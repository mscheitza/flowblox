using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote
{
    [FlowBlockUIGroup("AIPromptFlowBlock_Groups_Request", 0)]
    [FlowBlockUIGroup("AIPromptFlowBlock_Groups_Output", 1)]
    [Display(Name = "AIPromptFlowBlock_DisplayName", Description = "AIPromptFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class AIPromptFlowBlock : BaseSingleResultFlowBlock
    {
        [Required]
        [Display(Name = "AIPromptFlowBlock_Provider", Description = "AIPromptFlowBlock_Provider_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Association,
            SelectionFilterMethod = nameof(GetPossibleProviders),
            SelectionDisplayMember = nameof(ManagedObject.Name))]
        public AIProviderBase Provider { get; set; }

        [Required]
        [Display(Name = "AIPromptFlowBlock_PromptTemplate", Description = "AIPromptFlowBlock_PromptTemplate_Tooltip", ResourceType = typeof(FlowBloxTexts), GroupName = "AIPromptFlowBlock_Groups_Request", Order = 1)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = true, IsCodingMode = true)]
        public string PromptTemplate { get; set; }

        [Display(Name = "AIPromptFlowBlock_SystemInstruction", Description = "AIPromptFlowBlock_SystemInstruction_Tooltip", ResourceType = typeof(FlowBloxTexts), GroupName = "AIPromptFlowBlock_Groups_Request", Order = 2)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = true)]
        public string SystemInstruction { get; set; }

        [Display(Name = "AIPromptFlowBlock_ModelOverride", Description = "AIPromptFlowBlock_ModelOverride_Tooltip", ResourceType = typeof(FlowBloxTexts), GroupName = "AIPromptFlowBlock_Groups_Request", Order = 3)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string ModelOverride { get; set; }

        [Display(Name = "AIPromptFlowBlock_Temperature", Description = "AIPromptFlowBlock_Temperature_Tooltip", ResourceType = typeof(FlowBloxTexts), GroupName = "AIPromptFlowBlock_Groups_Request", Order = 4)]
        public double Temperature { get; set; }

        [Display(Name = "AIPromptFlowBlock_MaxTokens", Description = "AIPromptFlowBlock_MaxTokens_Tooltip", ResourceType = typeof(FlowBloxTexts), GroupName = "AIPromptFlowBlock_Groups_Request", Order = 5)]
        public int? MaxTokens { get; set; }

        [Display(Name = "AIPromptFlowBlock_TimeoutSecondsOverride", Description = "AIPromptFlowBlock_TimeoutSecondsOverride_Tooltip", ResourceType = typeof(FlowBloxTexts), GroupName = "AIPromptFlowBlock_Groups_Request", Order = 6)]
        public int? TimeoutSecondsOverride { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.robot, 16, SKColors.DeepSkyBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.robot, 32, SKColors.DeepSkyBlue);

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.AI;
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override List<string> GetDisplayableProperties()
        {
            var props = base.GetDisplayableProperties();
            props.Add(nameof(Provider));
            return props;
        }

        public List<AIProviderBase> GetPossibleProviders()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            return registry.GetManagedObjects<AIProviderBase>().ToList();
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                var promptResolved = FlowBloxFieldHelper.ReplaceFieldsInString(this.PromptTemplate);

                var req = new AIRequest
                {
                    Prompt = promptResolved,
                    SystemInstruction = this.SystemInstruction,
                    Model = string.IsNullOrWhiteSpace(this.ModelOverride) ? null : this.ModelOverride,
                    Temperature = this.Temperature,
                    MaxTokens = this.MaxTokens,
                    TimeoutSecondsOverride = this.TimeoutSecondsOverride,
                };

                req.Meta["FlowBlock"] = this.Name;
                req.Meta["FlowBlockType"] = GetType().Name;

                var ct = runtime.GetCancellationToken();
                var resp = Provider.ExecuteAsync(runtime, req, ct).GetAwaiter().GetResult();

                if (!resp.Success)
                {
                    var logMessage =
                        $"AI prompt execution failed.{Environment.NewLine}" +
                        $"Provider: {Provider?.Name ?? "n/a"} ({Provider?.ProviderType ?? "n/a"}){Environment.NewLine}" +
                        $"Error: {resp.Error ?? "n/a"}{Environment.NewLine}" +
                        $"Prompt:{Environment.NewLine}{promptResolved}";

                    runtime.Report(logMessage);

                    CreateNotification(runtime, AIPromptNotifications.AIPromptExecutionFailed);

                    return;
                }

                var output = resp.Text;
                GenerateResult(runtime, output);
            });
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(AIPromptNotifications));
                return notificationTypes;
            }
        }

        public enum AIPromptNotifications
        {
            [FlowBlockNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "AI prompt execution failed")]
            AIPromptExecutionFailed
        }
    }
}

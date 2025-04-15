using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.SequenceDetection;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Util.Resources;
using FlowBlox.SequenceDetection;
using Google.Protobuf;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using ZstdSharp.Unsafe;
using static FlowBlox.Core.Models.FlowBlocks.RegexSelectorFlowBlock;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [Display(Name = "SequenceDetectionFlowBlock_DisplayName", Description = "SequenceDetectionFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class SequenceDetectionFlowBlock : BasePipeFlowBlock
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.repeat, 16, SKColors.MediumPurple);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.repeat, 32, SKColors.MediumPurple);

        public SequenceDetectionFlowBlock() : base()
        {
            
        }

        public override void OnAfterCreate()
        {
            CreateDefaultGenerationStrategy();
            base.OnAfterCreate();
        }

        private void CreateDefaultGenerationStrategy()
        {
            this.GenerationStrategies.Add(new SequenceDetectionGenerationStrategy(this));
        }

        [Display(Name = "SequenceDetectionFlowBlock_SequenceDetectionPattern", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(ReadOnly = true)]
        [FlowBlockTextBox(MultiLine = true, IsCodingMode = true)]
        public string SequenceDetectionPattern { get; set; }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Selection;

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(SequenceDetectionNotifications));
                return notificationTypes;
            }
        }

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                if (string.IsNullOrEmpty(this.SequenceDetectionPattern))
                {
                    CreateNotification(runtime, SequenceDetectionNotifications.NoSequencePatternSpecified);
                    GenerateResult(runtime);
                    return;
                }

                var sequenceDetectionPattern = JsonConvert.DeserializeObject<SequenceDetectionPattern>(this.SequenceDetectionPattern);
                
                var matchValues = new List<string>();
                SequenceSearch.Instance.SearchFor(this.InputField.StringValue, sequenceDetectionPattern, ref matchValues);

                if (!matchValues.Any())
                {
                    runtime.Report("The pattern returned no matches.", FlowBloxLogLevel.Warning);
                    CreateNotification(runtime, RegexSelectorNotifications.ReturnedNoMatches);
                }

                GenerateResult(runtime, matchValues);
            });
        }

        public enum SequenceDetectionNotifications
        {
            [FlowBlockNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "Pattern returned no matches")]
            ReturnedNoMatches,

            [FlowBlockNotification(NotificationType = NotificationType.Error)]
            [Display(Name = "No sequence pattern specified")]
            NoSequencePatternSpecified
        }
    }
}

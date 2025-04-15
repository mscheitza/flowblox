using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.StartEndPatternSelector;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FlowBlox.Core.Models.FlowBlocks
{
    public class StartEndPattern : FlowBloxReactiveObject
    {
        [Display(Name = "StartEndPattern_StartPattern", ResourceType = typeof(FlowBloxTexts))]
        public string StartPattern { get; set; }

        [Display(Name = "StartEndPattern_EndPattern", ResourceType = typeof(FlowBloxTexts))]
        public string EndPattern { get; set; }

        [Display(Name = "StartEndPattern_Index", ResourceType = typeof(FlowBloxTexts))]
        public int? Index { get; set; }
    }

    [Display(Name = "StartEndPatternSelectorFlowBlock_DisplayName", Description = "StartEndPatternSelectorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class StartEndPatternSelectorFlowBlock : BasePipeFlowBlock
    {
        [Display(Name = "StartEndPatternSelectorFlowBlock_StartEndPatterns", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.GridView)]
        [FlowBlockDataGrid(IsMovable = true)]
        public ObservableCollection<StartEndPattern> StartEndPatterns { get; set; }

        public override SKImage Icon16 => base.Icon16;
        public override SKImage Icon32 => base.Icon32;

        public StartEndPatternSelectorFlowBlock() : base()
        {
            this.StartEndPatterns = new ObservableCollection<StartEndPattern>();
        }

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Selection;

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

                string content = this.InputField.StringValue;

                List<string> results = new List<string>()
                {
                    content
                };

                foreach (var startEndPattern in this.StartEndPatterns)
                {
                    results = PatternSelector.SelectPatternFromContext(results, startEndPattern.StartPattern, startEndPattern.EndPattern, true);
                    runtime.Report($"Found {results.Count} results for pattern combination \"{startEndPattern.StartPattern}\" and \"{startEndPattern.EndPattern}\"");

                    if (startEndPattern.Index != null)
                    {
                        if (results.Count > startEndPattern.Index)
                        {
                            results = new List<string>()
                        {
                            results.ElementAt(startEndPattern.Index.Value)
                        };
                            runtime.Report($"Selected index {startEndPattern.Index.Value} of {results.Count} results.");
                        }
                        else
                        {
                            results = new List<string>();
                            runtime.Report($"Index {startEndPattern.Index.Value} is out of range at {results.Count} results.");
                        }
                    }
                }

                if (results.Count == 0)
                    CreateNotification(runtime, StartEndPatternSelectorNotifications.ReturnedNoMatches);

                GenerateResult(runtime, results);
            });
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(StartEndPatternSelectorNotifications));
                return notificationTypes;
            }
        }
        
        public enum StartEndPatternSelectorNotifications
        {
            [FlowBlockNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "The pattern selector returned no matches.")]
            ReturnedNoMatches
        }
    }
}

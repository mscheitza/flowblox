using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.StartEndPatternSelector;
using FlowBlox.Core.Models.Runtime;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks
{
    public class StartEndPattern : FlowBloxReactiveObject
    {
        [Required]
        [Display(Name = "StartEndPattern_StartPattern", ResourceType = typeof(FlowBloxTexts))]
        public string StartPattern { get; set; }

        [Display(Name = "StartEndPattern_EndPattern", ResourceType = typeof(FlowBloxTexts))]
        public string EndPattern { get; set; }

        [Display(Name = "StartEndPattern_Index", ResourceType = typeof(FlowBloxTexts))]
        public int? Index { get; set; }

        [Display(Name = "StartEndPattern_ReturnOptions", Description = "StartEndPattern_ReturnOptions_Tooltip", ResourceType = typeof(FlowBloxTexts))]
        public StartEndPatternReturnOptions? ReturnOptions { get; set; }
    }

    [Display(Name = "StartEndPatternSelectorFlowBlock_DisplayName", Description = "StartEndPatternSelectorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    [FlowBloxSpecialExplanation("StartEndPatternSelectorFlowBlock_SpecialExplanation_HierarchicalParsing", Icon = SpecialExplanationIcon.Information)]
    [FlowBloxSpecialExplanation("StartEndPatternSelectorFlowBlock_SpecialExplanation_PatternEntryBehavior", Icon = SpecialExplanationIcon.Information)]
    public class StartEndPatternSelectorFlowBlock : BasePipeFlowBlock
    {
        [Display(Name = "StartEndPatternSelectorFlowBlock_StartEndPatterns", Description = "StartEndPatternSelectorFlowBlock_StartEndPatterns_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.GridView)]
        [FlowBloxDataGrid(IsMovable = true)]
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
                    var endPattern = string.IsNullOrWhiteSpace(startEndPattern.EndPattern)
                        ? null
                        : startEndPattern.EndPattern;

                    results = PatternSelector.SelectPatternFromContext(results, startEndPattern.StartPattern, endPattern, true);
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

                var lastPattern = StartEndPatterns.LastOrDefault();
                results = ApplyReturnOptions(results, lastPattern);

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
            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "The pattern selector returned no matches.")]
            ReturnedNoMatches
        }

        private static List<string> ApplyReturnOptions(IEnumerable<string> values, StartEndPattern startEndPattern)
        {
            if (startEndPattern?.ReturnOptions == null)
                return values.ToList();

            return startEndPattern.ReturnOptions.Value switch
            {
                StartEndPatternReturnOptions.StartPattern => values
                    .Select(val => string.Concat(startEndPattern.StartPattern, val))
                    .ToList(),
                StartEndPatternReturnOptions.EndPattern => values
                    .Select(val => string.Concat(val, startEndPattern.EndPattern))
                    .ToList(),
                StartEndPatternReturnOptions.StartAndEndPattern => values
                    .Select(val => string.Concat(startEndPattern.StartPattern, val, startEndPattern.EndPattern))
                    .ToList(),
                _ => values.ToList()
            };
        }
    }
}


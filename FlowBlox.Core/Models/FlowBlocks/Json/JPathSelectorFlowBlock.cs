using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    [FlowBloxSpecialExplanation("JsonFlowBlocks_SpecialExplanation_JPathSyntax", Icon = SpecialExplanationIcon.Information)]
    [FlowBloxSpecialExplanation("JPathSelectorFlowBlock_SpecialExplanation_PathExamples", Icon = SpecialExplanationIcon.Information)]
    [Display(Name = "JPathSelectorFlowBlock_DisplayName", Description = "JPathSelectorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class JPathSelectorFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "JPathSelectorFlowBlock_JsonContent", Description = "JPathSelectorFlowBlock_JsonContent_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox(IsCodingMode = true, MultiLine = true, SyntaxHighlighting = "JSON")]
        [Required]
        public string JsonContent { get; set; }

        [Display(Name = "JPathSelectorFlowBlock_Path", Description = "JPathSelectorFlowBlock_Path_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection, ToolboxCategory = nameof(FlowBloxToolboxCategory.JPath))]
        [Required]
        public string Path { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.selection_ellipse_arrow_inside, 16, SKColors.Goldenrod);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.selection_ellipse_arrow_inside, 32, SKColors.Goldenrod);

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Json;
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(JPathSelectorNotifications));
                return notificationTypes;
            }
        }

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(Path));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);

                var jsonText = FlowBloxFieldHelper.ReplaceFieldsInString(JsonContent);
                var path = FlowBloxFieldHelper.ReplaceFieldsInString(Path);
                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    CreateNotification(runtime, JPathSelectorNotifications.JsonContentIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    CreateNotification(runtime, JPathSelectorNotifications.PathIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                var rootToken = JToken.Parse(jsonText);
                var resultTokens = rootToken.SelectTokens(path, errorWhenNoMatch: false).ToList();
                if (resultTokens.Count == 0)
                {
                    CreateNotification(runtime, JPathSelectorNotifications.JsonTokenCouldNotBeResolved);
                    GenerateResult(runtime);
                    return;
                }

                var results = new List<string>();

                foreach (var resultToken in resultTokens)
                {
                    if (resultToken is JArray array)
                    {
                        foreach (var item in array)
                            results.Add(SerializeToken(item));
                    }
                    else
                    {
                        results.Add(SerializeToken(resultToken));
                    }
                }

                if (results.Count == 0)
                    CreateNotification(runtime, JPathSelectorNotifications.ReturnedNoMatches);

                GenerateResult(runtime, results);
            });
        }

        private static string SerializeToken(JToken token)
        {
            return token is JValue value
                ? value.Value?.ToString()
                : JsonConvert.SerializeObject(token, Formatting.None);
        }

        public enum JPathSelectorNotifications
        {
            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "JSON content is empty")]
            JsonContentIsEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "JSON path is empty")]
            PathIsEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "JSON token could not be resolved by path")]
            JsonTokenCouldNotBeResolved,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "JSON path returned no matches")]
            ReturnedNoMatches
        }
    }
}

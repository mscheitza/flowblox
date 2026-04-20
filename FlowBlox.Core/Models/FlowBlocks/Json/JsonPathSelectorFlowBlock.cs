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
    [Display(Name = "JsonPathSelectorFlowBlock_DisplayName", Description = "JsonPathSelectorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class JsonPathSelectorFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "JsonPathSelectorFlowBlock_JsonContent", Description = "JsonPathSelectorFlowBlock_JsonContent_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox(IsCodingMode = true, MultiLine = true, SyntaxHighlighting = "JSON")]
        [Required]
        public string JsonContent { get; set; }

        [Display(Name = "JsonPathSelectorFlowBlock_Path", Description = "JsonPathSelectorFlowBlock_Path_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
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
                notificationTypes.Add(typeof(JsonPathSelectorNotifications));
                return notificationTypes;
            }
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
                    CreateNotification(runtime, JsonPathSelectorNotifications.JsonContentIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    CreateNotification(runtime, JsonPathSelectorNotifications.PathIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                var rootToken = JToken.Parse(jsonText);
                if (rootToken is not JObject rootObj)
                    throw new InvalidOperationException("The provided JSON must represent an object at the root.");

                var resultToken = JsonPathSelector.GetJToken(rootObj, path, out _, out _);
                if (resultToken == null)
                {
                    CreateNotification(runtime, JsonPathSelectorNotifications.JsonTokenCouldNotBeResolved);
                    GenerateResult(runtime);
                    return;
                }

                var results = new List<string>();

                switch (resultToken)
                {
                    case JValue jv:
                        results.Add(jv.Value?.ToString());
                        break;

                    case JArray arr:
                        foreach (var item in arr)
                        {
                            if (item is JValue iv)
                                results.Add(iv.Value?.ToString());
                            else
                                results.Add(JsonConvert.SerializeObject(item, Formatting.None));
                        }
                        break;

                    default:
                        results.Add(JsonConvert.SerializeObject(resultToken, Formatting.None));
                        break;
                }

                if (results.Count == 0)
                    CreateNotification(runtime, JsonPathSelectorNotifications.ReturnedNoMatches);

                GenerateResult(runtime, results);
            });
        }

        public enum JsonPathSelectorNotifications
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

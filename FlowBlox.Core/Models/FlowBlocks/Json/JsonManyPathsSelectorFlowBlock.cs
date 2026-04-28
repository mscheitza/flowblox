using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    [Display(Name = "JsonManyPathsSelectorMappingEntry_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class JsonManyPathsSelectorMappingEntry : FlowBloxReactiveObject
    {
        [Required]
        [Display(Name = "JsonManyPathsSelectorMappingEntry_JsonPath", Description = "JsonManyPathsSelectorMappingEntry_JsonPath_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string JsonPath { get; set; }

        [Required]
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.Association, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public FieldElement Field { get; set; }

        public bool IsDeletable(out List<IFlowBloxComponent> dependencies)
        {
            if (Field == null)
            {
                dependencies = null;
                return true;
            }

            return Field.IsDeletable(out dependencies);
        }
    }

    [FlowBloxUIGroup("JsonManyPathsSelectorFlowBlock_Groups_Mappings", 0)]
    [Display(Name = "JsonManyPathsSelectorFlowBlock_DisplayName", Description = "JsonManyPathsSelectorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class JsonManyPathsSelectorFlowBlock : BaseResultFlowBlock
    {
        [Required]
        [Display(Name = "JsonManyPathsSelectorFlowBlock_JsonContent", Description = "JsonManyPathsSelectorFlowBlock_JsonContent_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox(IsCodingMode = true, MultiLine = true, SyntaxHighlighting = "JSON")]
        public string JsonContent { get; set; }

        [Display(Name = "JsonManyPathsSelectorFlowBlock_MappingEntries", ResourceType = typeof(FlowBloxTexts), GroupName = "JsonManyPathsSelectorFlowBlock_Groups_Mappings", Order = 1)]
        [FlowBloxUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        [FlowBloxDataGrid(GridColumnMemberNames =
            [
                nameof(JsonManyPathsSelectorMappingEntry.JsonPath),
                nameof(JsonManyPathsSelectorMappingEntry.Field)
            ])]

        public ObservableCollection<JsonManyPathsSelectorMappingEntry> MappingEntries { get; set; } = new ObservableCollection<JsonManyPathsSelectorMappingEntry>();

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.selection_ellipse_arrow_inside, 16, SKColors.CadetBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.selection_ellipse_arrow_inside, 32, SKColors.CadetBlue);

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Json;
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(JsonManyPathsSelectorNotifications));
                return notificationTypes;
            }
        }

        public override List<FieldElement> Fields
        {
            get
            {
                return MappingEntries
                    .Select(x => x.Field)
                    .ExceptNull()
                    .ToList();
            }
        }

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(JsonContent));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                var jsonText = FlowBloxFieldHelper.ReplaceFieldsInString(JsonContent ?? string.Empty)?.Trim();
                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    CreateNotification(runtime, JsonManyPathsSelectorNotifications.JsonContentIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                if (MappingEntries == null || MappingEntries.Count == 0)
                {
                    CreateNotification(runtime, JsonManyPathsSelectorNotifications.MappingEntriesAreEmpty);
                    GenerateResult(runtime);
                    return;
                }

                JToken rootToken;
                try
                {
                    rootToken = JToken.Parse(jsonText);
                }
                catch (Exception)
                {
                    CreateNotification(runtime, JsonManyPathsSelectorNotifications.JsonContentInvalid);
                    GenerateResult(runtime);
                    return;
                }

                var row = new Dictionary<FieldElement, string>();
                var hadUnresolvedPaths = false;

                foreach (var mappingEntry in MappingEntries)
                {
                    if (mappingEntry?.Field == null)
                        continue;

                    var resolvedPath = FlowBloxFieldHelper.ReplaceFieldsInString(mappingEntry.JsonPath ?? string.Empty)?.Trim();
                    if (string.IsNullOrWhiteSpace(resolvedPath))
                    {
                        hadUnresolvedPaths = true;
                        continue;
                    }

                    JToken resultToken;
                    try
                    {
                        resultToken = JsonPathSelector.GetJToken(rootToken, resolvedPath, out _, out _);
                    }
                    catch (Exception)
                    {
                        hadUnresolvedPaths = true;
                        continue;
                    }

                    if (resultToken == null)
                    {
                        hadUnresolvedPaths = true;
                        continue;
                    }

                    row[mappingEntry.Field] = SerializeToken(resultToken);
                }

                if (hadUnresolvedPaths)
                    CreateNotification(runtime, JsonManyPathsSelectorNotifications.PathCouldNotBeResolved);

                if (row.Count == 0)
                {
                    CreateNotification(runtime, JsonManyPathsSelectorNotifications.ReturnedNoMatches);
                    GenerateResult(runtime);
                    return;
                }

                GenerateResult(runtime, [row]);
            });
        }

        private static string SerializeToken(JToken token)
        {
            if (token is JValue value)
                return value.Value?.ToString() ?? string.Empty;

            return JsonConvert.SerializeObject(token, Formatting.None);
        }

        public enum JsonManyPathsSelectorNotifications
        {
            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "JSON content is empty")]
            JsonContentIsEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "No mapping entries configured")]
            MappingEntriesAreEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "JSON content is invalid")]
            JsonContentInvalid,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "One or more JSON paths could not be resolved")]
            PathCouldNotBeResolved,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "JSON path mapping returned no matches")]
            ReturnedNoMatches
        }
    }
}

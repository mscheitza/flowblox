using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    [Display(Name = "JsonObjectWriterFlowBlock_DisplayName", Description = "JsonObjectWriterFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class JsonObjectWriterFlowBlock : BaseFlowBlock
    {
        [Display(Name = "JsonObjectWriterFlowBlock_AssociatedJsonObject", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [AssociatedFlowBlockResolvable()]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionFilterMethod = nameof(GetPossibleJsonObjectFlowBlocks),
            SelectionDisplayMember = nameof(Name))]
        public JsonObjectFlowBlock AssociatedJsonObject { get; set; }

        public List<JsonObjectFlowBlock> GetPossibleJsonObjectFlowBlocks()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFlowBlocks<JsonObjectFlowBlock>()
                .ToList();
        }

        [Display(Name = "JsonObjectWriterFlowBlock_AssociatedJsonObjectWriter", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
           SelectionFilterMethod = nameof(GetPossibleJsonObjectWriters),
           SelectionDisplayMember = nameof(BaseFlowBlock.Name))]
        public JsonObjectWriterFlowBlock AssociatedJsonObjectWriter { get; set; }

        public List<JsonObjectWriterFlowBlock> GetPossibleJsonObjectWriters()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            return registry.GetFlowBlocks<JsonObjectWriterFlowBlock>().ToList();
        }

        [Display(Name = "JsonObjectWriterFlowBlock_JsonAssignments", ResourceType = typeof(FlowBloxTexts), Order = 10)]
        [FlowBlockUI(Factory = UIFactory.GridView, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public ObservableCollection<JsonPropertyValueAssignment> Assignments { get; set; } = new();


        [Display(Name = "JsonObjectWriterFlowBlock_Path", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public string Path { get; set; }

        [JsonIgnore]
        [DeepCopierIgnore]
        public JObject CreatedOrUpdatedObject { get; private set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.code_tags, 16, SKColors.Orange);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.code_tags, 32, SKColors.Orange);
        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Json;
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(AssociatedJsonObject));
            properties.Add(nameof(AssociatedJsonObjectWriter));
            properties.Add(nameof(Path));
            properties.Add(nameof(Assignments));
            return properties;
        }

        protected override void OnReferencedFieldNameChanged(FieldElement field, string oldFQFieldName, string newFQFieldName)
        {
            foreach (var a in Assignments)
            {
                a.PropertyName = FlowBloxFieldHelper.ReplaceFQName(a.PropertyName, oldFQFieldName, newFQFieldName);
                a.Value = FlowBloxFieldHelper.ReplaceFQName(a.Value, oldFQFieldName, newFQFieldName);
            }
            base.OnReferencedFieldNameChanged(field, oldFQFieldName, newFQFieldName);
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);

                var associated = this.AssociatedJsonObject ?? GetPreviousFlowBlockOnPath<JsonObjectFlowBlock>(this);
                if (associated == null)
                    throw new InvalidOperationException("No JSON object source is assigned to the flow block.");

                if (associated.InternalJsonObject == null)
                    throw new InvalidOperationException("The JSON object in the source flow block is null or has not been initialized.");

                // Determine root/parent
                JToken root = associated.InternalJsonObject;
                if (AssociatedJsonObjectWriter != null)
                {
                    if (AssociatedJsonObjectWriter.CreatedOrUpdatedObject == null)
                        throw new InvalidOperationException("SourceObject was specified, but no CreatedToken exists.");
                    root = AssociatedJsonObjectWriter.CreatedOrUpdatedObject;
                }

                // Search/create parent via path
                var effectivePath = string.IsNullOrWhiteSpace(Path) ? "" : Path;
                var current = JsonPathSelector.GetJToken(root, effectivePath, out JToken parent, out string propertyName);

                JObject createdOrUpdatedObject;
                if (current is JArray parr)
                {
                    createdOrUpdatedObject = new JObject();
                    parr.Add(createdOrUpdatedObject);
                }
                else if (current is null)
                {
                    createdOrUpdatedObject = new JObject();
                    parent[propertyName] = createdOrUpdatedObject;
                }
                else if (current is JObject jObject)
                {
                    createdOrUpdatedObject = jObject;
                }
                else
                {
                    throw new InvalidOperationException("The target parent for the new node must be an object or array.");
                }

                CreatedOrUpdatedObject = createdOrUpdatedObject;
                ApplyAssignments(createdOrUpdatedObject);
                ExecuteNextFlowBlocks(runtime);
            });
        }

        private void ApplyAssignments(JObject jObject)
        {
            if (jObject == null)
                throw new ArgumentNullException(nameof(jObject));

            foreach (var a in Assignments)
            {
                var propertyName = FlowBloxFieldHelper.ReplaceFieldsInString(a.PropertyName);
                var valueObj = a.FieldValue != null
                    ? a.FieldValue.Value
                    : FlowBloxFieldHelper.ReplaceFieldsInString(a.Value);

                if (a.IsArray)
                {
                    if (!jObject.TryGetValue(propertyName, out var token) || token.Type != JTokenType.Array)
                    {
                        token = new JArray();
                        jObject[propertyName] = token;
                    }

                    if (token is JArray arr)
                        arr.Add(valueObj != null ? JToken.FromObject(valueObj) : JValue.CreateNull());
                    else
                        throw new InvalidOperationException($"Property '{propertyName}' exists but is not an array.");
                }
                else
                {
                    jObject[propertyName] = valueObj != null
                        ? JToken.FromObject(valueObj)
                        : JValue.CreateNull();
                }
            }
        }
    }
}
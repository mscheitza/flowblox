using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    [Display(Name = "JsonPathSelectorFlowBlock_DisplayName", Description = "JsonPathSelectorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class JsonPathSelectorFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "JsonPathSelectorFlowBlock_JsonContent", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(IsCodingMode = true, MultiLine = true, SyntaxHighlighting = "JSON")]
        [Required]
        public string JsonContent { get; set; }

        [Display(Name = "JsonPathSelectorFlowBlock_Path", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [Required]
        public string Path { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.selection_ellipse_arrow_inside, 16, SKColors.Goldenrod);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.selection_ellipse_arrow_inside, 32, SKColors.Goldenrod);

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Json;
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override bool Execute(BaseRuntime runtime, object data)
        {
            // TODO: Demo Projekt CsvToJson fertigstellen
            //       "Öffnen (Bearbeitung möglich, Standard-App)"
            //       "Öffnen (Bearbeitung möglich, Standardeditor)"   
            //       JsonPath Example erstellen
            //       XmlUpdaterFlowBlock entfernen ---- XmlDocumentNodeAppenderFlowBlock in XmlDocumentNodeWriterFlowBlock umbenennen mit CreateOrUpdate Logik analog zu Json
            //       XmlAssignableBase entfernen (Leichtgewichtiger implementieren)

            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);

                var jsonText = FlowBloxFieldHelper.ReplaceFieldsInString(JsonContent);
                var path = FlowBloxFieldHelper.ReplaceFieldsInString(Path);

                var rootToken = JToken.Parse(jsonText);
                if (rootToken is not JObject rootObj)
                    throw new InvalidOperationException("The provided JSON must represent an object at the root.");

                var resultToken = JsonPathSelector.GetJToken(rootObj, path, out _, out _);
                if (resultToken == null)
                    throw new InvalidOperationException($"Path '{path}' does not exist in the JSON structure.");

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

                GenerateResult(runtime, results);
            });
        }
    }
}

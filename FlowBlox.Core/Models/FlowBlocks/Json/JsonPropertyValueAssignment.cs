using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.Fields;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    [Display(Name = "JsonPropertyValueAssignment_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class JsonPropertyValueAssignment : FlowBloxReactiveObject
    {
        [Display(Name = "JsonPropertyValueAssignment_PropertyName", ResourceType = typeof(FlowBloxTexts))]
        public string PropertyName { get; set; }

        [Display(Name = "JsonPropertyValueAssignment_Value", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public string Value { get; set; }

        [Display(Name = "JsonPropertyValueAssignment_FieldValue", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(Factory = UIFactory.ComboBox, SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName),
            SelectionFilterMethod = nameof(BaseFlowBlock.GetPossibleFieldElements))]
        public FieldElement FieldValue { get; set; }

        [Display(Name = "JsonPropertyValueAssignment_IsArray", ResourceType = typeof(FlowBloxTexts))]
        public bool IsArray { get; set; }
    }
}

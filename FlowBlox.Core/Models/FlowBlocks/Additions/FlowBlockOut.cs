using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    public class FlowBlockOutDatasetFieldValueMapping
    {
        public Dictionary<FieldElement, string> PrecedingFieldValues { get; set; }

        public Dictionary<FieldElement, string> GetPrecedingAndCurrentFieldValues()
        {
            var combinedValues = new Dictionary<FieldElement, string>(this.PrecedingFieldValues);
            combinedValues[this.Field] = this.Value;
            return combinedValues;
        }

        public FieldElement Field { get; set; }

        public string Value { get; set; }
    }

    public class FlowBlockOutDataset
    {
        public List<FlowBlockOutDatasetFieldValueMapping> FieldValueMappings { get; set; }
    }

    public class FlowBlockOut
    {
        public static FlowBlockOut CreateEmptyResult() 
        {
            return new FlowBlockOut()
            {
                Results = new List<FlowBlockOutDataset>()
            };
        }

        public List<FlowBlockOutDataset> Results { get; set; }

        public int ResultCount => Results?.Count ?? 0;
    }
}

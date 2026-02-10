using FlowBlox.Core.Models.Components;

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

    /// <summary>
    /// Stellt eine Ergebnis-Datensatz Sammlung eines Flow-Blocks dar. 
    /// <para>Eine Ergebnisdatensatz-Sammlung hat mehrere Ergebnisdatensätze.</para>
    /// <para>Ein Ergebnisdatensatz hat pro Feld einen zugewiesenen Wert.</para>
    /// <para>Jedes FlowBlockOutDatasetFieldValueMapping ist hinsichtlich seiner Felder identisch, sie unterscheiden sich hinsichtlich ihrer Werte.</para>
    /// </summary>
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

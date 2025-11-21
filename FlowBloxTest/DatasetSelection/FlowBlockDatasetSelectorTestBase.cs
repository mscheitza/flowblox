using FlowBlox.Core.Models.Components;

namespace FlowBloxTest.DatasetSelection
{
    public class FlowBlockDatasetSelectorTestBase
    {
        protected Dictionary<FieldElement, string> GetPrecedingFieldValues(FieldElement fieldElement, string value) => GetPrecedingFieldValues(new Tuple<FieldElement, string>(fieldElement, value));

        protected Dictionary<FieldElement, string> GetPrecedingFieldValues(params Tuple<FieldElement, string>[] fieldValues)
        {
            var result = new Dictionary<FieldElement, string>();
            foreach (var fieldValue in fieldValues)
            {
                result[fieldValue.Item1] = fieldValue.Item2;
            }
            return result;
        }
    }
}

using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Components;

namespace FlowBlox.Core.Models.FlowBlocks.Base.DatasetSelection
{
    public class FlowBlockCombinationDatasetSelector : FlowBlockDatasetSelectorBase
    {
        public FlowBlockCombinationDatasetSelector(
            Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>> passedResults,
            IList<InputBehaviorAssignment> inputBehaviorAssignments) : base(passedResults, inputBehaviorAssignments)
        {

        }

        public override List<FlowBlockOutDataset> GetResults()
        {
            var resultList = new List<FlowBlockOutDataset>();
            var listOfLists = passedResults.Select(x => x.Value.SelectMany(fbo => fbo.Results).ToList()).ToList();
            GetCombinations(listOfLists, 0, new List<FlowBlockOutDatasetFieldValueMapping>(), resultList);
            resultList = FilterResultList(resultList);
            return resultList;
        }

        private List<FlowBlockOutDataset> FilterResultList(List<FlowBlockOutDataset> resultList)
        {
            List<FlowBlockOutDataset> filtered = new List<FlowBlockOutDataset>(resultList);
            foreach (var flowBlockoutDataset in resultList)
            {
                var allPrecedingAndCurrentFieldValues = flowBlockoutDataset.FieldValueMappings
                    .Select(x => x.GetPrecedingAndCurrentFieldValues());

                foreach (Dictionary<FieldElement, string> precedingAndCurrentFieldValues in allPrecedingAndCurrentFieldValues)
                {
                    foreach (KeyValuePair<FieldElement, string> fieldAndValue in precedingAndCurrentFieldValues)
                    {
                        FieldElement fieldElement = fieldAndValue.Key;
                        string fieldValue = fieldAndValue.Value;

                        // If at least one field exists in other preceding fields and the values ​​differ, then the strands are not compatible.
                        if (allPrecedingAndCurrentFieldValues
                            .Except([precedingAndCurrentFieldValues])
                            .Any(x => x.ContainsKey(fieldElement) && x[fieldElement] != fieldValue))
                        {
                            filtered.Remove(flowBlockoutDataset);
                        }
                    }
                }
            }
            return filtered;
        }

        private void GetCombinations(
            List<List<FlowBlockOutDataset>> listOfLists, 
            int depth,
            List<FlowBlockOutDatasetFieldValueMapping> current,
            List<FlowBlockOutDataset> result)
        {
            if (depth == listOfLists.Count)
            {
                result.Add(new FlowBlockOutDataset 
                { 
                    FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>(current) 
                });

                return;
            }

            if (IsInputBehavior(listOfLists[depth], InputBehavior.First))
                Recurse(listOfLists[depth].First(), listOfLists, depth, current, result);
            else if (IsInputBehavior(listOfLists[depth], InputBehavior.Last))
                Recurse(listOfLists[depth].Last(), listOfLists, depth, current, result);
            if (IsInputBehavior(listOfLists[depth], InputBehavior.FirstValid))
            {
                var validDatasets = listOfLists[depth].Where(x => IsValid(x));
                Recurse(validDatasets.Any() ? 
                    validDatasets.First() : 
                    listOfLists[depth].First(), listOfLists, depth, current, result);
            }
            else if (IsInputBehavior(listOfLists[depth], InputBehavior.LastValid))
            {
                var validDatasets = listOfLists[depth].Where(x => IsValid(x));
                Recurse(validDatasets.Any() ? 
                    validDatasets.Last() : 
                    listOfLists[depth].First(), listOfLists, depth, current, result);
            }
            else if (IsInputBehavior(listOfLists[depth], InputBehavior.Cross))
            {
                foreach (var item in listOfLists[depth])
                {
                    Recurse(item, listOfLists, depth, current, result);
                }
            }
            else if (IsInputBehavior(listOfLists[depth], InputBehavior.CrossValid))
            {
                foreach (var item in listOfLists[depth].Where(x => IsValid(x)))
                {
                    Recurse(item, listOfLists, depth, current, result);
                }
            }
        }

        private void Recurse(
            FlowBlockOutDataset item,
            List<List<FlowBlockOutDataset>> listOfLists,
            int depth,
            List<FlowBlockOutDatasetFieldValueMapping> current,
            List<FlowBlockOutDataset> result)
        {
            current.AddRange(item.FieldValueMappings);
            GetCombinations(listOfLists, depth + 1, current, result);
            current.RemoveRange(current.Count - item.FieldValueMappings.Count, item.FieldValueMappings.Count);
        }

        private bool IsInputBehavior(List<FlowBlockOutDataset> items, InputBehavior inputBehavior)
        {
            if (items.Count == 0)
                throw new InvalidOperationException("At least one item must be in list of result datasets.");

            var firstFieldValueMappings = items.First().FieldValueMappings;
            if (firstFieldValueMappings.Count == 0)
                throw new InvalidOperationException("At least one field value mapping must be in result item.");

            var field = firstFieldValueMappings.First().Field;
            var underlyingFlowBlock = field.Source;

            return GetInputBehaviorAssignment(underlyingFlowBlock).Behavior == inputBehavior;
        }
    }
}

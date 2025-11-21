using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Components;

namespace FlowBlox.Core.Models.FlowBlocks.Base.DatasetSelection
{
    /// <summary>
    /// Selects result data sets row by row (RowWise) from multiple FlowBlockOut collections.
    /// Supported InputBehaviors:
    /// <list type="bullet">
    /// <item>RowWise, RowWiseValid</item>
    /// <item>First, FirstValid, Last, LastValid</item>
    /// </list>
    /// Cross / CrossValid will cause an exception.
    /// <para>RowWise:
    /// Row i takes dataset i from all RowWise inputs. First/Last inputs are broadcast across all rows.</para>
    /// <para>RowWiseValid:
    /// Like RowWise, but after combining, all invalid rows are where the combined dataset is invalid according to IsValid().</para>
    /// </summary>
    public class FlowBlockRowWiseDatasetSelector : FlowBlockDatasetSelectorBase
    {
        public FlowBlockRowWiseDatasetSelector(
            Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>> passedResults,
            IList<InputBehaviorAssignment> inputBehaviorAssignments) : base(passedResults, inputBehaviorAssignments)
        {

        }

        private class InputColumn
        {
            public BaseResultFlowBlock FlowBlock { get; init; }
            public InputBehavior Behavior { get; init; }
            public List<FlowBlockOutDataset> Datasets { get; init; }
        }

        public override List<FlowBlockOutDataset> GetResults()
        {
            // Complete list, order remains the same
            var columns = new List<InputColumn>();

            foreach (var resultSet in passedResults.Values)
            {
                var allDatasets = resultSet
                    .SelectMany(fbo => fbo.Results ?? Enumerable.Empty<FlowBlockOutDataset>())
                    .ToList();

                if (allDatasets.Count == 0)
                    throw new InvalidOperationException("At least one result dataset must exist per input flow block.");

                var firstMappings = allDatasets.First().FieldValueMappings;
                if (firstMappings == null || firstMappings.Count == 0)
                    throw new InvalidOperationException("At least one field value mapping must exist in each result dataset.");

                var field = firstMappings.First().Field;
                var underlyingFlowBlock = field.Source;

                var behavior = GetInputBehaviorAssignment(underlyingFlowBlock).Behavior;
                if (behavior == InputBehavior.Cross || behavior == InputBehavior.CrossValid)
                {
                    throw new InvalidOperationException(
                        "RowWise selector does not support Cross / CrossValid behaviors. " +
                        "Combining (Cross) and RowWise together is invalid.");
                }

                var effectiveDatasets = GetEffectiveDatasetsForBehavior(allDatasets, behavior);

                columns.Add(new InputColumn
                {
                    FlowBlock = underlyingFlowBlock,
                    Behavior = behavior,
                    Datasets = effectiveDatasets
                });
            }

            // Determine matrix size: smallest result list below the RowWise/RowWiseValid columns
            var rowWiseColumns = columns
                .Where(c => c.Behavior == InputBehavior.RowWise || c.Behavior == InputBehavior.RowWiseValid)
                .ToList();

            int rowCount;
            if (rowWiseColumns.Any())
            {
                rowCount = rowWiseColumns.Min(c => c.Datasets.Count);
            }
            else
            {
                // No RowWise column, only First/Last(/Valid); exactly one row
                rowCount = 1;
            }

            if (rowCount == 0)
                return new List<FlowBlockOutDataset>();

            // Combine line by line
            var resultList = new List<FlowBlockOutDataset>(rowCount);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var combinedMappings = new List<FlowBlockOutDatasetFieldValueMapping>();

                foreach (var column in columns)
                {
                    FlowBlockOutDataset sourceDataset;

                    if (column.Behavior == InputBehavior.RowWise ||
                        column.Behavior == InputBehavior.RowWiseValid)
                    {
                        sourceDataset = column.Datasets[rowIndex];
                    }
                    else
                    {
                        // First / Last / FirstValid / LastValid
                        sourceDataset = column.Datasets[0];
                    }

                    if (sourceDataset?.FieldValueMappings != null)
                    {
                        combinedMappings.AddRange(sourceDataset.FieldValueMappings);
                    }
                }

                var combinedDataset = new FlowBlockOutDataset
                {
                    FieldValueMappings = combinedMappings
                };

                resultList.Add(combinedDataset);
            }

            // RowWiseValid: remove entire rows if the combined dataset is invalid
            bool applyRowWiseValidFilter = columns.Any(c => c.Behavior == InputBehavior.RowWiseValid);
            if (applyRowWiseValidFilter)
            {
                resultList = resultList.Where(IsValid).ToList();
            }

            return resultList;
        }

        private List<FlowBlockOutDataset> GetEffectiveDatasetsForBehavior(
            List<FlowBlockOutDataset> allDatasets,
            InputBehavior behavior)
        {
            switch (behavior)
            {
                case InputBehavior.First:
                    return new List<FlowBlockOutDataset> { allDatasets.First() };

                case InputBehavior.Last:
                    return new List<FlowBlockOutDataset> { allDatasets.Last() };

                case InputBehavior.FirstValid:
                    {
                        var firstValid = allDatasets.FirstOrDefault(IsValid);
                        return new List<FlowBlockOutDataset> { firstValid ?? allDatasets.First() };
                    }

                case InputBehavior.LastValid:
                    {
                        var lastValid = allDatasets.LastOrDefault(IsValid);
                        return new List<FlowBlockOutDataset> { lastValid ?? allDatasets.First() };
                    }

                case InputBehavior.RowWise:
                case InputBehavior.RowWiseValid:
                    // Complete list, order remains the same
                    return allDatasets;

                default:
                    throw new InvalidOperationException($"Unsupported input behavior '{behavior}' for row-wise selector.");
            }
        }
    }
}

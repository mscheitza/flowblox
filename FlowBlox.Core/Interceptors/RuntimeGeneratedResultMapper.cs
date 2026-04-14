using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Runtime.Debugging;

namespace FlowBlox.Core.Interceptors
{
    internal static class RuntimeGeneratedResultMapper
    {
        public static List<RuntimeGeneratedResultDataset> MapDatasets(IReadOnlyList<FlowBlockOutDataset> datasets)
        {
            if (datasets == null || datasets.Count == 0)
                return new List<RuntimeGeneratedResultDataset>();

            var serializedDatasets = new List<RuntimeGeneratedResultDataset>(datasets.Count);
            for (var datasetIndex = 0; datasetIndex < datasets.Count; datasetIndex++)
            {
                var dataset = datasets[datasetIndex];
                var serializedDataset = new RuntimeGeneratedResultDataset
                {
                    DatasetIndex = datasetIndex
                };

                if (dataset?.FieldValueMappings != null)
                {
                    foreach (var mapping in dataset.FieldValueMappings)
                    {
                        serializedDataset.FieldValueMappings.Add(new RuntimeGeneratedResultFieldValueMapping
                        {
                            FieldFullyQualifiedName = mapping?.Field?.FullyQualifiedName ?? string.Empty,
                            Value = mapping?.Value
                        });
                    }
                }

                serializedDatasets.Add(serializedDataset);
            }

            return serializedDatasets;
        }
    }
}

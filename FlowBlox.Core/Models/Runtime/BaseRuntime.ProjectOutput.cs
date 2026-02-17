using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.Runtime
{
    public sealed class FlowBloxProjectOutputDataset
    {
        public FlowBloxProjectOutputDataset()
        {
            this.Values = new Dictionary<string, object>();
        }

        public string OutputName { get; set; }
        public DateTime CreatedUtc { get; set; }
        public Dictionary<string, object> Values { get; set; }
    }

    partial class BaseRuntime
    {
        private readonly object _outputLock = new object();

        // OutputName -> datasets
        private readonly Dictionary<string, List<FlowBloxProjectOutputDataset>> _outputDatasets = new Dictionary<string, List<FlowBloxProjectOutputDataset>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Appends an output dataset under the given output name.
        /// The output name is typically the FlowBlock.Name of the ProjectOutputFlowBlock.
        /// </summary>
        public void AppendOutputDataset(string outputName, FlowBloxProjectOutputDataset dataset)
        {
            if (string.IsNullOrWhiteSpace(outputName))
                throw new ArgumentException("outputName must not be empty.", nameof(outputName));

            if (dataset == null)
                throw new ArgumentNullException(nameof(dataset));

            lock (_outputLock)
            {
                if (!_outputDatasets.TryGetValue(outputName, out var list))
                {
                    list = new List<FlowBloxProjectOutputDataset>();
                    _outputDatasets.Add(outputName, list);
                }

                list.Add(dataset);
            }
        }

        /// <summary>
        /// Returns a snapshot copy of all output datasets for the given output name.
        /// </summary>
        public IReadOnlyList<FlowBloxProjectOutputDataset> GetOutputDatasets(string outputName)
        {
            if (string.IsNullOrWhiteSpace(outputName))
                return Array.Empty<FlowBloxProjectOutputDataset>();

            lock (_outputLock)
            {
                if (_outputDatasets.TryGetValue(outputName, out var list))
                    return list.ToList();

                return Array.Empty<FlowBloxProjectOutputDataset>();
            }
        }

        /// <summary>
        /// Returns a snapshot copy of all outputs (OutputName -> datasets).
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<FlowBloxProjectOutputDataset>> GetAllOutputs()
        {
            lock (_outputLock)
            {
                return _outputDatasets.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<FlowBloxProjectOutputDataset>)kvp.Value.ToList(),
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Serializes all collected project outputs to JSON.
        /// Values are stored as plain objects (no FieldElements).
        /// </summary>
        public string SerializeAllOutputsToJson(bool pretty = false)
        {
            var outputs = GetAllOutputs();

            var dto = new
            {
                Project?.ProjectName,
                StartedUtc = Started.ToUniversalTime(),
                Outputs = outputs.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(ds => new
                    {
                        ds.OutputName,
                        ds.CreatedUtc,
                        ds.Values
                    }).ToList(),
                    StringComparer.OrdinalIgnoreCase)
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = pretty
            };

            return JsonSerializer.Serialize(dto, jsonOptions);
        }
    }
}
using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetLastDebugArtefactHandler : ToolHandlerBase
    {
        public override string Name => "GetLastDebugArtefact";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns debug artefacts from the last debug run: field changes and generated result datasets.",
            new JObject
            {
                ["fieldChangeId"] = "int? (use this for FieldChange artefact retrieval)",
                ["generatedResultId"] = "int? (use this for GeneratedResult artefact retrieval)",
                ["datasetSelectionMode"] = "string? (First|Last|Index, only relevant for generatedResultId; default: First)",
                ["datasetIndex"] = "int? (0-based, required when datasetSelectionMode=Index)",
                ["runId"] = "string? (optional safety check)",
                ["usageHint"] =
                    "Use fieldChangeId only for field-change artefacts. " +
                    "Use generatedResultId for generated-result artefacts and optionally select a dataset via datasetSelectionMode (First|Last|Index) and datasetIndex (0-based for Index)."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var snapshot = AiAssistantDebugRunState.Get();
            if (snapshot == null)
                return Task.FromResult(ToolHandlerUtilities.Fail("No debug run available yet. Run 'RunProjectDebugTest' first."));

            var requestedRunId = (args.Value<string>("runId") ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(requestedRunId)
                && !string.Equals(requestedRunId, snapshot.RunId, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    $"Requested runId '{requestedRunId}' does not match last run '{snapshot.RunId}'."));
            }

            var fieldChangeId = args.Value<int?>("fieldChangeId");
            var generatedResultId = args.Value<int?>("generatedResultId");
            var hasFieldChangeId = fieldChangeId.HasValue && fieldChangeId.Value > 0;
            var hasGeneratedResultId = generatedResultId.HasValue && generatedResultId.Value > 0;

            if (!hasFieldChangeId && !hasGeneratedResultId)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    "Provide either fieldChangeId or generatedResultId."));
            }

            if (hasFieldChangeId && hasGeneratedResultId)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    "Provide only one artefact identifier: fieldChangeId or generatedResultId."));
            }

            if (hasFieldChangeId)
            {
                var fieldChanges = GetArrayIgnoreCase(snapshot.DebuggingResult, "FieldValueChanges", "fieldValueChanges");
                var fieldChange = fieldChanges
                    .OfType<JObject>()
                    .FirstOrDefault(x => GetIntIgnoreCase(x, "Id", "id") == fieldChangeId.Value);

                if (fieldChange == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail($"FieldChangeId '{fieldChangeId.Value}' not found in last debug run."));

                return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                {
                    ["runId"] = snapshot.RunId,
                    ["createdUtc"] = snapshot.CreatedUtc,
                    ["artefactType"] = "FieldChange",
                    ["fieldChange"] = fieldChange
                }));
            }

            var generatedResults = GetArrayIgnoreCase(snapshot.DebuggingResult, "GeneratedResults", "generatedResults");
            var generatedResult = generatedResults
                .OfType<JObject>()
                .FirstOrDefault(x => GetIntIgnoreCase(x, "Id", "id") == generatedResultId.Value);

            if (generatedResult == null)
                return Task.FromResult(ToolHandlerUtilities.Fail($"GeneratedResultId '{generatedResultId.Value}' not found in last debug run."));

            var datasets = GetArrayIgnoreCase(generatedResult, "Datasets", "datasets");
            var selectionMode = (args.Value<string>("datasetSelectionMode") ?? "First").Trim();
            var selectedDatasetInfo = SelectDataset(datasets, selectionMode, args.Value<int?>("datasetIndex"));
            if (!selectedDatasetInfo.Ok)
                return Task.FromResult(ToolHandlerUtilities.Fail(selectedDatasetInfo.ErrorMessage));

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["runId"] = snapshot.RunId,
                ["createdUtc"] = snapshot.CreatedUtc,
                ["artefactType"] = "GeneratedResult",
                ["generatedResult"] = generatedResult,
                ["selectedDatasetIndex"] = selectedDatasetInfo.SelectedIndex,
                ["selectedDataset"] = selectedDatasetInfo.SelectedDataset ?? JValue.CreateNull()
            }));
        }

        private static (bool Ok, string ErrorMessage, int SelectedIndex, JToken SelectedDataset) SelectDataset(
            JArray datasets,
            string selectionModeRaw,
            int? datasetIndex)
        {
            var selectionMode = string.IsNullOrWhiteSpace(selectionModeRaw)
                ? "First"
                : selectionModeRaw.Trim();

            if (datasets == null || datasets.Count == 0)
            {
                return (true, string.Empty, -1, JValue.CreateNull());
            }

            if (string.Equals(selectionMode, "First", StringComparison.OrdinalIgnoreCase))
            {
                return (true, string.Empty, 0, datasets[0]);
            }

            if (string.Equals(selectionMode, "Last", StringComparison.OrdinalIgnoreCase))
            {
                var lastIndex = datasets.Count - 1;
                return (true, string.Empty, lastIndex, datasets[lastIndex]);
            }

            if (string.Equals(selectionMode, "Index", StringComparison.OrdinalIgnoreCase))
            {
                if (!datasetIndex.HasValue || datasetIndex.Value < 0)
                {
                    return (false, "datasetIndex must be provided and >= 0 when datasetSelectionMode is 'Index'.", -1, JValue.CreateNull());
                }

                if (datasetIndex.Value >= datasets.Count)
                {
                    return (false, $"datasetIndex {datasetIndex.Value} is out of range. Available range: 0..{datasets.Count - 1}.", -1, JValue.CreateNull());
                }

                return (true, string.Empty, datasetIndex.Value, datasets[datasetIndex.Value]);
            }

            return (false, "datasetSelectionMode must be one of: First, Last, Index.", -1, JValue.CreateNull());
        }

        private static JArray GetArrayIgnoreCase(JObject root, params string[] keys)
        {
            if (root == null)
                return new JArray();

            foreach (var key in keys)
            {
                if (root[key] is JArray direct)
                    return direct;
            }

            var prop = root.Properties()
                .FirstOrDefault(x => keys.Any(k => string.Equals(k, x.Name, StringComparison.OrdinalIgnoreCase)));

            return prop?.Value as JArray ?? new JArray();
        }

        private static int? GetIntIgnoreCase(JObject root, params string[] keys)
        {
            foreach (var key in keys)
            {
                var value = root.Value<int?>(key);
                if (value.HasValue)
                    return value;
            }

            var prop = root.Properties()
                .FirstOrDefault(x => keys.Any(k => string.Equals(k, x.Name, StringComparison.OrdinalIgnoreCase)));

            return prop?.Value?.Value<int?>();
        }
    }
}

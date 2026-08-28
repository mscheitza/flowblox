using FlowBlox.AIAssistant.Helper;
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
                ["datasetIndex"] = "int? (0-based; when set, only this generated-result dataset is returned)",
                ["fieldValueStartIndex"] = "int? (default: 0; character offset used when continuing a truncated field value)",
                ["fieldValueSearchValues"] = "string? (comma-separated; when provided, each field value starts at the first matching search value instead of fieldValueStartIndex)",
                ["runId"] = "string? (optional safety check)",
                ["usageHint"] =
                    "Use fieldChangeId only for field-change artefacts. " +
                    "Use generatedResultId for generated-result artefacts. " + 
                    "Use datasetIndex plus fieldValueStartIndex or fieldValueSearchValues to inspect large values in focused chunks."
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
            var fieldValueStartIndex = Math.Max(0, args.Value<int?>("fieldValueStartIndex") ?? 0);
            var fieldValueSearchValues = args.Value<string>("fieldValueSearchValues");
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
                var requestedFieldChangeId = fieldChangeId.GetValueOrDefault();
                var fieldChange = fieldChanges
                    .OfType<JObject>()
                    .FirstOrDefault(x => GetIntIgnoreCase(x, "Id", "id") == requestedFieldChangeId);

                if (fieldChange == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail($"FieldChangeId '{requestedFieldChangeId}' not found in last debug run."));

                var outputLimiter = FieldValueResponseLimiter.FromConfiguration();
                var limitedFieldChange = LimitFieldChange(fieldChange, outputLimiter, fieldValueStartIndex, fieldValueSearchValues);
                return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                {
                    ["runId"] = snapshot.RunId,
                    ["createdUtc"] = snapshot.CreatedUtc,
                    ["artefactType"] = "FieldChange",
                    ["fieldChange"] = limitedFieldChange,
                    ["fieldValueOutput"] = outputLimiter.CreateMetadata()
                }));
            }

            var generatedResults = GetArrayIgnoreCase(snapshot.DebuggingResult, "GeneratedResults", "generatedResults");
            var requestedGeneratedResultId = generatedResultId.GetValueOrDefault();
            var generatedResult = generatedResults
                .OfType<JObject>()
                .FirstOrDefault(x => GetIntIgnoreCase(x, "Id", "id") == requestedGeneratedResultId);

            if (generatedResult == null)
                return Task.FromResult(ToolHandlerUtilities.Fail($"GeneratedResultId '{requestedGeneratedResultId}' not found in last debug run."));

            var datasets = GetArrayIgnoreCase(generatedResult, "Datasets", "datasets");
            var selectionMode = (args.Value<string>("datasetSelectionMode") ?? "First").Trim();
            var datasetIndex = args.Value<int?>("datasetIndex");
            var selectedDatasetInfo = SelectDataset(datasets, selectionMode, datasetIndex);
            if (!selectedDatasetInfo.Ok)
                return Task.FromResult(ToolHandlerUtilities.Fail(selectedDatasetInfo.ErrorMessage));

            var requestedDatasetIndex = ValidateDatasetIndex(datasetIndex, datasets.Count);
            if (requestedDatasetIndex.ErrorMessage.Length > 0)
                return Task.FromResult(ToolHandlerUtilities.Fail(requestedDatasetIndex.ErrorMessage));

            var generatedResultOutputLimiter = FieldValueResponseLimiter.FromConfiguration();
            var limitedGeneratedResult = LimitGeneratedResult(
                generatedResult,
                requestedDatasetIndex.Index,
                generatedResultOutputLimiter,
                fieldValueStartIndex,
                fieldValueSearchValues);

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["runId"] = snapshot.RunId,
                ["createdUtc"] = snapshot.CreatedUtc,
                ["artefactType"] = "GeneratedResult",
                ["generatedResult"] = limitedGeneratedResult,
                ["selectedDatasetIndex"] = selectedDatasetInfo.SelectedIndex,
                ["includedDatasetIndex"] = requestedDatasetIndex.Index == null
                    ? JValue.CreateNull()
                    : requestedDatasetIndex.Index,
                ["fieldValueOutput"] = generatedResultOutputLimiter.CreateMetadata()
            }));
        }

        private static JObject LimitFieldChange(
            JObject fieldChange,
            FieldValueResponseLimiter limiter,
            int fieldValueStartIndex,
            string? fieldValueSearchValues)
        {
            var result = (JObject)fieldChange.DeepClone();
            LimitStringProperty(result, "OldValue", "oldValue", "oldValueInfo", limiter, fieldValueStartIndex, fieldValueSearchValues);
            LimitStringProperty(result, "NewValue", "newValue", "newValueInfo", limiter, fieldValueStartIndex, fieldValueSearchValues);
            return result;
        }

        private static JObject LimitGeneratedResult(
            JObject generatedResult,
            int? datasetIndex,
            FieldValueResponseLimiter limiter,
            int fieldValueStartIndex,
            string? fieldValueSearchValues)
        {
            var result = (JObject)generatedResult.DeepClone();
            var datasets = GetArrayIgnoreCase(result, "Datasets", "datasets");
            if (datasetIndex.HasValue)
            {
                var filteredDatasets = new JArray(datasets
                    .OfType<JObject>()
                    .Where(x => (GetIntIgnoreCase(x, "DatasetIndex", "datasetIndex") ?? -1) == datasetIndex.Value));
                SetArrayIgnoreCase(result, filteredDatasets, "Datasets", "datasets");
                datasets = filteredDatasets;
            }

            foreach (var dataset in datasets.OfType<JObject>())
            {
                var mappings = GetArrayIgnoreCase(dataset, "FieldValueMappings", "fieldValueMappings");
                foreach (var mapping in mappings.OfType<JObject>())
                    LimitStringProperty(mapping, "Value", "value", "valueInfo", limiter, fieldValueStartIndex, fieldValueSearchValues);
            }

            return result;
        }

        private static void LimitStringProperty(
            JObject obj,
            string pascalName,
            string camelName,
            string infoName,
            FieldValueResponseLimiter limiter,
            int fieldValueStartIndex,
            string? fieldValueSearchValues)
        {
            var property = GetPropertyIgnoreCase(obj, pascalName, camelName);
            if (property == null)
                return;

            var limited = limiter.Limit(property.Value.Value<string>() ?? string.Empty, fieldValueStartIndex, fieldValueSearchValues);
            property.Value = limited.Value;
            obj[infoName] = limited.ToMetadata();
        }

        private static DatasetSelectionResult SelectDataset(
            JArray datasets,
            string selectionModeRaw,
            int? datasetIndex)
        {
            var selectionMode = string.IsNullOrWhiteSpace(selectionModeRaw)
                ? "First"
                : selectionModeRaw.Trim();

            if (datasets == null || datasets.Count == 0)
            {
                return DatasetSelectionResult.Success(-1);
            }

            if (string.Equals(selectionMode, "First", StringComparison.OrdinalIgnoreCase))
            {
                return DatasetSelectionResult.Success(0);
            }

            if (string.Equals(selectionMode, "Last", StringComparison.OrdinalIgnoreCase))
            {
                var lastIndex = datasets.Count - 1;
                return DatasetSelectionResult.Success(lastIndex);
            }

            if (string.Equals(selectionMode, "Index", StringComparison.OrdinalIgnoreCase))
            {
                if (!datasetIndex.HasValue || datasetIndex.Value < 0)
                {
                    return DatasetSelectionResult.Fail("datasetIndex must be provided and >= 0 when datasetSelectionMode is 'Index'.");
                }

                if (datasetIndex.Value >= datasets.Count)
                {
                    return DatasetSelectionResult.Fail($"datasetIndex {datasetIndex.Value} is out of range. Available range: 0..{datasets.Count - 1}.");
                }

                return DatasetSelectionResult.Success(datasetIndex.Value);
            }

            return DatasetSelectionResult.Fail("datasetSelectionMode must be one of: First, Last, Index.");
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

        private static void SetArrayIgnoreCase(JObject root, JArray value, params string[] keys)
        {
            var property = GetPropertyIgnoreCase(root, keys);
            if (property != null)
            {
                property.Value = value;
                return;
            }

            root[keys.First()] = value;
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

        private static JProperty? GetPropertyIgnoreCase(JObject root, params string[] keys)
        {
            if (root == null)
                return null;

            foreach (var key in keys)
            {
                var property = root.Property(key, StringComparison.OrdinalIgnoreCase);
                if (property != null)
                    return property;
            }

            return null;
        }

        private static DatasetIndexValidationResult ValidateDatasetIndex(int? datasetIndex, int datasetCount)
        {
            if (!datasetIndex.HasValue)
                return DatasetIndexValidationResult.Success(null);

            if (datasetIndex.Value < 0 || datasetIndex.Value >= datasetCount)
                return DatasetIndexValidationResult.Fail($"datasetIndex {datasetIndex.Value} is out of range. Available range: 0..{Math.Max(0, datasetCount - 1)}.");

            return DatasetIndexValidationResult.Success(datasetIndex.Value);
        }

        private sealed record DatasetSelectionResult(bool Ok, string ErrorMessage, int SelectedIndex)
        {
            public static DatasetSelectionResult Success(int selectedIndex) => new(true, string.Empty, selectedIndex);
            public static DatasetSelectionResult Fail(string errorMessage) => new(false, errorMessage, -1);
        }

        private sealed record DatasetIndexValidationResult(int? Index, string ErrorMessage)
        {
            public static DatasetIndexValidationResult Success(int? index) => new(index, string.Empty);
            public static DatasetIndexValidationResult Fail(string errorMessage) => new(null, errorMessage);
        }
    }
}
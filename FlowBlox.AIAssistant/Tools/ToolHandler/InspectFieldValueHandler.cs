using FlowBlox.AIAssistant.Helper;
using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class InspectFieldValueHandler : ToolHandlerBase
    {
        public override string Name => "InspectFieldValue";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Inspects one large field-value string from the last debug run with focused navigation.",
            new JObject
            {
                ["fieldChangeId"] = "int? (use this for FieldChange value inspection)",
                ["fieldChangeValue"] = "string? (Old|New; default: New, only relevant for fieldChangeId)",
                ["generatedResultId"] = "int? (use this for GeneratedResult dataset field-value inspection)",
                ["datasetIndex"] = "int? (0-based; required for generatedResultId)",
                ["fieldName"] = "string? (field name to inspect, only relevant for generatedResultId)",
                ["fullyQualifiedFieldName"] = "string? (preferred field identifier, only relevant for generatedResultId)",
                ["searchIndex"] = "int? (character index; default: 0)",
                ["searchValues"] = "string? (comma-separated marker values; earliest match is used instead of searchIndex)",
                ["searchMode"] = "string? (StartAt|LookAround; default: StartAt)",
                ["runId"] = "string? (optional safety check)",
                ["usageHint"] = "Use this after GetLastDebugArtefact reports truncated or omitted field values. It is intended for navigation in oversized FieldValue strings, not for normal artefact listing."
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
                return Task.FromResult(ToolHandlerUtilities.Fail("Provide either fieldChangeId or generatedResultId."));

            if (hasFieldChangeId && hasGeneratedResultId)
                return Task.FromResult(ToolHandlerUtilities.Fail("Provide only one artefact identifier: fieldChangeId or generatedResultId."));

            var searchMode = (args.Value<string>("searchMode") ?? "StartAt").Trim();
            if (!IsValidSearchMode(searchMode))
                return Task.FromResult(ToolHandlerUtilities.Fail("searchMode must be one of: StartAt, LookAround."));

            var options = new FieldValueInspectionOptions
            {
                SearchIndex = args.Value<int?>("searchIndex"),
                SearchValues = args.Value<string>("searchValues"),
                SearchMode = searchMode
            };

            var resolvedValue = hasFieldChangeId
                ? ResolveFieldChangeValue(snapshot.DebuggingResult, fieldChangeId.GetValueOrDefault(), args.Value<string>("fieldChangeValue"))
                : ResolveGeneratedResultValue(
                    snapshot.DebuggingResult,
                    generatedResultId.GetValueOrDefault(),
                    args.Value<int?>("datasetIndex"),
                    args.Value<string>("fieldName"),
                    args.Value<string>("fullyQualifiedFieldName"));

            if (!resolvedValue.Ok)
                return Task.FromResult(ToolHandlerUtilities.Fail(resolvedValue.ErrorMessage));

            var limiter = FieldValueResponseLimiter.FromConfiguration();
            var inspected = limiter.Inspect(resolvedValue.Value, options);

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["runId"] = snapshot.RunId,
                ["createdUtc"] = snapshot.CreatedUtc,
                ["artefactType"] = resolvedValue.ArtefactType,
                ["fieldName"] = resolvedValue.FieldName ?? string.Empty,
                ["fullyQualifiedFieldName"] = resolvedValue.FullyQualifiedFieldName ?? string.Empty,
                ["datasetIndex"] = resolvedValue.DatasetIndex.HasValue
                    ? new JValue(resolvedValue.DatasetIndex.Value)
                    : JValue.CreateNull(),
                ["fieldChangeValue"] = resolvedValue.FieldChangeValue ?? string.Empty,
                ["value"] = inspected.Value,
                ["valueInfo"] = inspected.ToMetadata(),
                ["fieldValueOutput"] = limiter.CreateMetadata()
            }));
        }

        private static ResolvedFieldValue ResolveFieldChangeValue(
            JObject debugResult,
            int fieldChangeId,
            string? fieldChangeValue)
        {
            var fieldChanges = GetArrayIgnoreCase(debugResult, "FieldValueChanges", "fieldValueChanges");
            var fieldChange = fieldChanges
                .OfType<JObject>()
                .FirstOrDefault(x => GetIntIgnoreCase(x, "Id", "id") == fieldChangeId);

            if (fieldChange == null)
                return ResolvedFieldValue.Fail($"FieldChangeId '{fieldChangeId}' not found in last debug run.");

            var valueName = string.Equals(fieldChangeValue, "Old", StringComparison.OrdinalIgnoreCase)
                ? "Old"
                : "New";
            var valueProperty = valueName == "Old"
                ? GetPropertyIgnoreCase(fieldChange, "OldValue", "oldValue")
                : GetPropertyIgnoreCase(fieldChange, "NewValue", "newValue");

            return ResolvedFieldValue.Success(
                valueProperty?.Value?.Value<string>() ?? string.Empty,
                "FieldChange",
                null,
                GetStringIgnoreCase(fieldChange, "FieldName", "fieldName"),
                GetStringIgnoreCase(fieldChange, "FullyQualifiedFieldName", "fullyQualifiedFieldName"),
                valueName);
        }

        private static ResolvedFieldValue ResolveGeneratedResultValue(
            JObject debugResult,
            int generatedResultId,
            int? datasetIndex,
            string? fieldName,
            string? fullyQualifiedFieldName)
        {
            if (!datasetIndex.HasValue || datasetIndex.Value < 0)
                return ResolvedFieldValue.Fail("datasetIndex is required and must be >= 0 for generatedResultId inspection.");

            var generatedResults = GetArrayIgnoreCase(debugResult, "GeneratedResults", "generatedResults");
            var generatedResult = generatedResults
                .OfType<JObject>()
                .FirstOrDefault(x => GetIntIgnoreCase(x, "Id", "id") == generatedResultId);

            if (generatedResult == null)
                return ResolvedFieldValue.Fail($"GeneratedResultId '{generatedResultId}' not found in last debug run.");

            var datasets = GetArrayIgnoreCase(generatedResult, "Datasets", "datasets");
            var dataset = datasets
                .OfType<JObject>()
                .FirstOrDefault(x => (GetIntIgnoreCase(x, "DatasetIndex", "datasetIndex") ?? -1) == datasetIndex.Value)
                ?? (datasetIndex.Value < datasets.Count ? datasets[datasetIndex.Value] as JObject : null);

            if (dataset == null)
                return ResolvedFieldValue.Fail($"datasetIndex {datasetIndex.Value} is out of range. Available dataset count: {datasets.Count}.");

            var mappings = GetArrayIgnoreCase(dataset, "FieldValueMappings", "fieldValueMappings")
                .OfType<JObject>()
                .ToList();
            var mapping = ResolveFieldMapping(mappings, fieldName, fullyQualifiedFieldName);
            if (!mapping.Ok)
                return ResolvedFieldValue.Fail(mapping.ErrorMessage);

            return ResolvedFieldValue.Success(
                GetStringIgnoreCase(mapping.Value!, "Value", "value") ?? string.Empty,
                "GeneratedResult",
                datasetIndex.Value,
                GetStringIgnoreCase(mapping.Value!, "FieldName", "fieldName"),
                GetStringIgnoreCase(mapping.Value!, "FullyQualifiedFieldName", "fullyQualifiedFieldName"),
                null);
        }

        private static FieldMappingResolution ResolveFieldMapping(
            IReadOnlyList<JObject> mappings,
            string? fieldName,
            string? fullyQualifiedFieldName)
        {
            if (!string.IsNullOrWhiteSpace(fullyQualifiedFieldName))
            {
                var found = mappings.FirstOrDefault(x => string.Equals(
                    GetStringIgnoreCase(x, "FullyQualifiedFieldName", "fullyQualifiedFieldName"),
                    fullyQualifiedFieldName,
                    StringComparison.OrdinalIgnoreCase));
                return found == null
                    ? FieldMappingResolution.Fail($"fullyQualifiedFieldName '{fullyQualifiedFieldName}' not found in dataset.")
                    : FieldMappingResolution.Success(found);
            }

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                var matches = mappings
                    .Where(x => string.Equals(
                        GetStringIgnoreCase(x, "FieldName", "fieldName"),
                        fieldName,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (matches.Count == 1)
                    return FieldMappingResolution.Success(matches[0]);

                return FieldMappingResolution.Fail(matches.Count == 0
                    ? $"fieldName '{fieldName}' not found in dataset."
                    : $"fieldName '{fieldName}' is ambiguous. Use fullyQualifiedFieldName.");
            }

            if (mappings.Count == 1)
                return FieldMappingResolution.Success(mappings[0]);

            return FieldMappingResolution.Fail("Provide fieldName or fullyQualifiedFieldName when the dataset contains multiple field values.");
        }

        private static bool IsValidSearchMode(string searchMode)
            => string.Equals(searchMode, "StartAt", StringComparison.OrdinalIgnoreCase) || 
               string.Equals(searchMode, "LookAround", StringComparison.OrdinalIgnoreCase);

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

        private static string? GetStringIgnoreCase(JObject root, params string[] keys)
        {
            var property = GetPropertyIgnoreCase(root, keys);
            return property?.Value?.Value<string>();
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

        private sealed record ResolvedFieldValue(
            bool Ok,
            string ErrorMessage,
            string Value,
            string ArtefactType,
            int? DatasetIndex,
            string? FieldName,
            string? FullyQualifiedFieldName,
            string? FieldChangeValue)
        {
            public static ResolvedFieldValue Success(
                string value,
                string artefactType,
                int? datasetIndex,
                string? fieldName,
                string? fullyQualifiedFieldName,
                string? fieldChangeValue)
            {
                return new ResolvedFieldValue(
                    true,
                    string.Empty,
                    value,
                    artefactType,
                    datasetIndex,
                    fieldName,
                    fullyQualifiedFieldName,
                    fieldChangeValue);
            }

            public static ResolvedFieldValue Fail(string errorMessage)
            {
                return new ResolvedFieldValue(false, errorMessage, string.Empty, string.Empty, null, null, null, null);
            }
        }

        private sealed record FieldMappingResolution(bool Ok, string ErrorMessage, JObject? Value)
        {
            public static FieldMappingResolution Success(JObject value) => new(true, string.Empty, value);
            public static FieldMappingResolution Fail(string errorMessage) => new(false, errorMessage, null);
        }
    }
}
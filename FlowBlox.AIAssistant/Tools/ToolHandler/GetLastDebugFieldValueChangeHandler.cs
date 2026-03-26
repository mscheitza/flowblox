using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetLastDebugFieldValueChangeHandler : ToolHandlerBase
    {
        public override string Name => "GetLastDebugFieldValueChange";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns details for a field-value-change id from the last debug test run.",
            new JObject
            {
                ["fieldValueChangeId"] = "int",
                ["runId"] = "string? (optional safety check)"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var id = args.Value<int?>("fieldValueChangeId");
            if (!id.HasValue || id.Value <= 0)
                return Task.FromResult(ToolHandlerUtilities.Fail("fieldValueChangeId must be a positive integer."));

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

            var fieldChanges = GetArrayIgnoreCase(snapshot.DebuggingResult, "FieldValueChanges", "fieldValueChanges");
            if (fieldChanges == null || fieldChanges.Count == 0)
                return Task.FromResult(ToolHandlerUtilities.Fail("No field value changes recorded in last debug run."));

            var change = fieldChanges
                .OfType<JObject>()
                .FirstOrDefault(x => GetIntIgnoreCase(x, "Id", "id") == id.Value);

            if (change == null)
                return Task.FromResult(ToolHandlerUtilities.Fail($"FieldValueChangeId '{id.Value}' not found in last debug run."));

            var payload = new JObject
            {
                ["runId"] = snapshot.RunId,
                ["createdUtc"] = snapshot.CreatedUtc,
                ["fieldValueChange"] = change
            };

            return Task.FromResult(ToolHandlerUtilities.Ok(payload));
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

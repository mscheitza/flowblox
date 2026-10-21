using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler
{
    internal sealed class InspectExecutedTestFieldValueHandler : ToolHandlerBase
    {
        public override string Name => "InspectExecutedTestFieldValue";
        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(Name,
            "Searches or pages through a full assignment or result field value retained by RunTestDefinition or RunGenerationStrategies, without rerunning tests.",
            new JObject
            {
                ["runId"] = "string? (latest-run safety check)",
                ["executionId"] = "int? (required if run contains multiple executions; see GetLatestTestExecutionArtifact)",
                ["assignmentFQFieldName"] = "string? (assignment key; use either this or datasetIndex)",
                ["datasetIndex"] = "int? (0-based result row; pair with a field identifier)",
                ["fullyQualifiedFieldName"] = "string? (preferred result field identifier)",
                ["fieldName"] = "string? (result field name; must be unambiguous)",
                ["searchIndex"] = "int? (character offset; default: 0)",
                ["searchValues"] = "string? (comma-separated markers; earliest match overrides searchIndex)",
                ["searchMode"] = "string? (StartAt|LookAround; default: StartAt)",
                ["maxTokens"] = "int? (default/maximum: configured MaxFieldValuesTokensPerResponse, normally 4000; request the smallest useful scope)"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            if (!TestExecutionArtifactUtilities.TryResolve(args, out var snapshot, out var execution, out var error))
                return Task.FromResult(ToolHandlerUtilities.Fail(error));
            var assignment = args.Value<string>("assignmentFQFieldName");
            var index = args.Value<int?>("datasetIndex");
            var isAssignment = !string.IsNullOrWhiteSpace(assignment);
            if (isAssignment == index.HasValue || (isAssignment &&
                (!string.IsNullOrWhiteSpace(args.Value<string>("fieldName")) || !string.IsNullOrWhiteSpace(args.Value<string>("fullyQualifiedFieldName")))))
                return Task.FromResult(ToolHandlerUtilities.Fail("Provide either assignmentFQFieldName, or datasetIndex with a result field identifier."));

            string? value;
            string? fieldName = null;
            string? fqName = assignment;
            if (isAssignment)
            {
                var property = ((JObject)execution!["fieldValueAssignments"]!).Property(assignment!, StringComparison.OrdinalIgnoreCase);
                if (property == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail("assignmentFQFieldName was not found in this execution."));
                fqName = property.Name;
                value = property.Value.Value<string>();
            }
            else
            {
                var row = ((JArray)execution!["testResults"]!["rows"]!).OfType<JObject>()
                    .FirstOrDefault(x => x.Value<int>("datasetIndex") == index);
                if (row == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail("datasetIndex is out of range for this execution."));
                fqName = args.Value<string>("fullyQualifiedFieldName");
                fieldName = args.Value<string>("fieldName");
                var matches = ((JArray)row["values"]!).OfType<JObject>().Where(x =>
                    !string.IsNullOrWhiteSpace(fqName)
                        ? string.Equals(x.Value<string>("fullyQualifiedFieldName"), fqName, StringComparison.OrdinalIgnoreCase)
                        : !string.IsNullOrWhiteSpace(fieldName) && string.Equals(x.Value<string>("fieldName"), fieldName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matches.Count != 1)
                    return Task.FromResult(ToolHandlerUtilities.Fail("Result field missing or ambiguous. Provide a unique fullyQualifiedFieldName or fieldName."));
                value = matches[0].Value<string>("value");
                fieldName = matches[0].Value<string>("fieldName");
                fqName = matches[0].Value<string>("fullyQualifiedFieldName");
            }
            if (!FieldValueInspectionToolUtilities.TryCreateOptions(args, out var options, out error) ||
                !FieldValueResponseLimiter.TryFromConfiguration(args.Value<int?>("maxTokens"), out var limiter, out error))
                return Task.FromResult(ToolHandlerUtilities.Fail(error));
            var inspected = limiter!.Inspect(value, options);
            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["runId"] = snapshot!.RunId,
                ["createdUtc"] = snapshot.CreatedUtc,
                ["executionId"] = execution!["executionId"],
                ["artefactType"] = isAssignment ? "Assignment" : "TestResult",
                ["assignmentFQFieldName"] = isAssignment ? fqName : null,
                ["datasetIndex"] = index,
                ["fieldName"] = fieldName,
                ["fullyQualifiedFieldName"] = fqName,
                ["value"] = inspected.Value,
                ["valueInfo"] = inspected.ToMetadata(),
                ["fieldValueOutput"] = limiter.CreateMetadata()
            }));
        }
    }
}

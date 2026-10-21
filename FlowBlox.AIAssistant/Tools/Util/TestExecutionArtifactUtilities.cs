using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Testing;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static class TestExecutionArtifactUtilities
    {
        // Copy strings and identifiers while the runtime still owns the result datasets.
        public static JObject Capture(int executionId, string testDefinitionName, BaseFlowBlock target,
            FlowBloxTestResult result, string phase)
        {
            var rows = new JArray();
            if (target is BaseResultFlowBlock resultBlock)
            {
                var datasets = resultBlock.GridElementResult?.Results;
                if (datasets != null)
                    for (var i = 0; i < datasets.Count; i++)
                        rows.Add(new JObject
                        {
                            ["index"] = i,
                            ["datasetIndex"] = i,
                            ["values"] = new JArray(datasets[i].FieldValueMappings.Select(mapping => new JObject
                            {
                                ["fieldName"] = mapping.Field?.Name ?? string.Empty,
                                ["fullyQualifiedFieldName"] = mapping.Field?.FullyQualifiedName ?? string.Empty,
                                ["value"] = mapping.Value ?? string.Empty
                            }))
                        });
            }

            var testResults = new JObject
            {
                ["available"] = target is BaseResultFlowBlock,
                ["resultCount"] = rows.Count,
                ["columns"] = new JArray((rows.FirstOrDefault()?["values"] as JArray ?? new JArray())
                    .Select(x => x["fieldName"]!.DeepClone())),
                ["rows"] = rows
            };
            if (target is not BaseResultFlowBlock)
                testResults["reason"] = "Target flow block is not a BaseResultFlowBlock.";

            return new JObject
            {
                ["executionId"] = executionId,
                ["testDefinitionName"] = testDefinitionName,
                ["targetFlowBlockName"] = target.Name,
                ["phase"] = phase,
                ["success"] = result.Success,
                ["testResults"] = testResults,
                ["fieldValueAssignments"] = JObject.FromObject(result.FieldValueAssignments
                    .Where(x => !x.Key.StartsWith("$user::", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(x => x.Key, x => x.Value))
            };
        }

        public static JObject Limit(JObject execution, FieldValueResponseLimiter limiter, int? datasetIndex = null)
        {
            var copy = (JObject)execution.DeepClone();
            var rows = (JArray)copy["testResults"]!["rows"]!;
            if (datasetIndex.HasValue)
                copy["testResults"]!["rows"] = rows = new JArray(rows.OfType<JObject>()
                    .Where(x => x.Value<int>("datasetIndex") == datasetIndex.Value));
            foreach (var value in rows.SelectMany(x => (JArray)x["values"]!).OfType<JObject>())
            {
                var limited = limiter.Limit(value.Value<string>("value"));
                value["value"] = limited.Value;
                if (limited.ToMetadata().HasValues)
                    value["valueInfo"] = limited.ToMetadata();
            }
            var info = new JObject();
            foreach (var assignment in ((JObject)copy["fieldValueAssignments"]!).Properties())
            {
                var limited = limiter.Limit(assignment.Value.Value<string>());
                assignment.Value = limited.Value;
                if (limited.ToMetadata().HasValues)
                    info[assignment.Name] = limited.ToMetadata();
            }
            if (info.HasValues)
                copy["fieldValueAssignmentInfo"] = info;
            return copy;
        }

        public static bool TryResolve(JObject args, out AiAssistantTestExecutionSnapshot? snapshot,
            out JObject? execution, out string error)
        {
            snapshot = AiAssistantTestExecutionState.Get(ToolHandlerUtilities.CurrentSessionGuid, ToolHandlerUtilities.TryGetProject());
            execution = null;
            error = string.Empty;
            if (snapshot == null)
                error = "No test execution artifact available. Run RunTestDefinition or RunGenerationStrategies first.";
            else if (!string.IsNullOrWhiteSpace(args.Value<string>("runId")) &&
                !string.Equals(args.Value<string>("runId"), snapshot.RunId, StringComparison.OrdinalIgnoreCase))
                error = $"Requested runId does not match latest run '{snapshot.RunId}'.";
            else
            {
                var id = args.Value<int?>("executionId");
                if (!id.HasValue && snapshot.Executions.Count != 1)
                    error = "Provide executionId from testExecutionArtifact.executions or GetLatestTestExecutionArtifact.";
                else
                {
                    execution = snapshot.Executions.OfType<JObject>().FirstOrDefault(x => !id.HasValue || x.Value<int>("executionId") == id);
                    if (execution == null)
                        error = "executionId was not found in the latest run.";
                }
            }
            return error.Length == 0;
        }
    }
}

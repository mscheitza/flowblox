using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.State
{
    internal sealed class AiAssistantTestExecutionSnapshot
    {
        public string RunId { get; init; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
        public object? Project { get; init; }
        public string Source { get; init; } = string.Empty;
        public JArray Executions { get; init; } = new();

        public JObject Summary() => new()
        {
            ["runId"] = RunId,
            ["createdUtc"] = CreatedUtc,
            ["source"] = Source,
            ["executions"] = new JArray(Executions.OfType<JObject>().Select(x => new JObject
            {
                ["executionId"] = x["executionId"],
                ["testDefinitionName"] = x["testDefinitionName"],
                ["targetFlowBlockName"] = x["targetFlowBlockName"],
                ["phase"] = x["phase"],
                ["success"] = x["success"],
                ["resultCount"] = x["testResults"]?["resultCount"] ?? 0
            }))
        };
    }

    internal static class AiAssistantTestExecutionState
    {
        private static readonly ConcurrentDictionary<string, AiAssistantTestExecutionSnapshot> Snapshots = new();

        public static void Set(string sessionId, AiAssistantTestExecutionSnapshot snapshot) => Snapshots[sessionId] = Copy(snapshot);
        public static void Clear(string sessionId) => Snapshots.TryRemove(sessionId, out _);

        public static AiAssistantTestExecutionSnapshot? Get(string sessionId, object? project)
        {
            if (!Snapshots.TryGetValue(sessionId, out var snapshot) || !ReferenceEquals(snapshot.Project, project))
                return null;
            return Copy(snapshot);
        }

        private static AiAssistantTestExecutionSnapshot Copy(AiAssistantTestExecutionSnapshot snapshot) =>
            new AiAssistantTestExecutionSnapshot
            {
                RunId = snapshot.RunId,
                CreatedUtc = snapshot.CreatedUtc,
                Project = snapshot.Project,
                Source = snapshot.Source,
                Executions = (JArray)snapshot.Executions.DeepClone()
            };
    }
}

using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.State
{
    internal sealed class AiAssistantProjectRunSnapshot
    {
        public string RunId { get; init; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
        public string ProjectName { get; init; } = string.Empty;
        public JObject Outputs { get; init; } = new();
    }

    internal static class AiAssistantProjectRunState
    {
        private static readonly ConcurrentDictionary<string, AiAssistantProjectRunSnapshot> Snapshots =
            new(StringComparer.Ordinal);

        public static void Set(string sessionId, AiAssistantProjectRunSnapshot snapshot) =>
            Snapshots[sessionId] = snapshot;

        public static AiAssistantProjectRunSnapshot? Get(string sessionId)
        {
            if (!Snapshots.TryGetValue(sessionId, out var snapshot))
                return null;

            return new AiAssistantProjectRunSnapshot
            {
                RunId = snapshot.RunId,
                CreatedUtc = snapshot.CreatedUtc,
                ProjectName = snapshot.ProjectName,
                Outputs = (JObject)snapshot.Outputs.DeepClone()
            };
        }

        public static void Clear(string sessionId) => Snapshots.TryRemove(sessionId, out _);
    }
}

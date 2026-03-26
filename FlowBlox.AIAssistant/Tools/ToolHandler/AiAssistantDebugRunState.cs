using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class AiAssistantDebugRunSnapshot
    {
        public string RunId { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string DebuggingResultFilePath { get; set; } = string.Empty;
        public JObject DebuggingResult { get; set; } = new JObject();
    }

    internal static class AiAssistantDebugRunState
    {
        private static readonly object Sync = new();
        private static AiAssistantDebugRunSnapshot _lastSnapshot;

        public static void Set(AiAssistantDebugRunSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            lock (Sync)
            {
                _lastSnapshot = snapshot;
            }
        }

        public static AiAssistantDebugRunSnapshot Get()
        {
            lock (Sync)
            {
                if (_lastSnapshot == null)
                    return null;

                return new AiAssistantDebugRunSnapshot
                {
                    RunId = _lastSnapshot.RunId,
                    CreatedUtc = _lastSnapshot.CreatedUtc,
                    DebuggingResultFilePath = _lastSnapshot.DebuggingResultFilePath,
                    DebuggingResult = _lastSnapshot.DebuggingResult != null
                        ? (JObject)_lastSnapshot.DebuggingResult.DeepClone()
                        : new JObject()
                };
            }
        }
    }
}

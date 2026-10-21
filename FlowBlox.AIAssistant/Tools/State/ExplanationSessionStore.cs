using System.Collections.Concurrent;

namespace FlowBlox.AIAssistant.Tools.State
{
    internal static class ExplanationSessionStore
    {
        private static readonly ConcurrentDictionary<string, List<string>> LoadedKeysBySession =
            new(StringComparer.Ordinal);

        public static bool Load(string sessionId, string key)
        {
            var keys = LoadedKeysBySession.GetOrAdd(NormalizeSessionId(sessionId), _ => new List<string>());
            lock (keys)
            {
                if (keys.Contains(key, StringComparer.OrdinalIgnoreCase))
                    return false;

                keys.Add(key);
                return true;
            }
        }

        public static IReadOnlyList<string> GetLoadedKeys(string sessionId)
        {
            if (!LoadedKeysBySession.TryGetValue(NormalizeSessionId(sessionId), out var keys))
                return [];

            lock (keys)
                return keys.ToList();
        }

        public static void Restore(string sessionId, IEnumerable<string>? keys)
        {
            Clear(sessionId);
            foreach (var key in keys ?? [])
            {
                if (!string.IsNullOrWhiteSpace(key))
                    Load(sessionId, key.Trim());
            }
        }

        public static void Clear(string sessionId)
        {
            LoadedKeysBySession.TryRemove(NormalizeSessionId(sessionId), out _);
        }

        private static string NormalizeSessionId(string? sessionId) =>
            string.IsNullOrWhiteSpace(sessionId) ? "default-session" : sessionId.Trim();
    }
}

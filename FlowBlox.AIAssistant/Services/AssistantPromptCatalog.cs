using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace FlowBlox.AIAssistant.Services
{
    internal static class AssistantPromptCatalog
    {
        public const string SystemMessageKey = "system_message";
        public const string SessionBootstrapKey = "session_bootstrap";
        public const string IterationContextKey = "explaining_iteration_context";
        public const string EditAndDeleteKey = "explaining_edit_and_delete";

        private static readonly IReadOnlyDictionary<string, PromptEntryDefinition> Definitions =
            new Dictionary<string, PromptEntryDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                [SystemMessageKey] = new PromptEntryDefinition(
                    SystemMessageKey,
                    "System Message",
                    "FlowBlox.AIAssistant.Prompts.SystemMessage.txt",
                    "Core response contract and finalization rules."),
                [SessionBootstrapKey] = new PromptEntryDefinition(
                    SessionBootstrapKey,
                    "Session Bootstrap",
                    "FlowBlox.AIAssistant.Prompts.SessionBootstrap.txt",
                    "Editing rules, tool usage policy, and workflow guardrails."),
                [IterationContextKey] = new PromptEntryDefinition(
                    IterationContextKey,
                    "Explaining IterationContext",
                    "FlowBlox.AIAssistant.Prompts.ExplainingIterationContext.txt",
                    "Important for understanding FlowBlox flow execution logic."),
                [EditAndDeleteKey] = new PromptEntryDefinition(
                    EditAndDeleteKey,
                    "Explaining Edit and Delete",
                    "FlowBlox.AIAssistant.Prompts.ExplainingEditAndDelete.txt",
                    "How to update, link/unlink, and delete safely (including dependency order).")
            };

        private static readonly Lazy<IReadOnlyDictionary<string, PromptEntry>> Entries =
            new(LoadEntries, isThreadSafe: true);

        public static string? GetPromptContentOrNull(string key)
        {
            if (!Entries.Value.TryGetValue(key, out var entry))
                return null;

            return entry.Content;
        }

        public static IReadOnlyList<PromptEntry> GetAllEntries()
        {
            return Entries.Value.Values
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static bool TryGetEntry(string key, out PromptEntry? entry)
        {
            if (Entries.Value.TryGetValue(key, out var found))
            {
                entry = found;
                return true;
            }

            entry = null;
            return false;
        }

        private static IReadOnlyDictionary<string, PromptEntry> LoadEntries()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = new Dictionary<string, PromptEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in Definitions.Values)
            {
                var content = ReadEmbeddedTextOrNull(assembly, definition.ResourceName);
                if (string.IsNullOrWhiteSpace(content))
                    continue;

                var normalized = content.Replace("\r\n", "\n").Trim();
                if (normalized.Length == 0)
                    continue;

                result[definition.Key] = new PromptEntry(
                    definition.Key,
                    definition.Title,
                    definition.Hint,
                    normalized,
                    ComputeSha256Hex(normalized));
            }

            return result;
        }

        private static string? ReadEmbeddedTextOrNull(Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                return null;

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }

        private static string ComputeSha256Hex(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var hash = SHA256.HashData(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private sealed record PromptEntryDefinition(string Key, string Title, string ResourceName, string Hint);

        internal sealed record PromptEntry(string Key, string Title, string Hint, string Content, string ContentHash);
    }
}

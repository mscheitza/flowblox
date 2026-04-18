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
        public const string FlowBlocksManagingObjectKey = "explaining_flow_blocks_managing_an_object";
        public const string EditAndDeleteKey = "explaining_edit_and_delete";
        public const string NamingConventionsKey = "naming_conventions";
        public const string ExecutionRequirementsKey = "execution_requirements_and_required_fields";
        public const string InputFilesKey = "explaining_input_file_templates";
        public const string SpecialTestDefinitionsKey = "explaining_test_definitions";
        public const string SpecialTestDefinitionsDeepDiveKey = "explaining_test_definitions_deep_dive";
        public const string SpecialGeneratorsKey = "explaining_generators";
        public const string SpecialTestDrivenUnknownResourcesKey = "explaining_test_driven_unknown_resources";
        public const string SpecialModifiersAndFieldValidatorsKey = "explaining_modifiers_and_field_validators";
        public const string SpecialFlowRecursionKey = "explaining_flow_recursion";

        private static readonly IReadOnlyDictionary<string, PromptEntryDefinition> Definitions =
            new Dictionary<string, PromptEntryDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                [SystemMessageKey] = new PromptEntryDefinition(
                    SystemMessageKey,
                    "System Message",
                    "FlowBlox.AIAssistant.Prompts.SystemMessage.txt",
                    "Core response contract and finalization rules.",
                    true),
                [SessionBootstrapKey] = new PromptEntryDefinition(
                    SessionBootstrapKey,
                    "Session Bootstrap",
                    "FlowBlox.AIAssistant.Prompts.SessionBootstrap.txt",
                    "Editing rules, tool usage policy, and workflow guardrails.",
                    true),
                [IterationContextKey] = new PromptEntryDefinition(
                    IterationContextKey,
                    "Explaining IterationContext",
                    "FlowBlox.AIAssistant.Prompts.ExplainingIterationContext.txt",
                    "Important for understanding FlowBlox flow execution logic.",
                    true),
                [FlowBlocksManagingObjectKey] = new PromptEntryDefinition(
                    FlowBlocksManagingObjectKey,
                    "Explaining FlowBlocks Managing an Object",
                    "FlowBlox.AIAssistant.Prompts.ExplainingFlowBlocksManagingAnObject.txt",
                    "How object-managing flow blocks define object lifecycle per dataset or per run.",
                    true),
                [EditAndDeleteKey] = new PromptEntryDefinition(
                    EditAndDeleteKey,
                    "Explaining Edit and Delete",
                    "FlowBlox.AIAssistant.Prompts.ExplainingEditAndDelete.txt",
                    "How to update, link/unlink, and delete safely (including dependency order).",
                    true),
                [NamingConventionsKey] = new PromptEntryDefinition(
                    NamingConventionsKey,
                    "Naming Conventions",
                    "FlowBlox.AIAssistant.Prompts.NamingConventions.txt",
                    "Consistent naming rules for FlowBlocks and result-field descriptors.",
                    true),
                [ExecutionRequirementsKey] = new PromptEntryDefinition(
                    ExecutionRequirementsKey,
                    "Execution Requirements and Required Fields",
                    "FlowBlox.AIAssistant.Prompts.ExecutionRequirementsAndRequiredFields.txt",
                    "How to ensure downstream execution is guarded when upstream result datasets are empty.",
                    true),
                [InputFilesKey] = new PromptEntryDefinition(
                    InputFilesKey,
                    "Explaining Managed Input Files",
                    "FlowBlox.AIAssistant.Prompts.ExplainingInputFiles.txt",
                    "How managed input files are used for schemas, mock data, helper scripts and command execution.",
                    false),
                [SpecialTestDefinitionsKey] = new PromptEntryDefinition(
                    SpecialTestDefinitionsKey,
                    "Explaining Test Definitions",
                    "FlowBlox.AIAssistant.Prompts.ExplainingTestDefinitions.txt",
                    "Special: on-demand guidance for creating/linking/configuring test definitions via tool calls.",
                    false),
                [SpecialTestDefinitionsDeepDiveKey] = new PromptEntryDefinition(
                    SpecialTestDefinitionsDeepDiveKey,
                    "Explaining Test Definitions (Deep Dive)",
                    "FlowBlox.AIAssistant.Prompts.ExplainingTestDefinitionsDeepDive.txt",
                    "Special: on-demand deep-dive catalog with compact use cases, scope strategies, and advanced test-definition setup guidance.",
                    false),
                [SpecialGeneratorsKey] = new PromptEntryDefinition(
                    SpecialGeneratorsKey,
                    "Explaining Generators",
                    "FlowBlox.AIAssistant.Prompts.ExplainingGenerators.txt",
                    "Special: on-demand guidance for creating/configuring generation strategies from test results.",
                    false),
                [SpecialTestDrivenUnknownResourcesKey] = new PromptEntryDefinition(
                    SpecialTestDrivenUnknownResourcesKey,
                    "Test-Driven Approach for Unknown/Inaccessible Resources",
                    "FlowBlox.AIAssistant.Prompts.ExplainingTestDrivenUnknownResources.txt",
                    "Special: on-demand guidance for iterative flow construction when content is unknown or inaccessible.",
                    false),
                [SpecialModifiersAndFieldValidatorsKey] = new PromptEntryDefinition(
                    SpecialModifiersAndFieldValidatorsKey,
                    "Explaining Modifiers and Field Validators",
                    "FlowBlox.AIAssistant.Prompts.ExplainingModifiersAndFieldValidators.txt",
                    "Special: on-demand guidance for compact field-value post-processing and validation/filtering.",
                    false),
                [SpecialFlowRecursionKey] = new PromptEntryDefinition(
                    SpecialFlowRecursionKey,
                    "Explaining Flow Recursion",
                    "FlowBlox.AIAssistant.Prompts.ExplainingFlowRecursion.txt",
                    "Special: on-demand guidance for RecursiveCallFlowBlock setup and paging-style recursion loops.",
                    false)
            };

        private static readonly Lazy<IReadOnlyDictionary<string, PromptEntry>> Entries = new(LoadEntries, isThreadSafe: true);

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
                    ComputeSha256Hex(normalized),
                    definition.IsIncludedInInitialPrompt);
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

        private sealed record PromptEntryDefinition(string Key, string Title, string ResourceName, string Hint, bool IsIncludedInInitialPrompt);

        internal sealed record PromptEntry(string Key, string Title, string Hint, string Content, string ContentHash, bool IsIncludedInInitialPrompt);
    }
}


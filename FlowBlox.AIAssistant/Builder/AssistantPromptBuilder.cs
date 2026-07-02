using System.Diagnostics;
using System.Reflection;
using System.Text;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Models.Components;
using Newtonsoft.Json;

namespace FlowBlox.AIAssistant.Builder
{
    internal static class AssistantPromptBuilder
    {
        public static string BuildSystemPrompt()
        {
            var prompt = AssistantPromptCatalog.GetPromptContentOrNull(AssistantPromptCatalog.SystemMessageKey);
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new InvalidOperationException(
                    $"Required assistant system prompt '{AssistantPromptCatalog.SystemMessageKey}' is missing or empty.");
            }

            return ReplaceRuntimePromptTokens(prompt);
        }

        public static string BuildSessionBootstrapPrompt(IReadOnlyList<ToolDefinition> toolDefinitions)
        {
            var rootCategories = FlowBlockCategory.GetAll()
                .Where(x => x.ParentCategory == null)
                .Select(x => x.DisplayName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();
            var template = AssistantPromptCatalog.GetPromptContentOrNull(AssistantPromptCatalog.SessionBootstrapKey);
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new InvalidOperationException(
                    $"Required assistant bootstrap prompt '{AssistantPromptCatalog.SessionBootstrapKey}' is missing or empty.");
            }

            return ReplaceRuntimePromptTokens(template)
                .Replace("{{ROOT_CATEGORIES}}", string.Join(", ", rootCategories), StringComparison.Ordinal)
                .Replace("{{CENTRAL_GUIDELINES}}", BuildCentralGuidelinesText(), StringComparison.Ordinal)
                .Replace("{{EXPLANATION_MANIFEST}}", BuildExplanationManifestText(), StringComparison.Ordinal)
                .Replace("{{AVAILABLE_TOOLS}}", BuildToolDefinitionsText(toolDefinitions), StringComparison.Ordinal);
        }

        public static string BuildRoundPrompt(
            string userPrompt,
            string? projectJson,
            List<string> toolTranscript,
            int round,
            int maxRounds)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Round: {round}/{maxRounds}");

            if (round == 1)
            {
                sb.AppendLine("User prompt:");
                sb.AppendLine(userPrompt);
                sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(projectJson))
                {
                    sb.AppendLine("Current project JSON:");
                    sb.AppendLine(projectJson);
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("Tool execution updates since last round:");

                if (toolTranscript.Count == 0)
                {
                    sb.AppendLine("[]");
                }
                else
                {
                    foreach (var item in toolTranscript)
                        sb.AppendLine(item);
                }
            }

            return sb.ToString();
        }

        private static string BuildToolDefinitionsText(IReadOnlyList<ToolDefinition> toolDefinitions)
        {
            var sb = new StringBuilder();
            foreach (var tool in toolDefinitions ?? Array.Empty<ToolDefinition>())
            {
                sb.Append("- ");
                sb.Append(tool.Name);
                sb.Append(": ");
                sb.Append(tool.Description);
                if (tool.ArgumentsSchema?.HasValues == true)
                {
                    sb.Append(" args=");
                    sb.Append(tool.ArgumentsSchema.ToString(Formatting.None));
                }

                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildExplanationManifestText()
        {
            var explanations = AssistantPromptCatalog.GetAllEntries();
            if (explanations.Count == 0)
                return "[]";

            return string.Join(
                ", ",
                explanations.Select(x => $"{x.Key}:{x.ContentHash}{(x.IsIncludedInInitialPrompt ? ":included" : ":on-demand")}"));
        }

        private static string BuildCentralGuidelinesText()
        {
            var sections = new List<string>();

            AddPromptSection(sections, "Topic: FlowLogic", AssistantPromptCatalog.FlowLogicKey);
            AddPromptSection(sections, "Topic: IterationContext / Flow", AssistantPromptCatalog.IterationContextKey);
            AddPromptSection(sections, "Topic: FlowBlocks Managing an Object", AssistantPromptCatalog.FlowBlocksManagingObjectKey);
            AddPromptSection(sections, "Topic: Update / Delete Handling", AssistantPromptCatalog.EditAndDeleteKey);
            AddPromptSection(sections, "Topic: Naming Conventions", AssistantPromptCatalog.NamingConventionsKey);
            AddPromptSection(sections, "Topic: Execution Requirements / Required Fields", AssistantPromptCatalog.ExecutionRequirementsKey);
            AddPromptSection(sections, "Topic: Flow Organization Patterns", AssistantPromptCatalog.FlowOrganizationPatternsKey);
            AddPromptSection(sections, "Topic: Version Notes", AssistantPromptCatalog.VersionNotesKey);

            if (sections.Count == 0)
                return "No central guidelines available.";

            return string.Join("\n\n", sections);
        }

        private static void AddPromptSection(List<string> sections, string title, string key)
        {
            var content = AssistantPromptCatalog.GetPromptContentOrNull(key);
            if (!string.IsNullOrWhiteSpace(content))
                sections.Add(title + "\n" + ReplaceRuntimePromptTokens(content).Trim());
        }

        private static string ReplaceRuntimePromptTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text ?? string.Empty;

            return text
                .Replace("{{FLOWBLOX_VERSION}}", GetFlowBloxApplicationVersion(), StringComparison.Ordinal)
                .Replace("{{FLOWBLOX_GITHUB_REPOSITORY_URL}}", GlobalUrls.FlowBloxGitHubRepository, StringComparison.Ordinal)
                .Replace("{{FLOWBLOX_SAMPLE_EXTENSION_REPOSITORY_URL}}", GlobalUrls.FlowBloxSampleExtensionRepository, StringComparison.Ordinal)
                .Replace("{{FLOWBLOX_WEBSITE_URL}}", GlobalUrls.FlowBloxWebsite, StringComparison.Ordinal)
                .Replace("{{FLOWBLOX_REPORT_PROBLEM_URL}}", GlobalUrls.FlowBloxReportProblem, StringComparison.Ordinal);
        }

        private static string GetFlowBloxApplicationVersion()
        {
            try
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                    return "unknown";

                var location = entryAssembly.Location;
                if (!string.IsNullOrWhiteSpace(location))
                {
                    var productVersion = FileVersionInfo.GetVersionInfo(location).ProductVersion;
                    if (!string.IsNullOrWhiteSpace(productVersion))
                        return productVersion;
                }

                return entryAssembly.GetName().Version?.ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
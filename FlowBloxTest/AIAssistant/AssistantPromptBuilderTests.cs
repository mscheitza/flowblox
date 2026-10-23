using FlowBlox.AIAssistant.Builder;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class AssistantPromptBuilderTests
    {
        [TestMethod]
        public void BuildSessionBootstrapPrompt_RoutesAuxiliaryProjectUseCasesWithoutEmbeddingTemplateCatalog()
        {
            var prompt = AssistantPromptBuilder.BuildSessionBootstrapPrompt([]);

            Assert.IsFalse(prompt.Contains("{{EXPLANATION_ROUTING}}", StringComparison.Ordinal));
            StringAssert.Contains(prompt, "Local capabilities via auxiliary projects (local skills)");
            StringAssert.Contains(prompt, "bundled or session-created helpers");
            StringAssert.Contains(prompt, "accessing internal/external websites and inspecting DOM content");
            StringAssert.Contains(prompt, "reading database tables, columns, and types");
            StringAssert.Contains(prompt, "`explaining_auxiliary_projects`");
            Assert.IsFalse(prompt.Contains("FetchUrlDom:", StringComparison.Ordinal));
        }

        [TestMethod]
        public void BuildSessionBootstrapPrompt_ListsOnlyOnDemandExplanations()
        {
            var prompt = AssistantPromptBuilder.BuildSessionBootstrapPrompt([]);

            StringAssert.Contains(prompt, "`explaining_generators`");
            Assert.IsFalse(prompt.Contains("`explaining_version_notes`", StringComparison.Ordinal));
            Assert.IsFalse(prompt.Contains("`system_message`", StringComparison.Ordinal));
            Assert.IsFalse(prompt.Contains("`session_bootstrap`", StringComparison.Ordinal));
        }

    }
}

using FlowBlox.AIAssistant.Builder;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class AssistantPromptBuilderTests
    {
        [TestMethod]
        public void BuildSessionBootstrapPrompt_IncludesStandardAuxiliaryProjectOverview()
        {
            var prompt = AssistantPromptBuilder.BuildSessionBootstrapPrompt([]);

            Assert.IsFalse(prompt.Contains("{{STANDARD_AUXILIARY_PROJECTS}}", StringComparison.Ordinal));
            StringAssert.Contains(prompt, "FetchUrlDom:");
            StringAssert.Contains(prompt, "BraveWebSearch:");
            StringAssert.Contains(prompt, "SqliteTableDiscovery:");
        }

        [TestMethod]
        public void BuildSessionBootstrapPrompt_ExplainsWhenExplanationIsAlreadyLoaded()
        {
            var prompt = AssistantPromptBuilder.BuildSessionBootstrapPrompt([]);

            StringAssert.Contains(
                prompt,
                "counts as loaded when its manifest entry is tagged `:included`");
            StringAssert.Contains(
                prompt,
                "or when its content is present as a system message after `LoadExplanationContent`");
            StringAssert.Matches(
                prompt,
                new System.Text.RegularExpressions.Regex(
                    "explaining_version_notes:[a-f0-9]{64}:included",
                    System.Text.RegularExpressions.RegexOptions.CultureInvariant));
            StringAssert.Matches(
                prompt,
                new System.Text.RegularExpressions.Regex(
                    "explaining_generators:[a-f0-9]{64}:on-demand",
                    System.Text.RegularExpressions.RegexOptions.CultureInvariant));
        }
    }
}

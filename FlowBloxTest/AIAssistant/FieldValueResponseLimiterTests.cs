using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools.Util;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class FieldValueResponseLimiterTests
    {
        [TestMethod]
        public void TryCreate_UsesConfiguredMaximumWhenRequestOmitsLimit()
        {
            var success = FieldValueResponseLimiter.TryCreate(
                CreateConfiguration(4000),
                null,
                out var limiter,
                out var error);

            Assert.IsTrue(success, error);
            Assert.AreEqual(4000, limiter!.MaxTokens);
        }

        [TestMethod]
        public void TryCreate_UsesSmallerRequestedLimit()
        {
            var success = FieldValueResponseLimiter.TryCreate(
                CreateConfiguration(4000),
                50,
                out var limiter,
                out var error);

            Assert.IsTrue(success, error);
            Assert.AreEqual(50, limiter!.MaxTokens);
        }

        [TestMethod]
        public void TryCreate_RejectsLimitAboveConfiguredMaximum()
        {
            var success = FieldValueResponseLimiter.TryCreate(
                CreateConfiguration(100),
                101,
                out var limiter,
                out var error);

            Assert.IsFalse(success);
            Assert.IsNull(limiter);
            StringAssert.Contains(error, "between 1 and the configured MaxFieldValuesTokensPerResponse (100)");
        }

        [TestMethod]
        public void TryCreate_DoesNotApplyAnAdditionalInternalMaximum()
        {
            var success = FieldValueResponseLimiter.TryCreate(
                CreateConfiguration(25000),
                25000,
                out var limiter,
                out var error);

            Assert.IsTrue(success, error);
            Assert.AreEqual(25000, limiter!.MaxTokens);
        }

        private static AssistantConfiguration CreateConfiguration(int maxTokens)
        {
            return new AssistantConfiguration
            {
                ApproximateCharactersPerToken = 4,
                MaxFieldValuesTokensPerResponse = maxTokens
            };
        }
    }
}

using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Enums;
using FlowBloxTest.Services;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class OpenAICompatibleProviderTests
    {
        [TestMethod]
        public async Task ExecuteChatAsync_ReasoningDisabled_OmitsReasoningEffort()
        {
            var provider = new RecordingOpenAICompatibleProvider
            {
                ReasoningEffort = AIReasoningEffort.None
            };

            await provider.ExecuteChatAsync(CreateRequest(), CancellationToken.None);

            Assert.IsNull(provider.ExecutionSettings.Single().ReasoningEffort);
        }

        [TestMethod]
        public async Task ExecuteChatAsync_ReasoningEnabled_SendsConfiguredEffort()
        {
            var provider = new RecordingOpenAICompatibleProvider
            {
                ReasoningEffort = AIReasoningEffort.High
            };

            await provider.ExecuteChatAsync(CreateRequest(), CancellationToken.None);

            Assert.AreEqual("high", provider.ExecutionSettings.Single().ReasoningEffort);
        }

        private static AIChatRequest CreateRequest()
        {
            var request = new AIChatRequest();
            request.Messages.Add(new AIChatMessage
            {
                Role = "user",
                Content = "Hello"
            });
            return request;
        }
    }
}

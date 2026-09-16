using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    public class OptionsToolHandlerTests
    {
        private const string TestIntegerOptionName = "AI.Assistant.Tests.Integer";
        private const string TestPasswordOptionName = "AI.Assistant.Tests.Password";

        [TestCleanup]
        public void Cleanup()
        {
            ToolHandlerUtilities.SetAssistantConfigurationProvider(null);
            var options = FlowBloxOptions.GetOptionInstance();
            options.OptionCollection.Remove(TestIntegerOptionName);
            options.OptionCollection.Remove(TestPasswordOptionName);
        }

        [TestMethod]
        public async Task SearchOptions_WithoutPermission_ReturnsPermissionError()
        {
            var toolApi = CreateToolApi(AssistantOptionsAccessLevel.None);

            var response = await toolApi.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "SearchOptions",
                    Arguments = new JObject { ["searchForNames"] = "Runtime" }
                },
                CancellationToken.None);

            Assert.IsFalse(response.Ok);
            StringAssert.Contains(response.Error, "not currently permitted to search options");
            StringAssert.Contains(response.Error, "AI Assistant Configuration > Permissions > Options access");
        }

        [TestMethod]
        public async Task SearchOptions_WithReadPermission_MasksPasswordOptionValue()
        {
            var options = FlowBloxOptions.GetOptionInstance();
            options.OptionCollection[TestPasswordOptionName] = new OptionElement(
                TestPasswordOptionName,
                "stored-secret-placeholder",
                "Password option used by tests.",
                OptionElement.OptionType.Password);
            var toolApi = CreateToolApi(AssistantOptionsAccessLevel.ReadOnly);

            var response = await toolApi.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "SearchOptions",
                    Arguments = new JObject { ["searchForNames"] = "Assistant.Tests.Password" }
                },
                CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            var option = response.Result["options"]?.OfType<JObject>().Single();
            Assert.AreEqual(TestPasswordOptionName, option?["name"]?.Value<string>());
            Assert.AreEqual("Password", option?["type"]?.Value<string>());
            Assert.AreEqual(ToolHandlerUtilities.PasswordOptionProtectedValueMessage, option?["value"]?.Value<string>());
        }

        [TestMethod]
        public async Task SetOptionValue_WithReadOnlyPermission_ReturnsPermissionError()
        {
            var toolApi = CreateToolApi(AssistantOptionsAccessLevel.ReadOnly);

            var response = await toolApi.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "SetOptionValue",
                    Arguments = new JObject
                    {
                        ["key"] = TestIntegerOptionName,
                        ["stringValue"] = "42"
                    }
                },
                CancellationToken.None);

            Assert.IsFalse(response.Ok);
            StringAssert.Contains(response.Error, "not currently permitted to set options");
        }

        [TestMethod]
        public async Task SetOptionValue_WithInvalidInteger_ReturnsValidationError()
        {
            var options = FlowBloxOptions.GetOptionInstance();
            options.OptionCollection[TestIntegerOptionName] = new OptionElement(
                TestIntegerOptionName,
                "1",
                "Integer option used by tests.",
                OptionElement.OptionType.Integer);
            var toolApi = CreateToolApi(AssistantOptionsAccessLevel.ReadWrite);

            var response = await toolApi.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "SetOptionValue",
                    Arguments = new JObject
                    {
                        ["key"] = TestIntegerOptionName,
                        ["stringValue"] = "not an integer"
                    }
                },
                CancellationToken.None);

            Assert.IsFalse(response.Ok);
            StringAssert.Contains(response.Error, "expects an integer value");
            Assert.AreEqual("1", options.GetOption(TestIntegerOptionName)?.Value);
        }

        private static DefaultToolApi CreateToolApi(AssistantOptionsAccessLevel optionsAccessLevel)
        {
            return new DefaultToolApi(() => new AssistantConfiguration
            {
                OptionsAccessLevel = optionsAccessLevel
            });
        }
    }
}

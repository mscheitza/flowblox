using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class PublishedScriptToolHandlerTests
    {
        [TestMethod]
        public async Task GetPublishedScriptContent_ReturnsRequestedPythonScript()
        {
            var api = new DefaultToolApi();

            var response = await Execute(
                api,
                "python",
                "export_onnx_qa_model.py");

            Assert.IsTrue(response.Ok, response.Error);
            Assert.AreEqual("python", response.Result.Value<string>("scriptType"));
            Assert.AreEqual("export_onnx_qa_model.py", response.Result.Value<string>("scriptName"));
            StringAssert.Contains(response.Result.Value<string>("content"), "DEFAULT_MODEL_ROOT");
        }

        [TestMethod]
        public async Task GetPublishedScriptContent_RejectsUnsupportedScriptType()
        {
            var response = await Execute(new DefaultToolApi(), "powershell", "setup.ps1");

            Assert.IsFalse(response.Ok);
            StringAssert.Contains(response.Error, "scriptType must be 'python'");
        }

        [TestMethod]
        public async Task GetPublishedScriptContent_RejectsPaths()
        {
            var response = await Execute(
                new DefaultToolApi(),
                "python",
                "../export_onnx_qa_model.py");

            Assert.IsFalse(response.Ok);
            StringAssert.Contains(response.Error, "plain .py filename");
        }

        private static Task<ToolResponse> Execute(
            DefaultToolApi api,
            string scriptType,
            string scriptName) =>
            api.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "GetPublishedScriptContent",
                    Arguments = new JObject
                    {
                        ["scriptType"] = scriptType,
                        ["scriptName"] = scriptName
                    }
                },
                CancellationToken.None);

    }
}

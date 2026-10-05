using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class DistributedDataHandlerTests
    {
        [TestMethod]
        public async Task DistributedData_CanBeListedAndReadButNotTraversed()
        {
            var api = new DefaultToolApi();
            var listed = await api.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "GetDistributedData",
                    Arguments = new JObject { ["dataType"] = "auxiliary_project" }
                },
                CancellationToken.None);
            var read = await api.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "GetDistributedDataContent",
                    Arguments = new JObject
                    {
                        ["dataType"] = "python",
                        ["fileName"] = "export_onnx_qa_model.py"
                    }
                },
                CancellationToken.None);
            var traversal = await api.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "GetDistributedDataContent",
                    Arguments = new JObject
                    {
                        ["dataType"] = "python",
                        ["fileName"] = "../export_onnx_qa_model.py"
                    }
                },
                CancellationToken.None);

            Assert.IsTrue(listed.Ok, listed.Error);
            CollectionAssert.Contains(
                listed.Result["dataTypes"]![0]!["fileNames"]!.Values<string>().ToList(),
                "Web-Search.fbprj");
            Assert.IsTrue(read.Ok, read.Error);
            StringAssert.Contains(read.Result.Value<string>("content"), "DEFAULT_MODEL_ROOT");
            Assert.IsFalse(traversal.Ok);
            StringAssert.Contains(traversal.Error, "plain filename");
        }
    }
}

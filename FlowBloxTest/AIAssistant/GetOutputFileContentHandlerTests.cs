using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;
using Newtonsoft.Json.Linq;
using System.Text;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class GetOutputFileContentHandlerTests
    {
        private string _temporaryOutputRoot = null!;
        private string _previousOutputRoot = null!;
        private FlowBloxProject? _previousProject;

        [TestInitialize]
        public void Initialize()
        {
            _temporaryOutputRoot = Path.Combine(Path.GetTempPath(), "FlowBloxAiOutputFileTests", Guid.NewGuid().ToString("N"));
            var option = FlowBloxOptions.GetOptionInstance().GetOption("Paths.OutputDir")
                ?? throw new AssertFailedException("Paths.OutputDir option is missing.");
            _previousOutputRoot = option.Value;
            option.Value = _temporaryOutputRoot;
            _previousProject = FlowBloxProjectManager.Instance.ActiveProject;
        }

        [TestCleanup]
        public void Cleanup()
        {
            FlowBloxProjectManager.Instance.ActiveProject = _previousProject;

            var option = FlowBloxOptions.GetOptionInstance().GetOption("Paths.OutputDir");
            if (option != null)
                option.Value = _previousOutputRoot;

            if (Directory.Exists(_temporaryOutputRoot))
                Directory.Delete(_temporaryOutputRoot, recursive: true);
        }

        [TestMethod]
        public async Task GetOutputFileContent_ReadsTextFileBelowProjectOutputDirectory()
        {
            var project = new FlowBloxProject { ProjectName = "Output content test" };
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var outputFile = Path.Combine(project.ProjectOutputDirectory, "reports", "result.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
            await File.WriteAllBytesAsync(outputFile, Encoding.UTF8.GetBytes("Id;Name\n1;Test"));

            var response = await Execute("reports/result.csv");

            Assert.IsTrue(response.Ok, response.Error);
            Assert.AreEqual("reports/result.csv", response.Result.Value<string>("key"));
            Assert.AreEqual("text/csv", response.Result.Value<string>("mimeType"));
            Assert.AreEqual("Id;Name\n1;Test", response.Result.Value<string>("textContent"));
        }

        [TestMethod]
        public async Task GetOutputFileContent_RejectsPathTraversal()
        {
            var project = new FlowBloxProject { ProjectName = "Output containment test" };
            FlowBloxProjectManager.Instance.ActiveProject = project;

            var response = await Execute("../outside.txt");

            Assert.IsFalse(response.Ok);
            StringAssert.Contains(response.Error, "must not contain");
        }

        private static Task<ToolResponse> Execute(string key) =>
            new DefaultToolApi().ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "GetOutputFileContent",
                    Arguments = new JObject { ["key"] = key }
                },
                CancellationToken.None);
    }
}

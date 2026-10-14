using FlowBlox.AIAssistant.Tools.Handler;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class UpdateManagedObjectHandlerTests
    {
        private FlowBloxProject _previousProject;
        private FlowBloxProject _project;

        [TestInitialize]
        public void Initialize()
        {
            _previousProject = FlowBloxProjectManager.Instance.ActiveProject;
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestCleanup]
        public void Cleanup()
        {
            FlowBloxProjectManager.Instance.ActiveProject = _previousProject;
        }

        [TestMethod]
        public async Task UpdateManagedObject_QualifiedFieldNameRenamesOnlyTheFieldFromTheSpecifiedSource()
        {
            var first = CreateSource("FirstSource");
            var second = CreateSource("TableReader_Participants");
            first.ResultField.Name = "Vorname";
            second.ResultField.Name = "Vorname";

            var response = await Rename("$TableReader_Participants::Vorname", "FirstName");

            Assert.IsTrue(response.Ok, response.Error);
            Assert.AreEqual("Vorname", first.ResultField.Name);
            Assert.AreEqual("FirstName", second.ResultField.Name);
            Assert.AreSame(second, second.ResultField.Source);
        }

        [TestMethod]
        public async Task UpdateManagedObject_UnknownQualifiedFieldNameDoesNotFallBackToAnUnqualifiedField()
        {
            var source = CreateSource("ExistingSource");
            source.ResultField.Name = "Vorname";

            var response = await Rename("$MissingSource::Vorname", "FirstName");

            Assert.IsFalse(response.Ok);
            Assert.AreEqual("Vorname", source.ResultField.Name);
        }

        [TestMethod]
        public async Task UpdateManagedObject_StillResolvesOrdinaryManagedObjectNames()
        {
            var source = CreateSource("ExistingSource");
            source.ResultField.Name = "Vorname";

            var response = await Rename("Vorname", "FirstName");

            Assert.IsTrue(response.Ok, response.Error);
            Assert.AreEqual("FirstName", source.ResultField.Name);
        }

        private FileWriterFlowBlock CreateSource(string name)
        {
            var registry = _project.FlowBloxRegistry;
            var source = registry.CreateFlowBlockUnregistered<FileWriterFlowBlock>();
            source.Name = name;
            registry.PostProcessFlowBlockCreated(source);
            registry.Register(source);
            return source;
        }

        private static Task<FlowBlox.AIAssistant.Models.ToolResponse> Rename(string name, string newName)
        {
            return new UpdateManagedObjectHandler().HandleAsync(new JObject
            {
                ["name"] = name,
                ["path"] = "/Name",
                ["value"] = newName
            }, CancellationToken.None);
        }
    }
}

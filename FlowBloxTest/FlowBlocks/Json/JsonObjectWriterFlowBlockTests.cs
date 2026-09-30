using FlowBlox.Core.Models.FlowBlocks.Json;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.FlowBlocks.Json
{
    [TestClass]
    public class JsonObjectWriterFlowBlockTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;
        private FlowBloxUnitTestRuntime _runtime;

        [TestInitialize]
        public void TestInitialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
            CreateFlowBlock<FlowBlox.Core.Models.FlowBlocks.SequenceFlow.StartFlowBlock>();
            _runtime = new FlowBloxUnitTestRuntime(_project);
        }

        [TestMethod]
        public void Execute_JPathSelectsExistingObject_UpdatesObject()
        {
            var source = CreateJsonObject("""
            {
              "participants": [
                { "name": "first" },
                { "name": "second" }
              ]
            }
            """);
            var writer = CreateWriter(source, "$.participants[?(@.name == 'second')]");
            writer.Assignments.Add(new JsonPropertyValueAssignment
            {
                PropertyName = "status",
                Value = "updated"
            });

            Assert.IsTrue(writer.Execute(_runtime, null));
            Assert.AreEqual("updated", source.InternalJsonObject["participants"]?[1]?["status"]?.ToString());
        }

        [TestMethod]
        public void Execute_FinalPropertyDoesNotExist_CreatesObject()
        {
            var source = CreateJsonObject("""{ "participant": {} }""");
            var writer = CreateWriter(source, "$.participant.details");
            writer.Assignments.Add(new JsonPropertyValueAssignment
            {
                PropertyName = "active",
                Value = "true"
            });

            Assert.IsTrue(writer.Execute(_runtime, null));
            Assert.AreEqual("true", source.InternalJsonObject["participant"]?["details"]?["active"]?.ToString());
        }

        [TestMethod]
        public void Execute_ArrayTarget_AppendsNewObject()
        {
            var source = CreateJsonObject("""{ "participants": [] }""");
            var writer = CreateWriter(source, "$.participants", isArray: true);
            writer.Assignments.Add(new JsonPropertyValueAssignment
            {
                PropertyName = "name",
                Value = "new participant"
            });

            Assert.IsTrue(writer.Execute(_runtime, null));

            var participants = (JArray)source.InternalJsonObject["participants"]!;
            Assert.AreEqual(1, participants.Count);
            Assert.AreEqual("new participant", participants[0]?["name"]?.ToString());
        }

        private JsonObjectFlowBlock CreateJsonObject(string jsonContent)
        {
            var source = CreateFlowBlock<JsonObjectFlowBlock>();
            source.JsonContent = jsonContent;
            Assert.IsTrue(source.Execute(_runtime, null));
            return source;
        }

        private JsonObjectWriterFlowBlock CreateWriter(
            JsonObjectFlowBlock source,
            string path,
            bool isArray = false)
        {
            var writer = CreateFlowBlock<JsonObjectWriterFlowBlock>();
            writer.AssociatedJsonObject = source;
            writer.Path = path;
            writer.IsArray = isArray;
            return writer;
        }
    }
}

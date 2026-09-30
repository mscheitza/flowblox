using FlowBlox.Core.Models.FlowBlocks.Json;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;

namespace FlowBloxTest.FlowBlocks.Json
{
    [TestClass]
    public class JsonManyPathsSelectorFlowBlockTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void Execute_JPathReturnsMultipleMatches_SerializesMatchesAsArray()
        {
            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();
            var selector = CreateFlowBlock<JsonManyPathsSelectorFlowBlock>(startFlowBlock);
            var resultField = CreateUserField("Ids");
            selector.JsonContent = """
            [
              { "id": "first" },
              { "id": "second" }
            ]
            """;
            selector.MappingEntries.Add(new JsonManyPathsSelectorMappingEntry
            {
                JsonPath = "$[*].id",
                Field = resultField
            });
            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));

            var values = selector.GridElementResult.Results
                .Single()
                .FieldValueMappings
                .ToDictionary(x => x.Field, x => x.Value);
            Assert.AreEqual("[\"first\",\"second\"]", values[resultField]);
        }
    }
}

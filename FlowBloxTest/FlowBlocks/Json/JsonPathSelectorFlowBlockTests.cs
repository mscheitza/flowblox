using FlowBlox.Core.Models.FlowBlocks.Json;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;

namespace FlowBloxTest.FlowBlocks.Json
{
    [TestClass]
    public class JsonPathSelectorFlowBlockTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void Execute_RootArrayProjection_ReturnsSelectedValues()
        {
            var selector = CreateSelector("""
            [
              { "id": "first" },
              { "id": "second" }
            ]
            """, "$[*].id");

            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));
            CollectionAssert.AreEqual(new[] { "first", "second" }, GetResultValues(selector));
        }

        [TestMethod]
        public void Execute_FilterAndProjection_ReturnsMatchingValues()
        {
            var selector = CreateSelector("""
            {
              "addresses": [
                { "Country": "Germany", "Street": "Main Street" },
                { "Country": "France", "Street": "Rue A" },
                { "Country": "Germany", "Street": "Second Street" }
              ]
            }
            """, "$.addresses[?(@.Country == 'Germany')].Street");

            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));
            CollectionAssert.AreEqual(new[] { "Main Street", "Second Street" }, GetResultValues(selector));
        }

        [TestMethod]
        public void Execute_SelectedArray_ReturnsItsItemsIndividually()
        {
            var selector = CreateSelector("""
            {
              "participants": [
                { "name": "first" },
                { "name": "second" }
              ]
            }
            """, "$.participants");

            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));
            CollectionAssert.AreEqual(
                new[] { "{\"name\":\"first\"}", "{\"name\":\"second\"}" },
                GetResultValues(selector));
        }

        private JsonPathSelectorFlowBlock CreateSelector(string jsonContent, string path)
        {
            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();
            var selector = CreateFlowBlock<JsonPathSelectorFlowBlock>(startFlowBlock);
            selector.JsonContent = jsonContent;
            selector.Path = path;
            return selector;
        }

        private static string[] GetResultValues(JsonPathSelectorFlowBlock selector)
        {
            return selector.GridElementResult.Results
                .SelectMany(x => x.FieldValueMappings)
                .Select(x => x.Value)
                .ToArray();
        }
    }
}

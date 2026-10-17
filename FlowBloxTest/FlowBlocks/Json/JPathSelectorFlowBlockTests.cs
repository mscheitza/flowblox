using FlowBlox.Core.Models.FlowBlocks.Json;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Attributes;
using FlowBlox.Test.Runtime;
using System.Reflection;

namespace FlowBloxTest.FlowBlocks.Json
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class JPathSelectorFlowBlockTests : FlowBloxTestsBase
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

        [TestMethod]
        public void Execute_RowPathPreservesRowsAndUsesFallbackForMissingValues()
        {
            var selector = CreateSelector("""
            [
              { "id": 1, "category": { "name": "Dogs" } },
              { "id": 2, "category": { } },
              { "id": 3 },
              { "id": 4, "category": { "name": "Cats" } }
            ]
            """, "$[*].category.name");
            selector.RowPath = "$[*]";
            selector.FallbackValue = "No category";

            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));
            CollectionAssert.AreEqual(
                new[] { "Dogs", "No category", "No category", "Cats" },
                GetResultValues(selector));
        }

        [TestMethod]
        public void Execute_RowPathWithUnsetFallbackUsesEmptyString()
        {
            var selector = CreateSelector("""
            [
              { "category": { "name": "Dogs" } },
              { "category": { } },
              { }
            ]
            """, "$[*].category.name");
            selector.RowPath = "$[*]";

            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));
            CollectionAssert.AreEqual(new[] { "Dogs", string.Empty, string.Empty }, GetResultValues(selector));
        }

        [TestMethod]
        public void Execute_WithoutRowPathKeepsExistingMatchOnlyBehavior()
        {
            var selector = CreateSelector("""
            [
              { "category": { "name": "Dogs" } },
              { "category": { } },
              { }
            ]
            """, "$[*].category.name");

            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));
            CollectionAssert.AreEqual(new[] { "Dogs" }, GetResultValues(selector));
        }

        [TestMethod]
        public void FallbackValue_IsActiveOnlyWhenRowPathIsConfigured()
        {
            var selector = CreateSelector("[]", "$[*].name");
            var activationCondition = typeof(JPathSelectorFlowBlock)
                .GetProperty(nameof(JPathSelectorFlowBlock.FallbackValue))!
                .GetCustomAttribute<ActivationConditionAttribute>()!;

            Assert.IsFalse(activationCondition.IsActive(selector));

            selector.RowPath = "$[*]";

            Assert.IsTrue(activationCondition.IsActive(selector));
        }

        private JPathSelectorFlowBlock CreateSelector(string jsonContent, string path)
        {
            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();
            var selector = CreateFlowBlock<JPathSelectorFlowBlock>(startFlowBlock);
            selector.JsonContent = jsonContent;
            selector.Path = path;
            return selector;
        }

        private static string[] GetResultValues(JPathSelectorFlowBlock selector)
        {
            return selector.GridElementResult.Results
                .SelectMany(x => x.FieldValueMappings)
                .Select(x => x.Value)
                .ToArray();
        }
    }
}

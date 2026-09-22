using FlowBlox.Core.Models.FlowBlocks.Json;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;
using FlowBloxTest.FlowBlocks;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.FlowBlocks.Json
{
    [TestClass]
    public class JsonPathSelectorTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void GetJToken_ReturnsArrayWithoutIndex()
        {
            var root = JToken.Parse("""
            {
              "participant": {
                "addresses": [
                  { "Country": "Germany", "Street": "Main Street" },
                  { "Country": "France", "Street": "Rue A" }
                ]
              }
            }
            """);

            var token = JsonPathSelector.GetJToken(root, "participant/addresses", out _, out _);

            Assert.IsInstanceOfType(token, typeof(JArray));
            var array = (JArray)token;
            Assert.AreEqual(2, array.Count);
            Assert.AreEqual("Main Street", array[0]["Street"]?.ToString());
        }

        [TestMethod]
        public void GetJToken_FiltersArrayAndReturnsMatchingObjects()
        {
            var root = CreateAddressRoot();

            var token = JsonPathSelector.GetJToken(root, "addresses/@Country=Germany", out _, out _);

            Assert.IsInstanceOfType(token, typeof(JArray));
            var array = (JArray)token;
            Assert.AreEqual(2, array.Count);
            Assert.AreEqual("Main Street", array[0]["Street"]?.ToString());
            Assert.AreEqual("Second Street", array[1]["Street"]?.ToString());
        }

        [TestMethod]
        public void GetJToken_FiltersArrayAndProjectsProperty()
        {
            var root = CreateAddressRoot();

            var token = JsonPathSelector.GetJToken(root, "addresses/@Country=Germany/Street", out _, out _);

            Assert.IsInstanceOfType(token, typeof(JArray));
            CollectionAssert.AreEqual(
                new[] { "Main Street", "Second Street" },
                ((JArray)token).Select(x => x.ToString()).ToArray());
        }

        [TestMethod]
        public void GetJToken_SupportsComparisonOperators()
        {
            var root = JToken.Parse("""
            {
              "items": [
                { "Name": "A", "Price": 10, "Code": "" },
                { "Name": "B", "Price": 20, "Code": null },
                { "Name": "C", "Price": 30, "Code": "x" }
              ]
            }
            """);

            AssertNames(root, "items/@Price>10/Name", "B", "C");
            AssertNames(root, "items/@Price>=20/Name", "B", "C");
            AssertNames(root, "items/@Price<20/Name", "A");
            AssertNames(root, "items/@Price<=20/Name", "A", "B");
            AssertNames(root, "items/@Name!=B/Name", "A", "C");
            AssertNames(root, "items/@Code is null/Name", "B");
            AssertNames(root, "items/@Code is not null/Name", "A", "C");
            AssertNames(root, "items/@Code is empty/Name", "A", "B");
            AssertNames(root, "items/@Code is not empty/Name", "C");
        }

        [TestMethod]
        public void JsonPathSelectorFlowBlock_ReturnsSerializedArrayItems()
        {
            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();
            var selector = CreateFlowBlock<JsonPathSelectorFlowBlock>(startFlowBlock);
            selector.JsonContent = """
            {
              "participant": {
                "addresses": [
                  { "Country": "Germany", "Street": "Main Street" },
                  { "Country": "France", "Street": "Rue A" }
                ]
              }
            }
            """;
            selector.Path = "participant/addresses";

            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));

            var values = selector.GridElementResult.Results
                .SelectMany(x => x.FieldValueMappings)
                .Select(x => x.Value)
                .ToList();

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("""{"Country":"Germany","Street":"Main Street"}""", values[0]);
            Assert.AreEqual("""{"Country":"France","Street":"Rue A"}""", values[1]);
        }

        private static JToken CreateAddressRoot()
        {
            return JToken.Parse("""
            {
              "addresses": [
                { "Country": "Germany", "Street": "Main Street" },
                { "Country": "France", "Street": "Rue A" },
                { "Country": "Germany", "Street": "Second Street" }
              ]
            }
            """);
        }

        private static void AssertNames(JToken root, string path, params string[] expected)
        {
            var token = JsonPathSelector.GetJToken(root, path, out _, out _);

            Assert.IsInstanceOfType(token, typeof(JArray), path);
            CollectionAssert.AreEqual(
                expected,
                ((JArray)token).Select(x => x.ToString()).ToArray(),
                path);
        }
    }
}

using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.Json;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;
using Newtonsoft.Json;
using System.Reflection;

namespace FlowBloxTest.FlowBlocks.Json
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class JPathMultiSelectorFlowBlockTests : FlowBloxTestsBase
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
            var selector = CreateFlowBlock<JPathMultiSelectorFlowBlock>(startFlowBlock);
            var resultField = CreateUserField("Ids");
            selector.JsonContent = """
            [
              { "id": "first" },
              { "id": "second" }
            ]
            """;
            selector.MappingEntries.Add(new JPathMultiSelectorMappingEntry
            {
                JPath = "$[*].id",
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

        [TestMethod]
        public void Execute_RowMappingsPreserveRowsAndFallbackValuesAcrossFields()
        {
            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();
            var selector = CreateFlowBlock<JPathMultiSelectorFlowBlock>(startFlowBlock);
            var idField = CreateUserField("Id");
            var categoryField = CreateUserField("Category");
            selector.JsonContent = """
            [
              { "id": 1, "category": { "name": "Dogs" } },
              { "id": 2, "category": { } },
              { "id": 3 },
              { "id": 4, "category": { "name": "Cats" } }
            ]
            """;
            selector.MappingEntries.Add(new JPathMultiSelectorMappingEntry
            {
                JPath = "$[*].id",
                RowPath = "$[*]",
                Field = idField
            });
            selector.MappingEntries.Add(new JPathMultiSelectorMappingEntry
            {
                JPath = "$[*].category.name",
                RowPath = "$[*]",
                FallbackValue = "No category",
                Field = categoryField
            });
            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(selector.Execute(runtime, null));

            var rows = selector.GridElementResult.Results
                .Select(result => result.FieldValueMappings.ToDictionary(mapping => mapping.Field, mapping => mapping.Value))
                .ToList();
            Assert.AreEqual(4, rows.Count);
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4" }, rows.Select(row => row[idField]).ToArray());
            CollectionAssert.AreEqual(
                new[] { "Dogs", "No category", "No category", "Cats" },
                rows.Select(row => row[categoryField]).ToArray());
        }

        [TestMethod]
        public void FallbackValue_IsActiveOnlyWhenEntryRowPathIsConfigured()
        {
            var mapping = new JPathMultiSelectorMappingEntry();
            var activationCondition = typeof(JPathMultiSelectorMappingEntry)
                .GetProperty(nameof(JPathMultiSelectorMappingEntry.FallbackValue))!
                .GetCustomAttribute<ActivationConditionAttribute>()!;

            Assert.IsFalse(activationCondition.IsActive(mapping));

            mapping.RowPath = "$[*]";

            Assert.IsTrue(activationCondition.IsActive(mapping));
        }

        [TestMethod]
        public void Deserialize_LegacyJsonPathPropertyPopulatesJPath()
        {
            var mapping = JsonConvert.DeserializeObject<JPathMultiSelectorMappingEntry>("""
            { "JsonPath": "$[*].id" }
            """);

            Assert.IsNotNull(mapping);
            Assert.AreEqual("$[*].id", mapping.JPath);
        Assert.IsFalse(JsonConvert.SerializeObject(mapping).Contains("\"JsonPath\"", StringComparison.Ordinal));
        }
    }
}

using FlowBlox.Core.Migration.MigrationStrategies;
using FlowBlox.Core.Models.FlowBlocks.Json;
using FlowBlox.Core.Services;
using FlowBlox.Core.Util.Json.SerializationBinder;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.Migration
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class FlowBloxComponentMigrationStrategy_1_0_0Tests
    {
        [TestMethod]
        public void Migrate_JsonPathProperties_TranslatesLegacySyntax()
        {
            var cases = new[]
            {
                (Type: typeof(JPathSelectorFlowBlock), Property: nameof(JPathSelectorFlowBlock.Path), Legacy: "addresses/@Country=Germany/Street", Expected: "$.addresses[?(@.Country == 'Germany')].Street"),
                (Type: typeof(JsonObjectWriterFlowBlock), Property: nameof(JsonObjectWriterFlowBlock.Path), Legacy: "participants/0", Expected: "$.participants[0]")
            };
            var strategy = new FlowBloxComponentMigrationStrategy_1_0_0();

            foreach (var migrationCase in cases)
            {
                var component = new JObject
                {
                    ["$type"] = migrationCase.Type.AssemblyQualifiedName,
                    [migrationCase.Property] = migrationCase.Legacy
                };

                strategy.Migrate(component);

                Assert.AreEqual(migrationCase.Expected, component.Value<string>(migrationCase.Property));
            }
        }

        [TestMethod]
        public void Migrate_JsonManyPathsMappingEntry_RenamesJsonPathAndTranslatesLegacySyntax()
        {
            var component = new JObject
            {
                ["$type"] = "FlowBlox.Core.Models.FlowBlocks.Json.JsonManyPathsSelectorMappingEntry, FlowBlox.Core",
                ["JsonPath"] = "$/category/name"
            };

            new FlowBloxComponentMigrationStrategy_1_0_0().Migrate(component);

            Assert.IsNull(component.Property("JsonPath"));
            Assert.AreEqual("$[*].category.name", component.Value<string>(nameof(JPathMultiSelectorMappingEntry.JPath)));
        }

        [TestMethod]
        public void LegacyTypeMappings_ResolveJsonManyPathsTypesToJPathMultiSelectorTypes()
        {
            var mappings = new FlowBloxCoreLegacyTypeMappingService().GetLegacyTypeMappings();
            var binder = new FlowBloxSerializationBinder([], mappings);

            var flowBlockType = binder.BindToType(
                "FlowBlox.Core",
                "FlowBlox.Core.Models.FlowBlocks.Json.JsonManyPathsSelectorFlowBlock");
            var mappingEntryType = binder.BindToType(
                "FlowBlox.Core",
                "FlowBlox.Core.Models.FlowBlocks.Json.JsonManyPathsSelectorMappingEntry");

            Assert.AreEqual(typeof(JPathMultiSelectorFlowBlock), flowBlockType);
            Assert.AreEqual(typeof(JPathMultiSelectorMappingEntry), mappingEntryType);
        }

        [TestMethod]
        public void Migrate_ExistingJPath_DoesNotChangePath()
        {
            const string path = "$.addresses[?(@.Country == 'Germany')].Street";
            var component = new JObject
            {
                ["$type"] = typeof(JPathSelectorFlowBlock).AssemblyQualifiedName,
                [nameof(JPathSelectorFlowBlock.Path)] = path
            };

            new FlowBloxComponentMigrationStrategy_1_0_0().Migrate(component);

            Assert.AreEqual(path, component.Value<string>(nameof(JPathSelectorFlowBlock.Path)));
        }

        [TestMethod]
        public void Migrate_LegacyJsonPathSelectorType_TranslatesLegacySyntax()
        {
            var component = new JObject
            {
                ["$type"] = "FlowBlox.Core.Models.FlowBlocks.Json.JsonPathSelectorFlowBlock, FlowBlox.Core",
                [nameof(JPathSelectorFlowBlock.Path)] = "participants/0/name"
            };

            new FlowBloxComponentMigrationStrategy_1_0_0().Migrate(component);

            Assert.AreEqual("$.participants[0].name", component.Value<string>(nameof(JPathSelectorFlowBlock.Path)));
        }

    }
}

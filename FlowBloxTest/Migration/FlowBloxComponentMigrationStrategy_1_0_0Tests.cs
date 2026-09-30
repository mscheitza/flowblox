using FlowBlox.Core.Migration.MigrationStrategies;
using FlowBlox.Core.Models.FlowBlocks.Json;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.Migration
{
    [TestClass]
    public class FlowBloxComponentMigrationStrategy_1_0_0Tests
    {
        [TestMethod]
        public void Migrate_JsonPathProperties_TranslatesLegacySyntax()
        {
            var cases = new[]
            {
                (Type: typeof(JsonPathSelectorFlowBlock), Property: nameof(JsonPathSelectorFlowBlock.Path), Legacy: "addresses/@Country=Germany/Street", Expected: "$.addresses[?(@.Country == 'Germany')].Street"),
                (Type: typeof(JsonObjectWriterFlowBlock), Property: nameof(JsonObjectWriterFlowBlock.Path), Legacy: "participants/0", Expected: "$.participants[0]"),
                (Type: typeof(JsonManyPathsSelectorMappingEntry), Property: nameof(JsonManyPathsSelectorMappingEntry.JsonPath), Legacy: "$/id", Expected: "$[*].id")
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
        public void Migrate_ExistingJPath_DoesNotChangePath()
        {
            const string path = "$.addresses[?(@.Country == 'Germany')].Street";
            var component = new JObject
            {
                ["$type"] = typeof(JsonPathSelectorFlowBlock).AssemblyQualifiedName,
                [nameof(JsonPathSelectorFlowBlock.Path)] = path
            };

            new FlowBloxComponentMigrationStrategy_1_0_0().Migrate(component);

            Assert.AreEqual(path, component.Value<string>(nameof(JsonPathSelectorFlowBlock.Path)));
        }
    }
}

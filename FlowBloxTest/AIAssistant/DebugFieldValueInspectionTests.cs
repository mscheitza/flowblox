using FlowBlox.AIAssistant.Tools.Handler;
using FlowBlox.AIAssistant.Tools.State;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class DebugFieldValueInspectionTests
    {
        [TestInitialize]
        public void Initialize()
        {
            AiAssistantDebugRunState.Set(new AiAssistantDebugRunSnapshot
            {
                RunId = "inspection-property-names",
                DebuggingResult = new JObject
                {
                    ["FieldValueChanges"] = new JArray(new JObject
                    {
                        ["Id"] = 1,
                        ["FullyQualifiedFieldName"] = "$Input::Content",
                        ["OldValue"] = "before",
                        ["NewValue"] = "after"
                    }),
                    ["GeneratedResults"] = new JArray(new JObject
                    {
                        ["Id"] = 2,
                        ["Datasets"] = new JArray(new JObject
                        {
                            ["DatasetIndex"] = 0,
                            ["FieldValueMappings"] = new JArray(
                                new JObject
                                {
                                    ["FullyQualifiedFieldName"] = "$ResultA::Content",
                                    ["Value"] = "first"
                                },
                                new JObject
                                {
                                    ["FullyQualifiedFieldName"] = "$ResultB::Other",
                                    ["Value"] = "second"
                                })
                        })
                    })
                }
            });
        }

        [TestMethod]
        public async Task DebugInspection_UsesCanonicalFullyQualifiedFieldName()
        {
            var artefact = await new GetLastDebugArtefactHandler().HandleAsync(
                new JObject { ["generatedResultId"] = 2 },
                CancellationToken.None);
            Assert.IsTrue(artefact.Ok, artefact.Error);
            var mapping = (JObject)artefact.Result["generatedResult"]!["Datasets"]![0]!["FieldValueMappings"]![0]!;
            Assert.AreEqual("$ResultA::Content", mapping.Value<string>("FullyQualifiedFieldName"));
            Assert.IsNull(mapping["FieldFullyQualifiedName"]);

            var fullyQualified = await new InspectFieldValueHandler().HandleAsync(
                new JObject
                {
                    ["generatedResultId"] = 2,
                    ["datasetIndex"] = 0,
                    ["fullyQualifiedFieldName"] = "$ResultA::Content"
                },
                CancellationToken.None);
            Assert.IsTrue(fullyQualified.Ok, fullyQualified.Error);
            Assert.AreEqual("first", fullyQualified.Result.Value<string>("value"));
            Assert.AreEqual("$ResultA::Content", fullyQualified.Result.Value<string>("fullyQualifiedFieldName"));

            var simple = await new InspectFieldValueHandler().HandleAsync(
                new JObject
                {
                    ["generatedResultId"] = 2,
                    ["datasetIndex"] = 0,
                    ["fieldName"] = "Other"
                },
                CancellationToken.None);
            Assert.IsTrue(simple.Ok, simple.Error);
            Assert.AreEqual("second", simple.Result.Value<string>("value"));

            var fieldChange = await new InspectFieldValueHandler().HandleAsync(
                new JObject { ["fieldChangeId"] = 1 },
                CancellationToken.None);
            Assert.IsTrue(fieldChange.Ok, fieldChange.Error);
            Assert.AreEqual("$Input::Content", fieldChange.Result.Value<string>("fullyQualifiedFieldName"));
            Assert.AreEqual("Content", fieldChange.Result.Value<string>("fieldName"));
        }
    }
}

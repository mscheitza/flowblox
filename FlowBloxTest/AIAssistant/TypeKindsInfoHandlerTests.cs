using FlowBlox.AIAssistant.Tools;
using FlowBlox.AIAssistant.Tools.Handler;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class TypeKindsInfoHandlerTests
    {
        [TestMethod]
        public async Task DerivedFlowBlockMetadata_ExposesActivationConditionTypes()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();

            var response = await new GetTypeKindsInfoHandler().HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(TableReaderFlowBlock).FullName,
                    ["includeAlreadySent"] = true
                },
                CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            var activationConditions = response.Result["kind"]?["properties"]?
                .Children<JObject>()
                .SingleOrDefault(x => x.Value<string>("name") == "ActivationConditions");

            Assert.IsNotNull(activationConditions);
            Assert.AreEqual("ReactiveObject", activationConditions.Value<string>("majorTypeDescriptor"));

            var creatableTypes = activationConditions["ui"]?["creatableTypes"]?.Values<string>().ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    typeof(FieldLogicalComparisonCondition).FullName,
                    typeof(LogicalGroupCondition).FullName
                },
                creatableTypes);

            var logicalConditionType = response.Result["kind"]?["usedTypes"]?
                .Children<JObject>()
                .SingleOrDefault(x => x.Value<string>("fullName") == typeof(LogicalCondition).FullName);
            Assert.IsNotNull(logicalConditionType);

            var supportedTypes = logicalConditionType["supportedTypes"]?
                .Children<JObject>()
                .Select(x => x.Value<string>("fullName"))
                .ToList();
            CollectionAssert.IsSubsetOf(
                new[]
                {
                    typeof(FieldLogicalComparisonCondition).FullName,
                    typeof(LogicalGroupCondition).FullName
                },
                supportedTypes);

            var propertyNames = response.Result["kind"]?["properties"]?
                .Children<JObject>()
                .Select(x => x.Value<string>("name"))
                .ToList();
            CollectionAssert.IsSubsetOf(
                new[]
                {
                    "Name",
                    "RequiredFields",
                    "InputIgnoreDuplicates",
                    "InputBehaviorAssignments",
                    "ActivationConditions",
                    "GenerationStrategies"
                },
                propertyNames);
        }

        [TestMethod]
        public async Task ComponentMetadata_HidesFrameworkOnlyPropertiesForManagedObjects()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();

            var response = await new GetTypeKindsInfoHandler().HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(SQLTable).FullName,
                    ["includeAlreadySent"] = true
                },
                CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            var propertyNames = response.Result["kind"]?["properties"]?
                .Children<JObject>()
                .Select(x => x.Value<string>("name"))
                .ToList();

            CollectionAssert.Contains(propertyNames, "Name");
            CollectionAssert.Contains(propertyNames, "RequiredFields");
            CollectionAssert.DoesNotContain(propertyNames, "HandleRequirements");
            CollectionAssert.DoesNotContain(propertyNames, "HasErrors");
            CollectionAssert.DoesNotContain(propertyNames, "Icon16");
            CollectionAssert.DoesNotContain(propertyNames, "Icon32");
            CollectionAssert.DoesNotContain(propertyNames, "Version");
        }
    }
}

using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.AIAssistant.Tools.Handler;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.FlowBlocks.TextOperations;
using FlowBlox.Core.Models.FlowBlocks.Web;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.Compression;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class TypeKindsInfoHandlerTests
    {
        [TestMethod]
        public void GenericNameHelper_CompactsNestedGenericTypesRecursively()
        {
            var compactName = AiAssistantGenericTypeNameHelper.GetCompactTypeName(
                typeof(Dictionary<string, ObservableCollection<FlowBlox.Core.Models.Components.FieldElement>>));

            Assert.AreEqual(
                "System.Collections.Generic.Dictionary<System.String,System.Collections.ObjectModel.ObservableCollection<FMC.FieldElement>>",
                compactName);
        }

        [TestMethod]
        [DataRow((int)UIOperations.None, "")]
        [DataRow((int)UIOperations.Create, "C")]
        [DataRow((int)UIOperations.Link, "L")]
        [DataRow((int)UIOperations.Unlink, "U")]
        [DataRow((int)UIOperations.Edit, "E")]
        [DataRow((int)UIOperations.Delete, "D")]
        [DataRow((int)UIOperations.All, "CLUED")]
        public void UiFlags_MapEveryOperation(int operations, string expected)
        {
            Assert.AreEqual(
                expected,
                ToolHandlerUtilities.BuildUiFlags((UIOperations)operations, (UIOptions)0));
        }

        [TestMethod]
        [DataRow(0, "")]
        [DataRow((int)UIOptions.EnableFieldSelection, "[FLD]")]
        [DataRow((int)UIOptions.EnableFileSelection, "[FL]")]
        [DataRow((int)UIOptions.EnableFolderSelection, "[DI]")]
        [DataRow(
            (int)(UIOptions.EnableFieldSelection | UIOptions.EnableFileSelection | UIOptions.EnableFolderSelection),
            "[FLD][FL][DI]")]
        public void UiFlags_MapEveryOption(int options, string expected)
        {
            Assert.AreEqual(
                expected,
                ToolHandlerUtilities.BuildUiFlags(UIOperations.None, (UIOptions)options));
        }

        [TestMethod]
        public void UiFlags_UseCanonicalOperationThenOptionOrder()
        {
            Assert.AreEqual(
                "CLUED[FLD][FL][DI]",
                ToolHandlerUtilities.BuildUiFlags(
                    UIOperations.All,
                    UIOptions.EnableFieldSelection |
                    UIOptions.EnableFileSelection |
                    UIOptions.EnableFolderSelection));
        }

        [TestMethod]
        public async Task TypeMetadata_CompactsUiFlagsForProductionTypes()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();
            var handler = new GetTypeKindsInfoHandler();

            var copyMoveResponse = await handler.HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(CopyMoveFlowBlock).FullName,
                    ["includeSummarized"] = true
                },
                CancellationToken.None);
            var openXmlResponse = await handler.HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(OpenXml2TextFlowBlock).FullName,
                    ["includeSummarized"] = true
                },
                CancellationToken.None);
            var tableMappingResponse = await handler.HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(TableSelectorMappingEntry).FullName,
                    ["includeSummarized"] = true
                },
                CancellationToken.None);

            Assert.IsTrue(copyMoveResponse.Ok, copyMoveResponse.Error);
            Assert.IsTrue(openXmlResponse.Ok, openXmlResponse.Error);
            Assert.IsTrue(tableMappingResponse.Ok, tableMappingResponse.Error);

            Assert.AreEqual("CLUED[FLD][FL]", FindProperty(copyMoveResponse, "SourceFile")?.Value<string>("uiFlags"));
            Assert.AreEqual("CLUED[FLD][DI]", FindProperty(copyMoveResponse, "SourceFolder")?.Value<string>("uiFlags"));
            Assert.AreEqual("CLUED[FLD][FL][DI]", FindProperty(copyMoveResponse, "DestinationPath")?.Value<string>("uiFlags"));
            Assert.AreEqual("LU", FindProperty(openXmlResponse, "WordDocumentField")?.Value<string>("uiFlags"));
            Assert.AreEqual("CED", FindProperty(tableMappingResponse, "Field")?.Value<string>("uiFlags"));
            Assert.AreEqual(
                "ActivationCondition(MemberName=\"Mode\";Values=[FM.FlowBlocks.IO.CopyMoveMode.CopyFile;FM.FlowBlocks.IO.CopyMoveMode.MoveFile]),ConditionallyRequired",
                FindProperty(copyMoveResponse, "SourceFile")?.Value<string>("attributes"));

            var serializedResponses = string.Concat(
                copyMoveResponse.Result.ToString(Newtonsoft.Json.Formatting.None),
                openXmlResponse.Result.ToString(Newtonsoft.Json.Formatting.None),
                tableMappingResponse.Result.ToString(Newtonsoft.Json.Formatting.None));
            Assert.IsFalse(serializedResponses.Contains("\"operations\"", StringComparison.Ordinal));
            Assert.IsFalse(serializedResponses.Contains("\"uiOptions\"", StringComparison.Ordinal));
        }

        [TestMethod]
        public async Task TypeMetadata_CompactsAttributesWithoutLosingArguments()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();

            var response = await new GetTypeKindsInfoHandler().HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(FormatFlowBlock).FullName,
                    ["includeSummarized"] = true
                },
                CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            var attributes = FindProperty(response, "FormatExpression")?.Value<string>("attributes");

            Assert.AreEqual(
                "CustomValidation(typeof(FM.FlowBlocks.TextOperations.FormatFlowBlock);\"ValidateFormatExpression\"),RequiredIf(\"AllowEmptyFormatExpression\";false)",
                attributes);
            Assert.IsFalse(response.Result.ToString().Contains("System.ComponentModel.DataAnnotations", StringComparison.Ordinal));
            Assert.AreEqual(JTokenType.String, FindProperty(response, "FormatExpression")?["attributes"]?.Type);
        }

        [TestMethod]
        public async Task TypeMetadata_PreservesExplicitNullableAttributeCompactly()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();

            var response = await new GetTypeKindsInfoHandler().HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(NullableMetadataFixture).FullName,
                    ["includeSummarized"] = true
                },
                CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            var attributes = FindProperty(response, nameof(NullableMetadataFixture.Values))?
                .Value<string>("attributes");

            StringAssert.StartsWith(attributes, "Nullable(");
            Assert.IsFalse(attributes.Contains("System.Runtime.CompilerServices", StringComparison.Ordinal));
            Assert.IsFalse(attributes.Contains("Attribute", StringComparison.Ordinal));
        }

        [TestMethod]
        public async Task DerivedFlowBlockMetadata_ExposesActivationConditionTypes()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();

            var response = await new GetTypeKindsInfoHandler().HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(TableReaderFlowBlock).FullName,
                    ["includeSummarized"] = true,
                    ["includeBaseMembers"] = true
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
                    Compact(typeof(FieldLogicalComparisonCondition)),
                    Compact(typeof(LogicalGroupCondition))
                },
                creatableTypes);

            var logicalConditionType = response.Result["kind"]?["usedTypes"]?
                .Children<JObject>()
                .SingleOrDefault(x => x.Value<string>("fullName") == Compact(typeof(LogicalCondition)));
            Assert.IsNotNull(logicalConditionType);

            var supportedTypes = logicalConditionType["supportedTypes"]?
                .Children<JObject>()
                .Select(x => x.Value<string>("fullName"))
                .ToList();
            CollectionAssert.IsSubsetOf(
                new[]
                {
                    Compact(typeof(FieldLogicalComparisonCondition)),
                    Compact(typeof(LogicalGroupCondition))
                },
                supportedTypes);

            var serializedResult = response.Result.ToString();
            Assert.IsFalse(serializedResult.Contains("FlowBlox.Core.Models.", StringComparison.Ordinal));
            Assert.IsFalse(serializedResult.Contains("Version=", StringComparison.Ordinal));
            Assert.IsFalse(serializedResult.Contains("PublicKeyToken=", StringComparison.Ordinal));
            StringAssert.Contains(serializedResult, "FMFB.");

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
                    "OutputIgnoreDuplicates",
                    "ActivationConditions",
                    "GenerationStrategies"
                },
                propertyNames);
        }

        [TestMethod]
        public async Task DerivedFlowBlockMetadata_ExcludesBaseMembersByDefaultAndIncludesThemOnRequest()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();

            var handler = new GetTypeKindsInfoHandler();
            var compactBaseResultType = Compact(typeof(FlowBlox.Core.Models.FlowBlocks.Base.BaseResultFlowBlock));
            var defaultResponse = await handler.HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(TableReaderFlowBlock).FullName,
                    ["includeSummarized"] = true
                },
                CancellationToken.None);

            Assert.IsTrue(defaultResponse.Ok, defaultResponse.Error);
            var defaultKind = defaultResponse.Result["kind"];
            var defaultPropertyNames = defaultKind?["properties"]?
                .Children<JObject>()
                .Select(x => x.Value<string>("name"))
                .ToList();

            CollectionAssert.Contains(defaultPropertyNames, "MappingEntries");
            CollectionAssert.Contains(defaultPropertyNames, "ReferencedTable");
            CollectionAssert.DoesNotContain(defaultPropertyNames, "OutputIgnoreDuplicates");
            CollectionAssert.DoesNotContain(defaultPropertyNames, "ActivationConditions");
            CollectionAssert.Contains(
                defaultKind?["inheritanceHierarchy"]?.Values<string>().ToList(),
                compactBaseResultType);

            var responseWithBaseMembers = await handler.HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(TableReaderFlowBlock).FullName,
                    ["includeSummarized"] = true,
                    ["includeBaseMembers"] = true
                },
                CancellationToken.None);

            Assert.IsTrue(responseWithBaseMembers.Ok, responseWithBaseMembers.Error);
            var propertyNamesWithBaseMembers = responseWithBaseMembers.Result["kind"]?["properties"]?
                .Children<JObject>()
                .Select(x => x.Value<string>("name"))
                .ToList();

            CollectionAssert.Contains(propertyNamesWithBaseMembers, "OutputIgnoreDuplicates");
            CollectionAssert.Contains(propertyNamesWithBaseMembers, "ActivationConditions");
            CollectionAssert.DoesNotContain(propertyNamesWithBaseMembers, "DefinedManagedObjects");
            CollectionAssert.DoesNotContain(propertyNamesWithBaseMembers, "Location");
            CollectionAssert.DoesNotContain(propertyNamesWithBaseMembers, "TestDefinitions");
        }

        [TestMethod]
        public async Task DerivedFlowBlockMetadata_IncludesIntermediateDomainBaseMembersByDefault()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();

            var response = await new GetTypeKindsInfoHandler().HandleAsync(
                new JObject
                {
                    ["typeFullName"] = typeof(WebEventFlowBlock).FullName,
                    ["includeSummarized"] = true
                },
                CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            var propertyNames = response.Result["kind"]?["properties"]?
                .Children<JObject>()
                .Select(x => x.Value<string>("name"))
                .ToList();

            CollectionAssert.Contains(propertyNames, "AssociatedWebBrowser");
            CollectionAssert.DoesNotContain(propertyNames, "OutputIgnoreDuplicates");
            CollectionAssert.DoesNotContain(propertyNames, "ResultField");
        }

        [TestMethod]
        public async Task PropertyDescriptionCaches_DistinguishLiveAndPreviouslySummarizedDetails()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            var sessionId = Guid.NewGuid().ToString("N");
            ToolHandlerUtilities.SetCurrentSessionGuid(sessionId);
            ToolHandlerUtilities.ClearPropertyDescriptionCache(sessionId);

            try
            {
                var handler = new GetTypeKindsInfoHandler();
                var args = new JObject
                {
                    ["typeFullName"] = typeof(TableReaderFlowBlock).FullName
                };

                var firstResponse = await handler.HandleAsync(args, CancellationToken.None);
                var repeatedLiveResponse = await handler.HandleAsync(
                    new JObject
                    {
                        ["typeFullName"] = typeof(TableReaderFlowBlock).FullName,
                        ["includeSummarized"] = true
                    },
                    CancellationToken.None);

                Assert.IsNotNull(FindProperty(firstResponse, "MappingEntries")?["type"]);
                Assert.IsNull(FindProperty(repeatedLiveResponse, "MappingEntries"));
                var repeatedLiveOmissionMessage = repeatedLiveResponse.Result["kind"]?["omittedPropertyDetails"]?.Value<string>();
                StringAssert.Contains(
                    repeatedLiveOmissionMessage,
                    "available message history");
                StringAssert.Contains(repeatedLiveOmissionMessage, "MappingEntries");
                Assert.AreEqual(0, repeatedLiveResponse.Result["kind"]?["properties"]?.Count());
                Assert.IsNull(repeatedLiveResponse.Result["kind"]?["rules"]);
                Assert.IsTrue(
                    repeatedLiveResponse.Result.ToString(Newtonsoft.Json.Formatting.None).Length < 1500,
                    "A fully cached kind-info response should remain compact.");

                ToolHandlerUtilities.ClearLivePropertyDescriptionCache(sessionId);
                var summarizedResponse = await handler.HandleAsync(args, CancellationToken.None);

                Assert.IsNull(FindProperty(summarizedResponse, "MappingEntries"));
                var summarizedOmissionMessage = summarizedResponse.Result["kind"]?["omittedPropertyDetails"]?.Value<string>();
                StringAssert.Contains(
                    summarizedOmissionMessage,
                    "earlier in this session");
                StringAssert.Contains(summarizedOmissionMessage, "MappingEntries");
                StringAssert.Contains(summarizedOmissionMessage, "includeSummarized=true");

                var explicitlyRepeatedResponse = await handler.HandleAsync(
                    new JObject
                    {
                        ["typeFullName"] = typeof(TableReaderFlowBlock).FullName,
                        ["includeSummarized"] = true
                    },
                    CancellationToken.None);

                Assert.IsNotNull(FindProperty(explicitlyRepeatedResponse, "MappingEntries")?["type"]);
                Assert.IsNull(explicitlyRepeatedResponse.Result["kind"]?["omittedPropertyDetails"]);
            }
            finally
            {
                ToolHandlerUtilities.ClearPropertyDescriptionCache(sessionId);
                ToolHandlerUtilities.SetCurrentSessionGuid(null);
            }
        }

        [TestMethod]
        public async Task AbstractFlowBlockBaseTypes_CanBeQueriedDirectly()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            ToolHandlerUtilities.ClearSessionCache();

            var baseTypes = new[]
            {
                typeof(FlowBlox.Core.Models.FlowBlocks.Base.BaseFlowBlock),
                typeof(FlowBlox.Core.Models.FlowBlocks.Base.BaseResultFlowBlock),
                typeof(FlowBlox.Core.Models.FlowBlocks.Base.BaseSingleResultFlowBlock),
                typeof(FlowBlox.Core.Models.FlowBlocks.Base.BasePipeFlowBlock),
                typeof(FlowBlox.Core.Models.FlowBlocks.Base.BaseMultiResultCollectorFlowBlock),
                typeof(FlowBlox.Core.Models.FlowBlocks.Base.BaseSingleResultCollectorFlowBlock)
            };

            var handler = new GetTypeKindsInfoHandler();
            foreach (var baseType in baseTypes)
            {
                Assert.IsTrue(baseType.IsAbstract, $"Test setup expects '{baseType.FullName}' to be abstract.");

                var response = await handler.HandleAsync(
                    new JObject
                    {
                        ["typeFullName"] = baseType.FullName,
                        ["includeSummarized"] = true
                    },
                    CancellationToken.None);

                Assert.IsTrue(response.Ok, $"{baseType.FullName}: {response.Error}");
                Assert.AreEqual(Compact(baseType), response.Result["kind"]?["fullName"]?.Value<string>());
            }
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
                    ["includeSummarized"] = true
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

        private static string Compact(Type type) =>
            AiAssistantTypeAliasHelper.CompressTypeName(type.FullName!);

        private static JObject? FindProperty(ToolResponse response, string propertyName) =>
            response.Result["kind"]?["properties"]?
                .Children<JObject>()
                .SingleOrDefault(x => x.Value<string>("name") == propertyName);

        private sealed class NullableMetadataFixture : FlowBloxReactiveObject
        {
            public Dictionary<string, string?> Values { get; set; } = [];
        }
    }
}

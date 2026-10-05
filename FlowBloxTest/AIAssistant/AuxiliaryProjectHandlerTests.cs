using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class AuxiliaryProjectHandlerTests
    {
        private readonly string _sessionId = Guid.NewGuid().ToString("N");
        private readonly string _projectName = "Schema inspection " + Guid.NewGuid().ToString("N");
        private const string ProjectDescription = "Inspects a database schema for the current assistant session.";

        [TestInitialize]
        public void Initialize()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject { ProjectName = "Main" };
            ToolHandlerUtilities.SetCurrentSessionGuid(_sessionId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            ToolHandlerUtilities.ClearSessionCache(_sessionId);
            ToolHandlerUtilities.SetCurrentSessionGuid(null);
            foreach (var projectName in new[]
                     {
                         _projectName,
                         _projectName + " renamed",
                         _projectName + " collision"
                     })
            {
                var projectFile = AuxiliaryProjectSessionStore.GetStoredProjectFile(_sessionId, projectName);
                foreach (var file in new[]
                         {
                             projectFile,
                             Path.ChangeExtension(projectFile, ".fbdeps"),
                             Path.ChangeExtension(projectFile, ".fblocaldata")
                         })
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
            }
        }

        [TestMethod]
        public async Task CreateSaveGetAndEdit_PersistsProjectAndSwitchesRegistryScope()
        {
            var api = new DefaultToolApi();

            var created = await Execute(api, "CreateOrUpdateAuxiliaryProject", new JObject
            {
                ["projectName"] = _projectName,
                ["description"] = ProjectDescription,
                ["mode"] = "metadata_only"
            });
            var editing = await Execute(api, "AuxiliaryProjectEditing", new JObject
            {
                ["method"] = "EDIT",
                ["projectName"] = _projectName
            });
            var flowBlock = await Execute(api, "CreateFlowBlock", new JObject
            {
                ["typeFullName"] = typeof(StartFlowBlock).FullName,
                ["name"] = "AuxStart",
                ["x"] = 50,
                ["y"] = 400
            });
            var auxiliaryMetadata = await Execute(api, "GetProjectMetadata", new JObject());

            Assert.IsTrue(created.Ok, created.Error);
            Assert.IsTrue(editing.Ok, editing.Error);
            Assert.IsTrue(flowBlock.Ok, flowBlock.Error);
            Assert.IsTrue(auxiliaryMetadata.Result.Value<bool>("isAuxiliaryProject"));
            Assert.AreEqual(_projectName, auxiliaryMetadata.Result.Value<string>("projectName"));
            Assert.AreEqual(ProjectDescription, auxiliaryMetadata.Result.Value<string>("description"));
            Assert.AreEqual(0, FlowBloxProjectManager.Instance.ActiveProject.FlowBloxRegistry.GetFlowBlocks().Count());
            Assert.IsTrue(File.Exists(AuxiliaryProjectSessionStore.GetStoredProjectFile(_sessionId, _projectName)));

            var saved = await Execute(api, "AuxiliaryProjectEditing", new JObject
            {
                ["method"] = "SAVE",
                ["projectName"] = _projectName
            });
            var projects = await Execute(api, "GetAuxiliaryProjects", new JObject());
            var mainMetadata = await Execute(api, "GetProjectMetadata", new JObject());

            Assert.IsTrue(saved.Ok, saved.Error);
            Assert.IsTrue(saved.Result.Value<bool>("closed"));
            Assert.IsTrue(File.Exists(saved.Result.Value<string>("projectFile")));
            var discoveredProject = projects.Result["projects"]!.Single(x =>
                string.Equals(x!.Value<string>("projectName"), _projectName, StringComparison.Ordinal));
            Assert.AreEqual(ProjectDescription, discoveredProject!.Value<string>("description"));
            Assert.IsTrue(File.Exists(discoveredProject.Value<string>("projectFile")));
            StringAssert.Contains(File.ReadAllText(saved.Result.Value<string>("projectFile")), "AuxStart");
            Assert.IsFalse(mainMetadata.Result.Value<bool>("isAuxiliaryProject"));
            Assert.AreEqual("Main", mainMetadata.Result.Value<string>("projectName"));

            var secondSessionId = Guid.NewGuid().ToString("N");
            ToolHandlerUtilities.SetCurrentSessionGuid(secondSessionId);
            var projectsFromAnotherSession = await Execute(api, "GetAuxiliaryProjects", new JObject());
            Assert.IsFalse(projectsFromAnotherSession.Result["projects"]!.Any(x =>
                string.Equals(x!.Value<string>("projectName"), _projectName, StringComparison.Ordinal)));
            ToolHandlerUtilities.SetCurrentSessionGuid(_sessionId);

            var reopened = await Execute(api, "AuxiliaryProjectEditing", new JObject
            {
                ["method"] = "EDIT",
                ["projectName"] = _projectName
            });
            var editingMetadata = await Execute(api, "GetProjectMetadata", new JObject());
            var closed = await Execute(api, "AuxiliaryProjectEditing", new JObject { ["method"] = "CLOSE" });

            Assert.IsTrue(reopened.Ok, reopened.Error);
            Assert.IsTrue(editingMetadata.Result.Value<bool>("isAuxiliaryProject"));
            Assert.AreEqual(_projectName, editingMetadata.Result.Value<string>("projectName"));
            Assert.IsTrue(closed.Ok, closed.Error);
        }

        [TestMethod]
        public async Task Create_CopiesDistributedProject_AndUpdateRequiresClosedProject()
        {
            var api = new DefaultToolApi();
            var createWithoutDescription = await Execute(api, "CreateOrUpdateAuxiliaryProject", new JObject
            {
                ["projectName"] = _projectName + " without description",
                ["mode"] = "metadata_only"
            });
            var createResponse = await Execute(api, "CreateOrUpdateAuxiliaryProject", new JObject
            {
                ["projectName"] = _projectName,
                ["mode"] = "full_content",
                ["copyFromDistributedData"] = new JObject
                {
                    ["dataType"] = "auxiliary_project",
                    ["fileName"] = "Web-Search.fbprj"
                }
            });
            var opened = await Execute(api, "AuxiliaryProjectEditing", new JObject
            {
                ["method"] = "EDIT",
                ["projectName"] = _projectName
            });
            var updateWhileOpen = await Execute(api, "CreateOrUpdateAuxiliaryProject", new JObject
            {
                ["projectName"] = _projectName,
                ["mode"] = "full_content",
                ["projectJson"] = "{\"ProjectDescription\":\"Updated\",\"FlowBlocks\":[],\"ManagedObjects\":[],\"UserFields\":[],\"ProjectDependendDataObjects\":[],\"InputFiles\":[]}"
            });
            var saved = await Execute(api, "AuxiliaryProjectEditing", new JObject
            {
                ["method"] = "SAVE",
                ["projectName"] = _projectName
            });
            var updateAfterClose = await Execute(api, "CreateOrUpdateAuxiliaryProject", new JObject
            {
                ["projectName"] = _projectName,
                ["mode"] = "full_content",
                ["projectJson"] = "{\"ProjectDescription\":\"Updated\",\"FlowBlocks\":[],\"ManagedObjects\":[],\"UserFields\":[],\"ProjectDependendDataObjects\":[],\"InputFiles\":[]}"
            });

            Assert.IsFalse(createWithoutDescription.Ok);
            StringAssert.Contains(createWithoutDescription.Error, "description is required for metadata_only");
            Assert.IsTrue(createResponse.Ok, createResponse.Error);
            Assert.AreEqual("Runs a web search and returns structured search results.", createResponse.Result.Value<string>("description"));
            Assert.IsTrue(opened.Ok, opened.Error);
            Assert.IsFalse(updateWhileOpen.Ok);
            StringAssert.Contains(updateWhileOpen.Error, "not allowed while an auxiliary project is open");
            Assert.IsTrue(saved.Ok, saved.Error);
            Assert.IsTrue(updateAfterClose.Ok, updateAfterClose.Error);
        }

        [TestMethod]
        public async Task MetadataUpdate_RenamesProjectFileAndRejectsExistingTarget()
        {
            var api = new DefaultToolApi();
            var collisionName = _projectName + " collision";
            var renamedName = _projectName + " renamed";
            foreach (var projectName in new[] { _projectName, collisionName })
            {
                var created = await Execute(api, "CreateOrUpdateAuxiliaryProject", new JObject
                {
                    ["projectName"] = projectName,
                    ["description"] = ProjectDescription,
                    ["mode"] = "metadata_only"
                });
                Assert.IsTrue(created.Ok, created.Error);
            }

            var collision = await Execute(api, "CreateOrUpdateAuxiliaryProject", new JObject
            {
                ["existingProjectName"] = _projectName,
                ["projectName"] = collisionName,
                ["description"] = "Renamed project.",
                ["mode"] = "metadata_only"
            });
            var renamed = await Execute(api, "CreateOrUpdateAuxiliaryProject", new JObject
            {
                ["existingProjectName"] = _projectName,
                ["projectName"] = renamedName,
                ["description"] = "Renamed project.",
                ["mode"] = "metadata_only"
            });

            Assert.IsFalse(collision.Ok);
            StringAssert.Contains(collision.Error, "already exists in the current session");
            Assert.IsTrue(renamed.Ok, renamed.Error);
            Assert.IsFalse(File.Exists(AuxiliaryProjectSessionStore.GetStoredProjectFile(_sessionId, _projectName)));
            Assert.IsTrue(File.Exists(AuxiliaryProjectSessionStore.GetStoredProjectFile(_sessionId, renamedName)));
            Assert.AreEqual(renamedName, renamed.Result.Value<string>("projectName"));
        }

        private static Task<ToolResponse> Execute(DefaultToolApi api, string toolName, JObject arguments) =>
            api.ExecuteAsync(new ToolRequest { ToolName = toolName, Arguments = arguments }, CancellationToken.None);
    }
}

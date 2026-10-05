using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class RunProjectDebugTestHandlerTests
    {
        [TestMethod]
        public async Task Confirmation_IsRequiredForAuxiliaryAndExplicitExternalProjects()
        {
            var previousProject = FlowBloxProjectManager.Instance.ActiveProject;
            var localAppDataOption = FlowBloxOptions.GetOptionInstance().GetOption("Paths.LocalAppDataDir");
            var previousLocalAppDataDirectory = localAppDataOption.PersistentValue;
            var testDirectory = Path.Combine(Path.GetTempPath(), "FlowBloxTest", Guid.NewGuid().ToString("N"));
            var sessionId = Guid.NewGuid().ToString("N");
            localAppDataOption.PersistentValue = testDirectory;
            ToolHandlerUtilities.SetCurrentSessionGuid(sessionId);
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject { ProjectName = "Current" };
            var confirmationRequests = 0;
            var api = new DefaultToolApi
            {
                ToolExecutionConfirmationCallback = _ =>
                {
                    confirmationRequests++;
                    return false;
                }
            };

            try
            {
                var currentProjectResponse = await api.ExecuteAsync(
                    new ToolRequest
                    {
                        ToolName = "RunProjectDebugTest",
                        Arguments = new JObject { ["targetFlowBlockName"] = "Missing target" }
                    },
                    CancellationToken.None);
                Assert.AreEqual(0, confirmationRequests);
                Assert.IsFalse(currentProjectResponse.Result.Value<bool?>("cancelledByUser") == true);

                AuxiliaryProjectSessionStore.CreateOrUpdateFile(
                    sessionId,
                    "Auxiliary",
                    new FlowBloxProject
                    {
                        ProjectName = "Auxiliary",
                        ProjectDescription = "Security confirmation test"
                    });
                AuxiliaryProjectSessionStore.Edit(sessionId, "Auxiliary");

                var auxiliaryProjectResponse = await api.ExecuteAsync(
                    new ToolRequest
                    {
                        ToolName = "RunProjectDebugTest",
                        Arguments = new JObject { ["reason"] = "Inspect auxiliary data" }
                    },
                    CancellationToken.None);
                Assert.AreEqual(1, confirmationRequests);
                Assert.IsTrue(auxiliaryProjectResponse.Result.Value<bool>("cancelledByUser"));
                AuxiliaryProjectSessionStore.Close(sessionId);

                var externalProjectResponse = await api.ExecuteAsync(
                    new ToolRequest
                    {
                        ToolName = "RunProjectDebugTest",
                        Arguments = new JObject
                        {
                            ["projectFile"] = "external-project.fbprj",
                            ["reason"] = "Inspect external data"
                        }
                    },
                    CancellationToken.None);
                Assert.AreEqual(2, confirmationRequests);
                Assert.IsTrue(externalProjectResponse.Result.Value<bool>("cancelledByUser"));
            }
            finally
            {
                AuxiliaryProjectSessionStore.Clear(sessionId);
                ToolHandlerUtilities.SetCurrentSessionGuid(null);
                FlowBloxProjectManager.Instance.ActiveProject = previousProject;
                localAppDataOption.PersistentValue = previousLocalAppDataDirectory;
                if (Directory.Exists(testDirectory))
                    Directory.Delete(testDirectory, recursive: true);
            }
        }
    }
}

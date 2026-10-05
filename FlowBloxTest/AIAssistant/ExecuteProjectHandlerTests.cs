using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class ExecuteProjectHandlerTests
    {
        [TestMethod]
        public async Task ExecuteProject_IsRejectedBeforeRunnerAccessWithoutUserApproval()
        {
            var confirmationRequested = false;
            var api = new DefaultToolApi
            {
                ToolExecutionConfirmationCallback = request =>
                {
                    confirmationRequested = request.ToolName == "ExecuteProject";
                    return false;
                }
            };

            var response = await api.ExecuteAsync(
                new ToolRequest
                {
                    ToolName = "ExecuteProject",
                    Arguments = new JObject
                    {
                        ["projectFile"] = "untrusted-external-project.fbprj",
                        ["reason"] = "Read external information"
                    }
                },
                CancellationToken.None);

            Assert.IsTrue(confirmationRequested);
            Assert.IsFalse(response.Ok);
            Assert.IsTrue(response.Result.Value<bool>("cancelledByUser"));
        }

        [TestMethod]
        public async Task InspectOutputFieldValue_ReadsFullOutputFromSessionScopedProjectRun()
        {
            var sessionId = Guid.NewGuid().ToString("N");
            ToolHandlerUtilities.SetCurrentSessionGuid(sessionId);
            try
            {
                AiAssistantProjectRunState.Set(sessionId, new AiAssistantProjectRunSnapshot
                {
                    RunId = "project-run",
                    ProjectName = "Web lookup",
                    Outputs = new JObject
                    {
                        ["PageOutput"] = new JArray
                        {
                            new JObject
                            {
                                ["values"] = new JObject
                                {
                                    ["Content"] = "prefix TARGET suffix"
                                }
                            }
                        }
                    }
                });

                var response = await new DefaultToolApi().ExecuteAsync(
                    new ToolRequest
                    {
                        ToolName = "InspectOutputFieldValue",
                        Arguments = new JObject
                        {
                            ["runId"] = "project-run",
                            ["outputName"] = "PageOutput",
                            ["datasetIndex"] = 0,
                            ["fieldName"] = "Content",
                            ["searchValues"] = "TARGET",
                            ["searchMode"] = "LookAround"
                        }
                    },
                    CancellationToken.None);

                Assert.IsTrue(response.Ok, response.Error);
                Assert.AreEqual("TARGET", response.Result["valueInfo"]!.Value<string>("matchedSearchValue"));
                StringAssert.Contains(response.Result.Value<string>("value"), "TARGET");
            }
            finally
            {
                ToolHandlerUtilities.ClearSessionCache(sessionId);
                ToolHandlerUtilities.SetCurrentSessionGuid(null);
            }
        }
    }
}

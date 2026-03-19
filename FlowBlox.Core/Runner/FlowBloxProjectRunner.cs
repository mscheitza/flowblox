using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;
using FlowBlox.Core.Runner.Contracts;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.Authentication;

namespace FlowBlox.Core.Runner
{
    public static class FlowBloxProjectRunner
    {
        public static event Action<RunnerLogMessage> LogMessageCreated;

        private static void Report(string message, FlowBloxLogLevel level = FlowBloxLogLevel.Info)
        {
            LogMessageCreated?.Invoke(new RunnerLogMessage
            {
                UtcTimestamp = DateTime.UtcNow,
                LogLevel = level,
                Message = message
            });
        }

        private static async Task<FlowBloxProject> OpenProjectFromProjectSpaceAsync(
            string projectGuid,
            int? projectVersion,
            CancellationToken cancellationToken = default)
        {
            // Auto login
            var flowBloxAutoLoginExecutor = new FlowBloxAutoLoginExecutor();
            await flowBloxAutoLoginExecutor.TryAutoLoginAsync().ConfigureAwait(false);

            // Build Web API client and token
            var baseUrl = FlowBloxOptions.GetOptionInstance()
                .OptionCollection["Api.ProjectServiceBaseUrl"]
                .Value;

            var webApi = new FlowBloxWebApiService(baseUrl);
            var token = FlowBloxAccountManager.Instance.GetUserToken(baseUrl);

            // Load the project from ProjectSpace
            return await FlowBloxProject.FromProjectSpaceGuidAsync(
                    projectGuid,
                    projectVersion,
                    token,
                    webApi)
                .ConfigureAwait(false);
        }

        private static FlowBloxProject OpenProjectFromProjectSpace(string projectGuid, int? projectVersion)
        {
            return OpenProjectFromProjectSpaceAsync(projectGuid, projectVersion)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Runs a FlowBlox project in-process. Use RunnerHost for out-of-process sandbox execution.
        /// </summary>
        public static RunnerResponse Run(RunnerRequest request)
        {
            var response = new RunnerResponse
            {
                StartedUtc = DateTime.UtcNow
            };

            FlowBloxRuntime runtime = null;

            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                var hasProjectSpace = !string.IsNullOrWhiteSpace(request.ProjectSpaceGuid);
                var hasProjectFile = !string.IsNullOrWhiteSpace(request.ProjectFile);

                if (!hasProjectSpace && !hasProjectFile)
                    throw new ArgumentException("Either ProjectFile or ProjectSpaceGuid must be provided.", nameof(request));

                FlowBloxProject project;

                if (hasProjectSpace)
                {
                    if (request.ProjectSpaceVersion.HasValue)
                        Report($"Loading project from ProjectSpaceGuid '{request.ProjectSpaceGuid}' and Version {request.ProjectSpaceVersion}...");
                    else
                        Report($"Loading project from ProjectSpaceGuid '{request.ProjectSpaceGuid}'...");

                    project = OpenProjectFromProjectSpace(request.ProjectSpaceGuid, request.ProjectSpaceVersion);
                }
                else
                {
                    Report($"Loading project from '{request.ProjectFile}'...");
                    project = FlowBloxProject.FromFile(request.ProjectFile);
                }

                if (project == null)
                    throw new InvalidOperationException("Project could not be loaded.");

                FlowBloxProjectManager.Instance.ActiveProject = project;
                response.ProjectName = project.ProjectName;

                // Ensure options are initialized, then apply overrides.
                var flowBloxOptions = FlowBloxOptions.GetOptionInstance();
                flowBloxOptions.InitDefaults(false);
                ApplyOptionOverrides(flowBloxOptions, request.OptionOverrides);

                // Apply user field overrides (inputs).
                ApplyUserFieldOverrides(request.UserFields);

                // Create runtime.
                runtime = new FlowBloxRuntime(project)
                {
                    IsNoDesignerMode = true,

                    // Note: ExecuteProjectFlowBlock/RunnerHost may force this to false before calling the runner.
                    AutoRestart = request.AutoRestart
                };

                response.LogfilePath = runtime.GetLogfilePath();
                Report($"Logfile path: {response.LogfilePath}");

                // Forward runtime logs and optionally abort on warnings/errors.
                string abortReason = null;

                runtime.LogMessageCreated += (rt, msg, level) =>
                {
                    Report(msg, level);

                    if (abortReason == null)
                    {
                        if (request.AbortOnError && level == FlowBloxLogLevel.Error)
                            abortReason = "Aborted due to an error log message.";

                        if (request.AbortOnWarning && level == FlowBloxLogLevel.Warning)
                            abortReason = "Aborted due to a warning log message.";
                    }

                    if (request.AbortOnError && level == FlowBloxLogLevel.Error)
                        rt.Aborted = true;

                    if (request.AbortOnWarning && level == FlowBloxLogLevel.Warning)
                        rt.Aborted = true;
                };

                Report($"Execution of project '{project.ProjectName}' started...");

                // Execute synchronously.
                runtime.Execute();

                // If aborted, return aborted response.
                if (runtime.Aborted)
                {
                    response.Success = false;
                    response.ExitCode = RunnerExitCodes.Aborted;
                    response.ErrorMessage = abortReason ?? "Execution aborted.";
                    Report(response.ErrorMessage, FlowBloxLogLevel.Warning);
                    return response;
                }

                // Collect outputs (values only).
                var outputs = runtime.GetAllOutputs();
                foreach (var outputKvp in outputs)
                {
                    var list = new List<ProjectOutputDatasetDto>();

                    foreach (var ds in outputKvp.Value)
                    {
                        list.Add(new ProjectOutputDatasetDto
                        {
                            OutputName = ds.OutputName,
                            CreatedUtc = ds.CreatedUtc,
                            Values = ds.Values ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                        });
                    }

                    response.Outputs[outputKvp.Key] = list;
                }

                response.Success = true;
                response.ExitCode = RunnerExitCodes.Success;

                Report($"Execution of project '{project.ProjectName}' finished.", FlowBloxLogLevel.Info);
                return response;
            }
            catch (InvalidOperationException ex)
            {
                // Often used for FlowBlox validation errors
                response.Success = false;
                response.ExitCode = RunnerExitCodes.ValidationError;
                response.ErrorMessage = ex.Message;
                response.Exception = ex.ToString();

                Report(ex.Message, FlowBloxLogLevel.Error);
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ExitCode = RunnerExitCodes.RuntimeError;
                response.ErrorMessage = ex.Message;
                response.Exception = ex.ToString();

                Report(ex.Message, FlowBloxLogLevel.Error);
                return response;
            }
            finally
            {
                response.FinishedUtc = DateTime.UtcNow;
            }
        }


        private static void ApplyOptionOverrides(FlowBloxOptions flowBloxOptions, Dictionary<string, string> overrides)
        {
            if (overrides == null || overrides.Count == 0)
                return;

            foreach (var kvp in overrides)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                var opt = flowBloxOptions.GetOption(key);
                if (opt == null)
                {
                    Report($"Option override ignored: '{key}' not found.", FlowBloxLogLevel.Warning);
                    continue;
                }

                opt.Value = value;
                Report($"Option '{key}' overridden with '{value}'.", FlowBloxLogLevel.Info);
            }
        }

        private static void ApplyUserFieldOverrides(Dictionary<string, string> userFields)
        {
            userFields ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var fieldElement in FlowBloxRegistryProvider.GetRegistry().GetUserFields(UserFieldTypes.Input))
            {
                if (userFields.TryGetValue(fieldElement.Name, out var stringValue))
                {
                    fieldElement.StringValue = stringValue;
                    Report($"User field '{fieldElement.Name}' set to '{stringValue}'.", FlowBloxLogLevel.Info);
                }
                else
                {
                    // Keep project-defined value; log if empty
                    if (string.IsNullOrEmpty(fieldElement.StringValue))
                        Report($"No value provided for input field '{fieldElement.Name}'.", FlowBloxLogLevel.Warning);
                    else
                        Report($"No value provided for input field '{fieldElement.Name}', using current value '{fieldElement.StringValue}'.", FlowBloxLogLevel.Info);
                }
            }
        }
    }
}
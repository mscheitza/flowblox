using FlowBlox.Core.Runner;
using FlowBlox.Core.Runner.Contracts;
using FlowBlox.Core.Runner.Serialization;
using FlowBlox.WorkerService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowBlox.WorkerService
{
    public sealed class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptionsMonitor<FlowBloxServiceOptions> _options;

        public Worker(ILogger<Worker> logger, IOptionsMonitor<FlowBloxServiceOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe to runner logs and forward into service logging.
            void OnRunnerLog(RunnerLogMessage msg)
            {
                switch (msg.LogLevel)
                {
                    case FlowBlox.Core.Enums.FlowBloxLogLevel.Error:
                        _logger.LogError("[Runner] {Message}", msg.Message);
                        break;
                    case FlowBlox.Core.Enums.FlowBloxLogLevel.Warning:
                        _logger.LogWarning("[Runner] {Message}", msg.Message);
                        break;
                    default:
                        _logger.LogInformation("[Runner] {Message}", msg.Message);
                        break;
                }
            }

            FlowBloxProjectRunner.LogMessageCreated += OnRunnerLog;

            try
            {
                var cfg = _options.CurrentValue;

                if (!cfg.RunOnStart)
                {
                    _logger.LogInformation("RunOnStart is disabled. Service will stay idle.");
                    while (!stoppingToken.IsCancellationRequested)
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    return;
                }

                do
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    var request = BuildRunnerRequest(cfg);
                    _logger.LogInformation("Starting FlowBlox execution for project: {ProjectFile}", request.ProjectFile);

                    RunnerResponse response;
                    try
                    {
                        response = FlowBloxProjectRunner.Run(request);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception during runner execution.");
                        response = new RunnerResponse
                        {
                            Success = false,
                            ExitCode = RunnerExitCodes.RuntimeError,
                            ErrorMessage = ex.Message,
                            Exception = ex.ToString(),
                            StartedUtc = DateTime.UtcNow,
                            FinishedUtc = DateTime.UtcNow
                        };
                    }

                    // Write response JSON
                    if (!string.IsNullOrWhiteSpace(cfg.OutputFile))
                    {
                        try
                        {
                            var writtenTo = RunnerJson.WriteFileResolved(cfg.OutputFile!, response,
                                new RunnerPathTemplateContext { ProjectName = response.ProjectName });

                            _logger.LogInformation("Runner response written to: {OutputFile}", writtenTo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to write output file template: {OutputFile}", cfg.OutputFile);
                        }
                    }

                    if (response.Success)
                    {
                        _logger.LogInformation("Execution finished successfully (ExitCode={ExitCode}).", response.ExitCode);
                    }
                    else
                    {
                        _logger.LogError("Execution failed (ExitCode={ExitCode}). {Error}", response.ExitCode, response.ErrorMessage);
                    }

                    // Restart loop?
                    if (!cfg.Restart)
                        break;

                    var delay = TimeSpan.FromSeconds(Math.Max(1, cfg.RestartDelaySeconds));
                    _logger.LogInformation("Restart enabled. Next run in {Seconds} seconds...", delay.TotalSeconds);

                    await Task.Delay(delay, stoppingToken);

                } while (!stoppingToken.IsCancellationRequested);
            }
            finally
            {
                FlowBloxProjectRunner.LogMessageCreated -= OnRunnerLog;
            }
        }

        private static RunnerRequest BuildRunnerRequest(FlowBloxServiceOptions cfg)
        {
            if (cfg.ProjectFile == null)
                throw new InvalidOperationException("ProjectFile must be configured.");

            return new RunnerRequest
            {
                ProjectFile = RunnerPathTemplateResolver.Resolve(cfg.ProjectFile),

                AutoRestart = cfg.Restart,
                AbortOnError = cfg.AbortOnError,
                AbortOnWarning = cfg.AbortOnWarning,

                UserFields = cfg.UserFields ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                OptionOverrides = cfg.OptionOverrides ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
        }
    }
}

using System;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util;
using FlowBlox.Core.Provider;
using System.Collections.Generic;
using System.Linq;
using FlowBlox.Core.Exceptions;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Enums;
using Microsoft.Win32;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Logging;

namespace FlowBlox.Core.Models.Runtime
{
    public class FlowBloxRuntime : BaseRuntime
    {
        public bool IsNoDesignerMode { get; set; }

        public FlowBloxRuntime(FlowBloxProject project) : base(project)
        {
            this.StepTimeunit = FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StepTimeunit"].GetValueInt();
            this.StepwiseExecution = FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StepwiseExecution"].Value.ToLower().Equals("true");
            this.StopOnWarning = FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StopOnWarning"].Value.ToLower().Equals("true");
            this.StopOnError = FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StopOnError"].Value.ToLower().Equals("true");
            this.AutoRestart = FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.AutoRestart"].Value.ToLower().Equals("true");
        }

        private readonly HashSet<BaseFlowBlock> _approvedFlowBlocks = new HashSet<BaseFlowBlock>();

        private void ApproveFlowBlocks(BaseFlowBlock flowBlock)
        {
            if (flowBlock.ReferencedFlowBlocks.All(refBlock => _approvedFlowBlocks.Contains(refBlock)))
            {
                if (!_approvedFlowBlocks.Contains(flowBlock))
                {
                    // TODO: Es wird ein Test übergreifender Kontext für bereits ausgeführte Testschritte benötigt!!!

                    if (flowBlock.GenerationStrategies.Any())
                    {
                        var strategyExecutor = new FlowBlockGenerationStrategyExecutor(flowBlock);
                        if (!strategyExecutor.ExecuteGeneration())
                        {
                            this.Report($"The generation strategies for flow block \"{flowBlock.Name}\" could not be completed successfully.");
                        }
                    }

                    List<FlowBloxTestDefinition> failedTests = new List<FlowBloxTestDefinition>();
                    foreach (var testDefinition in flowBlock.TestDefinitions.Where(x => x.RequiredForExecution))
                    {
                        var testExecutor = new FlowBloxTestExecutor();
                        testExecutor.Initialize(testDefinition, flowBlock);

                        var testResult = testExecutor.ExecuteTest();
                        if (!testResult.Success)
                            failedTests.Add(testDefinition);
                    }

                    if (failedTests.Any())
                    {
                        this.Report("The following test cases required for execution were not passed: " + string.Join(", ", failedTests.Select(x => x.Name)));
                    }
                    else
                    {
                        _approvedFlowBlocks.Add(flowBlock);
                        flowBlock.GetNextFlowBlocks().ForEach(x => ApproveFlowBlocks(x));
                    }
                }
            }
        }

        protected override void OnBeforeRuntimeStarted(BaseFlowBlock startFlowBlock, IEnumerable<BaseFlowBlock> flowBlocks, IEnumerable<IManagedObject> managedObjects)
        {
            ApproveFlowBlocks(startFlowBlock);

            base.OnBeforeRuntimeStarted(startFlowBlock, flowBlocks, managedObjects);
        }

        public override void HandlePause()
        {
            if (!IsNoDesignerMode)
            {
                bool pauseContinue = false;
                if (this.Pause)
                {
                    pauseContinue = true;
                    this.RaisePauseContinue(this.Pause);
                }

                while (this.Pause && !this.Aborted)
                {
                    Thread.Sleep(1000);
                }

                if (this.Aborted)
                {
                    this.Report("The execution has been cancelled by user.");
                    throw new RuntimeCancellationException();
                }

                if (pauseContinue)
                    this.RaisePauseContinue(this.Pause);
            }
            
            base.HandlePause();
        }

        private static readonly HashSet<string> _alreadyCreatedLogfileNames = new HashSet<string>();
        private static readonly object _logfileLock = new object();
        private string _runtimeLogFileName;

        public string RuntimeLogFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_runtimeLogFileName))
                {
                    var baseName = string.Join("_",
                        Started.ToString("dd-MM-yyyy_HH-mm-ss"),
                        IOUtil.GetValidFileName(Project.ProjectName),
                        "runtime");

                    _runtimeLogFileName = baseName;

                    lock (_logfileLock)
                    {
                        var runtimeLogDirectory = FlowBloxOptions.GetOptionInstance().GetOption("General.RuntimeLogDir").Value;
                        if (_alreadyCreatedLogfileNames.Contains(_runtimeLogFileName) ||
                            File.Exists(Path.Combine(runtimeLogDirectory, _runtimeLogFileName)))
                        {
                            int suffix = 1;
                            while (_alreadyCreatedLogfileNames.Contains(_runtimeLogFileName + $"_{suffix}") ||
                                File.Exists(Path.Combine(runtimeLogDirectory, _runtimeLogFileName + $"_{suffix}")))
                            {
                                suffix++;
                            }
                            _runtimeLogFileName += $"_{suffix}";
                        }
                        _alreadyCreatedLogfileNames.Add(_runtimeLogFileName);
                    }
                }
                return _runtimeLogFileName;
            }
        }

        public string GetLogfilePath() => _logger?.GetLogfilePath();

        private ILogger _logger;
        public override void Report(string message, FlowBloxLogLevel logLevel = FlowBloxLogLevel.Info, Exception e = null)
        {
            if (_logger == null)
                _logger = new FlowBloxRuntimeLogger(this.RuntimeLogFileName);

            if (logLevel == FlowBloxLogLevel.Info)
                _logger.Info(message);
            else if (logLevel == FlowBloxLogLevel.Warning)
                _logger.Warn(message);
            else if (logLevel == FlowBloxLogLevel.Error)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _logger.Error(message, e);
                }
                else if (e != null)
                {
                    _logger.Exception(e);
                }
            }

            base.Report(message, logLevel, e);
        }

        public void Execute()
        {
            try
            {
                this.Running = true;
                this.OnBeforeRuntimeStarted();
                this.NotifyRuntimeStarted();
                this.Report("StepwiseExecution: " + StepwiseExecution.ToString().ToLower());
                this.Report("StepTimeunit: " + StepTimeunit.ToString());
                this.Report("AutoRestart: " + AutoRestart.ToString());

                int executionCounter = 0;
                do
                {
                    if (executionCounter == 0)
                    {
                        this.Report("Runtime execution has been started.");
                    }
                    else
                    {
                        this.Report($"Execution has been restarted. Number of executions: {executionCounter + 1}");
                    }
                    StartFlowBlock.Execute(this, null);
                    executionCounter++;
                }
                while (!Aborted && AutoRestart);

                this.Report("Runtime execution has finished.");
                this.Running = false;
                this.OnAfterRuntimeFinished();
                this.NotifyRuntimeFinished();
            }
            catch(RuntimeCancellationException e)
            {
                this.Running = false;
                this.OnAfterRuntimeFinished();
                this.NotifyRuntimeFinished();
            }
            catch (Exception e)
            {
                this.Running = false;
                this.Report("Runtime execution has aborted due to an critical error.", FlowBloxLogLevel.Error, e);
                this.OnAfterRuntimeFinished();
                this.NotifyRuntimeFinished();
            }
        }
    }
}

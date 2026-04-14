using System.Diagnostics;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime.Debugging;
using FlowBlox.Core.Util;

namespace FlowBlox.Core.Interceptors
{
    public sealed class RuntimeDebuggingInterceptor : RuntimeInterceptorBase
    {
        private readonly object _sync = new object();
        private Stopwatch _stopwatch;
        private RuntimeDebuggingResult _result;
        private int _nextFieldChangeId;

        private bool IsEnabled => Runtime?.ExternalDebuggingInformation != null;

        public override void NotifyBeforeRuntimeStarted()
        {
            if (!IsEnabled)
                return;

            var config = Runtime.ExternalDebuggingInformation;
            _stopwatch = Stopwatch.StartNew();
            _nextFieldChangeId = 0;

            _result = new RuntimeDebuggingResult
            {
                ProjectName = Runtime.Project?.ProjectName ?? string.Empty,
                StartedUtc = DateTime.UtcNow,
                TargetFlowBlockName = config.TargetFlowBlockName,
                MaxRuntimeSeconds = Math.Max(1, config.MaxRuntimeSeconds),
                IncludeTargetExecution = config.IncludeTargetExecution,
                MaxCapturedFieldValueChanges = Math.Max(0, config.MaxCapturedFieldValueChanges)
            };

            AppendProtocol("RuntimeStarted", "Runtime started.");
        }

        public override void NotifyRuntimeFinished()
        {
            if (!IsEnabled)
                return;

            lock (_sync)
            {
                AppendProtocol("RuntimeFinished", "Runtime finished.");
                _result.Aborted = Runtime.Aborted;
                _result.Cancellation = Runtime.CancellationContext;
                _result.FinishedUtc = DateTime.UtcNow;
                Runtime.SetDebuggingResult(_result);
            }
        }

        public override void NotifyRuntimeCancelled(RuntimeCancellationContext cancellationContext)
        {
            if (!IsEnabled)
                return;

            var reason = cancellationContext?.Reason ?? "Cancellation requested.";
            if (cancellationContext?.CancellationKind == RuntimeCancellationKind.DebuggingTargetReached)
            {
                AppendProtocol("DebugTargetReached", reason);
                return;
            }

            AppendProtocol("RuntimeCancelled", reason);
        }

        public override void NotifyInvocationStarted(BaseFlowBlock flowBlock)
        {
            if (!IsEnabled)
                return;

            AppendProtocol(
                "InvocationStarted",
                $"Invocation started for flow block '{flowBlock?.Name}'.",
                flowBlock?.Name);
        }

        public override void NotifyBeforeFlowBlockValidation(BaseFlowBlock flowBlock)
        {
            if (!IsEnabled || flowBlock == null || Runtime.Aborted)
                return;

            var config = Runtime.ExternalDebuggingInformation;
            if (config.IncludeTargetExecution)
                return;

            if (!IsTarget(flowBlock.Name, config.TargetFlowBlockName))
                return;

            Runtime.CancelExecution(
                RuntimeCancellationKind.DebuggingTargetReached,
                $"Debug target flow block reached before execution: '{flowBlock.Name}'.");
        }

        public override bool ShouldCancelValidation(BaseFlowBlock flowBlock, bool validationFinished)
        {
            if (!IsEnabled || flowBlock == null)
                return false;

            var config = Runtime.ExternalDebuggingInformation;
            if (!IsTarget(flowBlock.Name, config.TargetFlowBlockName))
                return false;

            if (!validationFinished)
                return !config.IncludeTargetExecution;

            return config.IncludeTargetExecution;
        }

        public override void NotifyInvocationFinished(BaseFlowBlock flowBlock)
        {
            if (!IsEnabled)
                return;

            var flowBlockName = flowBlock?.Name;
            AppendProtocol(
                "InvocationFinished",
                $"Invocation finished for flow block '{flowBlockName}'.",
                flowBlockName);

            var config = Runtime.ExternalDebuggingInformation;
            if (config.IncludeTargetExecution
                && !string.IsNullOrWhiteSpace(config.TargetFlowBlockName)
                && string.Equals(config.TargetFlowBlockName, flowBlockName, StringComparison.OrdinalIgnoreCase)
                && !Runtime.Aborted)
            {
                Runtime.CancelExecution(
                    RuntimeCancellationKind.DebuggingTargetReached,
                    $"Debug target flow block reached after execution: '{flowBlockName}'.");
            }
        }

        public override void NotifyPreconditionsNotMet(BaseFlowBlock flowBlock, IReadOnlyList<string> messages)
        {
            if (!IsEnabled)
                return;

            var flowBlockName = flowBlock?.Name ?? string.Empty;
            var list = (messages ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            lock (_sync)
            {
                if (_result == null)
                    return;

                _result.PreconditionFailures.Add(new RuntimeDebugPreconditionFailureEntry
                {
                    Elapsed = GetElapsed(),
                    FlowBlockName = flowBlockName,
                    Messages = list
                });
            }

            var summary = list.Count == 0
                ? "Preconditions were not met."
                : $"Preconditions were not met: {string.Join(" | ", list)}";
            AppendProtocol("PreconditionsNotMet", summary, flowBlockName);
        }

        public override void NotifyIterationStarted(BaseFlowBlock flowBlock)
        {
            if (!IsEnabled)
                return;

            AppendProtocol(
                "IterationStarted",
                $"Iteration started at flow block '{flowBlock?.Name}'.",
                flowBlock?.Name);
        }

        public override void NotifyIterationFinished(BaseFlowBlock flowBlock)
        {
            if (!IsEnabled)
                return;

            AppendProtocol(
                "IterationFinished",
                $"Iteration finished at flow block '{flowBlock?.Name}'.",
                flowBlock?.Name);
        }

        public override void NotifyResultDatasetGenerated(RuntimeResultDatasetSummary resultDatasetSummary)
        {
            if (!IsEnabled || resultDatasetSummary == null)
                return;

            var flowBlockName = resultDatasetSummary.FlowBlock?.Name;
            AppendProtocol(
                "ResultDatasetGenerated",
                $"New result dataset generated: {resultDatasetSummary.DatasetCount} record(s) by '{flowBlockName}'.",
                flowBlockName);
        }

        public override void NotifyWarning(BaseFlowBlock baseFlowBlock, string message)
        {
            if (!IsEnabled)
                return;

            var flowBlockName = baseFlowBlock?.Name ?? string.Empty;
            var text = message ?? string.Empty;

            lock (_sync)
            {
                if (_result == null)
                    return;

                _result.Warnings.Add(new RuntimeDebugProblemEntry
                {
                    Elapsed = GetElapsed(),
                    FlowBlockName = flowBlockName,
                    Message = text
                });
            }

            AppendProtocol("Warning", text, flowBlockName);
        }

        public override void NotifyError(BaseFlowBlock baseFlowBlock, string message, Exception exception = null)
        {
            if (!IsEnabled)
                return;

            var flowBlockName = baseFlowBlock?.Name ?? string.Empty;
            var text = message ?? string.Empty;

            lock (_sync)
            {
                if (_result == null)
                    return;

                _result.Errors.Add(new RuntimeDebugProblemEntry
                {
                    Elapsed = GetElapsed(),
                    FlowBlockName = flowBlockName,
                    Message = text,
                    Exception = exception?.ToString() ?? string.Empty
                });
            }

            AppendProtocol("Error", text, flowBlockName);
        }

        public override void NotifyFieldChange(FieldElement fieldElement, string oldValue, string newValue)
        {
            if (!IsEnabled)
                return;

            var sourceFlowBlockName = fieldElement?.Source?.Name;
            if (string.IsNullOrWhiteSpace(oldValue) && string.IsNullOrWhiteSpace(newValue))
            {
                AppendProtocol(
                    "FieldValueChanged",
                    $"Field value changed, old and new values are null or empty",
                    sourceFlowBlockName);

                return;
            }

            var config = Runtime.ExternalDebuggingInformation;
            var id = Interlocked.Increment(ref _nextFieldChangeId);

            lock (_sync)
            {
                if (_result == null)
                    return;

                _result.TotalFieldValueChanges++;

                var details = new RuntimeFieldValueChangeDetails
                {
                    Id = id,
                    Elapsed = GetElapsed(),
                    FieldName = fieldElement?.FullyQualifiedName ?? string.Empty,
                    SourceFlowBlockName = sourceFlowBlockName
                };

                if (_result.StoredFieldValueChanges < config.MaxCapturedFieldValueChanges)
                {
                    details.IsStored = true;
                    details.OldValue = oldValue;
                    details.NewValue = newValue;
                    _result.StoredFieldValueChanges++;
                }
                else
                {
                    details.IsStored = false;
                    details.Note = "Detailed values skipped due to MaxCapturedFieldValueChanges limit.";
                }

                _result.FieldValueChanges.Add(details);

                AppendProtocol(
                    "FieldValueChanged",
                    $"Field value changed, see id: {id}.",
                    sourceFlowBlockName,
                    id);
            }
        }

        private void AppendProtocol(string eventType, string message, string flowBlockName = null, int? fieldValueChangeId = null)
        {
            lock (_sync)
            {
                if (_result == null)
                    return;

                _result.Protocol.Add(new RuntimeDebugProtocolEntry
                {
                    Elapsed = GetElapsed(),
                    EventType = eventType ?? string.Empty,
                    Message = message ?? string.Empty,
                    FlowBlockName = flowBlockName,
                    FieldValueChangeId = fieldValueChangeId
                });
            }
        }

        private string GetElapsed()
        {
            if (_stopwatch == null)
                return "00:00.000";

            var elapsed = _stopwatch.Elapsed;
            return $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds:D3}";
        }

        private static bool IsTarget(string flowBlockName, string targetFlowBlockName)
        {
            if (string.IsNullOrWhiteSpace(flowBlockName) || string.IsNullOrWhiteSpace(targetFlowBlockName))
                return false;

            return string.Equals(flowBlockName, targetFlowBlockName, StringComparison.OrdinalIgnoreCase);
        }
    }
}

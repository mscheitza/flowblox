using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util;
using FlowBlox.Core.Models.Base;

namespace FlowBlox.Core.Interceptors
{
    public class RuntimeBacktraceInterceptor : RuntimeInterceptorBase
    {
        public delegate void ProblemTraceCreatedEventHandler(BaseRuntime runtime, ProblemTrace problemTrace);

        public event ProblemTraceCreatedEventHandler ProblemTraceCreated;

        private readonly Lazy<ProblemsTracer> _problemsTracer;

        public RuntimeBacktraceInterceptor()
        {
            _problemsTracer = new Lazy<ProblemsTracer>(() => new ProblemsTracer(this.Runtime));
        }

        public override void NotifyWarning(BaseFlowBlock baseFlowBlock, string message)
        {
            var trace = new ProblemTrace
            {
                Name = baseFlowBlock.Name,
                Criticality = "Warning",
                Message = message
            };

            AppendTrace(trace);

            base.NotifyWarning(baseFlowBlock, message);
        }

        public override void NotifyError(BaseFlowBlock baseFlowBlock, string message, Exception exception = null)
        {
            var trace = new ProblemTrace
            {
                Name = baseFlowBlock.Name,
                Criticality = "Error",
                Message = message,
                Exception = exception
            };

            AppendTrace(trace);

            base.NotifyError(baseFlowBlock, message);
        }

        private void AppendTrace(ProblemTrace trace)
        {
            // Create trace output
            _problemsTracer.Value.AppendTrace(trace);

            // Notify external components
            ProblemTraceCreated?.Invoke(Runtime, trace);
        }

        private int? _maxFieldValueLength;
        private int MaxFieldValueLength
        {
            get
            {
                if (_maxFieldValueLength == null)
                {
                    var optionMaxFieldValueLength = FlowBloxOptions.GetOptionInstance().GetOption("Runtime.ProblemTracing.MaxFieldValueLength");
                    var maxFieldValueLengthString = optionMaxFieldValueLength.Value;

                    int maxFieldValueLength;
                    if (int.TryParse(maxFieldValueLengthString, out maxFieldValueLength))
                        _maxFieldValueLength = maxFieldValueLength;
                    else
                        _maxFieldValueLength = 200;
                }

                return _maxFieldValueLength.Value;
            }
        }

        private bool GetListOfFieldValues(BaseFlowBlock baseFlowBlock, BaseFlowBlock currentFlowBlock, List<FieldValue> fieldValues)
        {
            if (baseFlowBlock is BaseResultFlowBlock)
            {
                foreach (var field in ((BaseResultFlowBlock)baseFlowBlock).Fields)
                {
                    fieldValues.Add(new FieldValue(field.FullyQualifiedName, TextHelper.ShortenString(field.StringValue, MaxFieldValueLength, false)));
                }
            }

            foreach (var nextFlowBlock in baseFlowBlock.GetNextFlowBlocks())
            {
                if (nextFlowBlock == currentFlowBlock)
                    return false;

                if (!GetListOfFieldValues(nextFlowBlock, currentFlowBlock, fieldValues))
                    return false;
            }

            return true;
        }
    }
}

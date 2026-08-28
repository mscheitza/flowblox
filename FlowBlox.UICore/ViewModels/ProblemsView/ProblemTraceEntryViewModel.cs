using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.UICore.ViewModels.ProblemsView
{
    public class ProblemTraceEntryViewModel
    {
        public ProblemTraceEntryViewModel(ProblemTrace problemTrace)
        {
            ProblemTrace = problemTrace;
        }

        public ProblemTrace ProblemTrace { get; }

        public string Timestamp => ProblemTrace?.Timestamp.ToString() ?? string.Empty;

        public string Name => ProblemTrace?.Name ?? string.Empty;

        public string Criticality => ProblemTrace?.Criticality ?? string.Empty;

        public string Message => ProblemTrace?.Message ?? string.Empty;

        public string ExceptionText => ProblemTrace?.Exception?.ToString() ?? string.Empty;
    }
}
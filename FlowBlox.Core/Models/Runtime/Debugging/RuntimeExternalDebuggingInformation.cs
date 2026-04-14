namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeExternalDebuggingInformation
    {
        public int MaxRuntimeSeconds { get; set; } = 30;
        public string TargetFlowBlockName { get; set; }
        public bool IncludeTargetExecution { get; set; } = false;
        public int MaxCapturedFieldValueChanges { get; set; } = 100;
    }
}

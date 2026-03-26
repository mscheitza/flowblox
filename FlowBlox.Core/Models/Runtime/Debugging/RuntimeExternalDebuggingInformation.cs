namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeExternalDebuggingInformation
    {
        public int MaxRuntimeSeconds { get; set; } = 30;
        public string TargetFlowBlockName { get; set; }
        public bool FinishWhenTargetFlowBlockReached { get; set; } = true;
        public int MaxCapturedFieldValueChanges { get; set; } = 100;
        public int MaxFieldValueLength { get; set; } = 2000;
    }
}

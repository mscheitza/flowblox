namespace FlowBlox.Core.Runner.Contracts
{
    public sealed class RunnerExternalDebuggingInformation
    {
        public int MaxRuntimeSeconds { get; set; } = 30;
        public string TargetFlowBlockName { get; set; }
        public bool IncludeTargetExecution { get; set; } = false;
        public int MaxCapturedFieldValueChanges { get; set; } = 100;
        public int MaxFieldValueLength { get; set; } = 2000;
        public string DebuggingResultFilePath { get; set; }
    }
}

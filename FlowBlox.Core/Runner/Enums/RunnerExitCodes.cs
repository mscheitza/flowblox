namespace FlowBlox.Core.Runner.Contracts
{
    public static class RunnerExitCodes
    {
        public const int Success = 0;
        public const int ValidationError = 10;
        public const int RuntimeError = 20;
        public const int Aborted = 30;
        public const int ProjectLoadError = 40;
        public const int UnknownError = 99;
    }
}

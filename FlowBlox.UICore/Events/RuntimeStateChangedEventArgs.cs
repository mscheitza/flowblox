using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.UICore.Events
{
    public sealed class RuntimeStateChangedEventArgs : EventArgs
    {
        public RuntimeStateChangedEventArgs(BaseRuntime runtime, bool isRuntimeActive, bool isRuntimePaused)
        {
            Runtime = runtime;
            IsRuntimeActive = isRuntimeActive;
            IsRuntimePaused = isRuntimePaused;
        }

        public BaseRuntime Runtime { get; }
        public bool IsRuntimeActive { get; }
        public bool IsRuntimePaused { get; }
    }
}

using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.UICore.Events
{
    public sealed class RuntimeStateChangedEventArgs : EventArgs
    {
        public RuntimeStateChangedEventArgs(
            BaseRuntime runtime,
            bool isRuntimeActive,
            bool isRuntimePaused,
            bool isRuntimeStartBlocked)
        {
            Runtime = runtime;
            IsRuntimeActive = isRuntimeActive;
            IsRuntimePaused = isRuntimePaused;
            IsRuntimeStartBlocked = isRuntimeStartBlocked;
        }

        public BaseRuntime Runtime { get; }
        public bool IsRuntimeActive { get; }
        public bool IsRuntimePaused { get; }
        public bool IsRuntimeStartBlocked { get; }
    }
}

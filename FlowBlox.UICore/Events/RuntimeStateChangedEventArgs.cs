using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.UICore.Events
{
    public sealed class RuntimeStateChangedEventArgs : EventArgs
    {
        public RuntimeStateChangedEventArgs(
            BaseRuntime runtime,
            bool isRuntimeActive,
            bool isRuntimePaused,
            bool isExternalProjectEditActive)
        {
            Runtime = runtime;
            IsRuntimeActive = isRuntimeActive;
            IsRuntimePaused = isRuntimePaused;
            IsExternalProjectEditActive = isExternalProjectEditActive;
        }

        public BaseRuntime Runtime { get; }
        public bool IsRuntimeActive { get; }
        public bool IsRuntimePaused { get; }
        public bool IsExternalProjectEditActive { get; }
    }
}
using FlowBlox.Core.Models.Runtime;
using FlowBlox.UICore.Events;
using System.ComponentModel;

namespace FlowBlox.UICore.Interfaces
{
    public interface IRuntimeStateService : INotifyPropertyChanged
    {
        BaseRuntime CurrentRuntime { get; }
        bool IsRuntimeActive { get; }
        bool IsRuntimePaused { get; }
        bool IsExternalProjectEditActive { get; }

        event EventHandler<RuntimeStateChangedEventArgs> StateChanged;
        event EventHandler<RuntimeStateChangedEventArgs> RuntimeStarted;
        event EventHandler<RuntimeStateChangedEventArgs> RuntimePausedChanged;
        event EventHandler<RuntimeStateChangedEventArgs> RuntimeFinished;
        event EventHandler<RuntimeStateChangedEventArgs> ExternalProjectEditActiveChanged;

        void AttachRuntime(BaseRuntime runtime);
        void ClearRuntime(BaseRuntime runtime = null);
        void SetExternalProjectEditActive(bool isActive);
    }
}

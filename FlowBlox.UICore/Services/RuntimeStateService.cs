using FlowBlox.Core.Models.Runtime;
using FlowBlox.UICore.Events;
using FlowBlox.UICore.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.Services
{
    public sealed class RuntimeStateService : IRuntimeStateService
    {
        private BaseRuntime _currentRuntime;
        private bool _isRuntimeActive;
        private bool _isRuntimePaused;

        public BaseRuntime CurrentRuntime
        {
            get => _currentRuntime;
            private set
            {
                if (ReferenceEquals(_currentRuntime, value))
                    return;

                _currentRuntime = value;
                OnPropertyChanged();
            }
        }

        public bool IsRuntimeActive
        {
            get => _isRuntimeActive;
            private set
            {
                if (_isRuntimeActive == value)
                    return;

                _isRuntimeActive = value;
                OnPropertyChanged();
            }
        }

        public bool IsRuntimePaused
        {
            get => _isRuntimePaused;
            private set
            {
                if (_isRuntimePaused == value)
                    return;

                _isRuntimePaused = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<RuntimeStateChangedEventArgs> StateChanged;
        public event EventHandler<RuntimeStateChangedEventArgs> RuntimeStarted;
        public event EventHandler<RuntimeStateChangedEventArgs> RuntimePausedChanged;
        public event EventHandler<RuntimeStateChangedEventArgs> RuntimeFinished;

        public void AttachRuntime(BaseRuntime runtime)
        {
            if (runtime == null)
            {
                ClearRuntime();
                return;
            }

            if (!ReferenceEquals(CurrentRuntime, runtime))
            {
                DetachRuntime(CurrentRuntime);
                CurrentRuntime = runtime;
                CurrentRuntime.RuntimeStarted += CurrentRuntime_RuntimeStarted;
                CurrentRuntime.PauseContinue += CurrentRuntime_PauseContinue;
                CurrentRuntime.Finish += CurrentRuntime_Finish;
            }

            UpdateState(runtime, runtime.Running && !runtime.Aborted, runtime.Pause);
        }

        public void ClearRuntime(BaseRuntime runtime = null)
        {
            if (runtime != null && !ReferenceEquals(CurrentRuntime, runtime))
                return;

            var previousRuntime = CurrentRuntime;
            DetachRuntime(CurrentRuntime);
            CurrentRuntime = null;
            UpdateState(previousRuntime, false, false, RuntimeFinished);
        }

        private void CurrentRuntime_RuntimeStarted(BaseRuntime runtime)
            => UpdateState(runtime, runtime.Running && !runtime.Aborted, runtime.Pause, RuntimeStarted);

        private void CurrentRuntime_PauseContinue(bool isPaused)
            => UpdateState(CurrentRuntime, CurrentRuntime?.Running == true && CurrentRuntime.Aborted == false, isPaused, RuntimePausedChanged);

        private void CurrentRuntime_Finish(object result)
            => ClearRuntime(result as BaseRuntime ?? CurrentRuntime);

        private void UpdateState(
            BaseRuntime runtime,
            bool isRuntimeActive,
            bool isRuntimePaused,
            EventHandler<RuntimeStateChangedEventArgs> specificEvent = null)
        {
            IsRuntimePaused = isRuntimePaused;
            IsRuntimeActive = isRuntimeActive;

            var args = new RuntimeStateChangedEventArgs(runtime, IsRuntimeActive, IsRuntimePaused);
            specificEvent?.Invoke(this, args);
            StateChanged?.Invoke(this, args);
        }

        private void DetachRuntime(BaseRuntime runtime)
        {
            if (runtime == null)
                return;

            runtime.RuntimeStarted -= CurrentRuntime_RuntimeStarted;
            runtime.PauseContinue -= CurrentRuntime_PauseContinue;
            runtime.Finish -= CurrentRuntime_Finish;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

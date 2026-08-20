using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    internal sealed class DockPanelLayoutDebouncer
    {
        private const int QuietIntervalMilliseconds = 1500;
        private static readonly Dictionary<DockPanel, DockPanelLayoutDebouncer> Debouncers = new Dictionary<DockPanel, DockPanelLayoutDebouncer>();

        private readonly DockPanel _dockPanel;
        private readonly Timer _timer;
        private readonly HashSet<DockPanelLayoutDebounceRegistration> _registrations = new HashSet<DockPanelLayoutDebounceRegistration>();
        private int _version;
        private bool _disposed;

        private DockPanelLayoutDebouncer(DockPanel dockPanel)
        {
            _dockPanel = dockPanel;
            _timer = new Timer
            {
                Interval = QuietIntervalMilliseconds
            };
            _timer.Tick += Timer_Tick;
            _dockPanel.Disposed += DockPanel_Disposed;
        }

        public static DockPanelLayoutDebouncer For(DockPanel dockPanel)
        {
            if (!Debouncers.TryGetValue(dockPanel, out var debouncer) || debouncer._disposed)
            {
                debouncer = new DockPanelLayoutDebouncer(dockPanel);
                Debouncers[dockPanel] = debouncer;
            }

            return debouncer;
        }

        public DockPanelLayoutDebounceRegistration Register(string key)
        {
            var registration = new DockPanelLayoutDebounceRegistration(this, key);
            _registrations.Add(registration);
            Schedule($"Register:{key}");
            return registration;
        }

        private void NotifyActivity(DockPanelLayoutDebounceRegistration registration, string reason)
        {
            if (!registration.IsSuppressed)
                return;

            Schedule($"{registration.Key}:{reason}");
        }

        private void Complete(DockPanelLayoutDebounceRegistration registration)
        {
            _registrations.Remove(registration);

            if (_registrations.Count == 0)
                _timer.Stop();
        }

        private void DockPanel_Disposed(object sender, EventArgs e)
        {
            _disposed = true;
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
            _timer.Dispose();
            _registrations.Clear();
            _dockPanel.Disposed -= DockPanel_Disposed;
            Debouncers.Remove(_dockPanel);
        }

        private void Schedule(string reason)
        {
            if (!_registrations.Any(x => x.IsSuppressed))
                return;

            _version++;
            _timer.Stop();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            var version = _version;

            if (_dockPanel.IsHandleCreated && !_dockPanel.IsDisposed)
            {
                _dockPanel.BeginInvoke(new MethodInvoker(() => CompleteQuietPhase(version)));
                return;
            }

            CompleteQuietPhase(version);
        }

        private void CompleteQuietPhase(int version)
        {
            if (version != _version)
                return;

            var initializing = _registrations.Where(x => x.Phase == DockPanelLayoutDebouncePhase.Initializing).ToList();
            if (initializing.Count > 0)
            {
                foreach (var registration in initializing)
                    registration.StartSettling();

                Schedule("StartSettling");
                return;
            }

            var settling = _registrations.Where(x => x.Phase == DockPanelLayoutDebouncePhase.Settling).ToList();
            foreach (var registration in settling)
                registration.Complete();
        }

        internal enum DockPanelLayoutDebouncePhase
        {
            Initializing,
            Settling,
            Completed
        }

        internal sealed class DockPanelLayoutDebounceRegistration
        {
            private readonly DockPanelLayoutDebouncer _owner;

            public DockPanelLayoutDebounceRegistration(DockPanelLayoutDebouncer owner, string key)
            {
                _owner = owner;
                Key = key;
                Phase = DockPanelLayoutDebouncePhase.Initializing;
            }

            public string Key { get; }

            public DockPanelLayoutDebouncePhase Phase { get; private set; }

            public bool IsSuppressed => Phase != DockPanelLayoutDebouncePhase.Completed;

            public void NotifyActivity(string reason)
            {
                _owner.NotifyActivity(this, reason);
            }

            public void Stop()
            {
                Phase = DockPanelLayoutDebouncePhase.Completed;
                _owner.Complete(this);
            }

            internal void StartSettling()
            {
                Phase = DockPanelLayoutDebouncePhase.Settling;
            }

            internal void Complete()
            {
                Phase = DockPanelLayoutDebouncePhase.Completed;
                _owner.Complete(this);
            }
        }
    }
}

using FlowBlox.Core.Models.Project;

namespace FlowBlox.Core.Provider.Project
{
    public class FlowBloxProjectManager
    {
        private static readonly Lazy<FlowBloxProjectManager> lazy =
            new Lazy<FlowBloxProjectManager>(() => new FlowBloxProjectManager());

        public static FlowBloxProjectManager Instance { get { return lazy.Value; } }

        public event EventHandler<ProjectChangedEventArgs> ProjectChanged;

        private FlowBloxProject _activeProject;
        public FlowBloxProject ActiveProject
        {
            get
            {
                return _activeProject;
            }
            set
            {
                if (_activeProject != value)
                {
                    var oldProject = _activeProject;
                    if (value == null)
                        _activeProject?.OnProjectClosed();

                    _activeProject = value;

                    if (_activeProject != null)
                        _activeProject.OnProjectLoaded();

                    ProjectChanged?.Invoke(this, new ProjectChangedEventArgs(oldProject, _activeProject));
                }
            }
        }

        public string ActiveProjectPath { get; set; }
    }
}

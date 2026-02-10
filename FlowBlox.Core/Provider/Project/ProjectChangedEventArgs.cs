using FlowBlox.Core.Models.Project;

namespace FlowBlox.Core.Provider.Project
{
    public class ProjectChangedEventArgs
    {
        public ProjectChangedEventArgs(FlowBloxProject oldProject, FlowBloxProject newProject)
        {
            this.OldProject = oldProject;
            this.NewProject = newProject;
        }

        public FlowBloxProject OldProject { get; }

        public FlowBloxProject NewProject { get; }
    }
}

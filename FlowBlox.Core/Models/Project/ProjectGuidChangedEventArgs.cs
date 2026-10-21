namespace FlowBlox.Core.Models.Project
{
    public sealed class ProjectGuidChangedEventArgs : EventArgs
    {
        public ProjectGuidChangedEventArgs(Guid oldProjectGuid, Guid newProjectGuid)
        {
            OldProjectGuid = oldProjectGuid;
            NewProjectGuid = newProjectGuid;
        }

        public Guid OldProjectGuid { get; }

        public Guid NewProjectGuid { get; }
    }
}

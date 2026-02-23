using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;

namespace FlowBlox.UICore.ViewModels.PSProjects
{
    public sealed class PSProjectSelection
    {
        public FbProjectResult Project { get; }
        public FbProjectVersionResult Version { get; }

        public bool HasVersion => Version != null;

        public PSProjectSelection(FbProjectResult project, FbProjectVersionResult version = null)
        {
            Project = project;
            Version = version;
        }
    }
}
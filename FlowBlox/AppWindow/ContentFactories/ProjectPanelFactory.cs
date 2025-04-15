using FlowBlox.AppWindow.Contents;
using System;
using System.Windows.Controls;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class ProjectPanelFactory : DockContentFactoryBase<ProjectPanel>
    {
        public ProjectPanelFactory(WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override ProjectPanel Create()
        {
            var dockContent = new ProjectPanel()
            {
                Dock = DockStyle.Fill
            };
            var key = typeof(ProjectPanel).FullName;
            return Create(key, dockContent);
        }

        protected override DockContentSettings GetDefaults()
        {
            return new DockContentSettings()
            {
                DockState = DockState.Document
            };
        }

    }
}

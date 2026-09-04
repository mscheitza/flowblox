using FlowBlox.UICore.Views;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class ManagedObjectsView : DockContent
    {
        private readonly ElementHost _elementHost;
        private readonly ManagedObjectsViewControl _managedObjectsViewControl;

        public ManagedObjectsView()
        {
            Text = FlowBlox.UICore.Resources.ManagedObjectsView.Title;
            Name = nameof(ManagedObjectsView);
            DockAreas = DockAreas.DockRight | DockAreas.DockLeft | DockAreas.DockBottom;
            Padding = new Padding(0, 25, 0, 25);

            _managedObjectsViewControl = new ManagedObjectsViewControl();
            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Child = _managedObjectsViewControl
            };

            Controls.Add(_elementHost);
        }

        internal void AfterProjectFullyInitialized()
        {
            _managedObjectsViewControl.AfterProjectFullyInitialized();
        }

        internal void UpdateUI()
        {
        }
    }
}
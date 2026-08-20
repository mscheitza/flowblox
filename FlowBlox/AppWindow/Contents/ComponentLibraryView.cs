using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.WPF;
using FlowBlox.UICore.ViewModels.ComponentLibrary;
using FlowBlox.UICore.Views;
using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class ComponentLibraryView : DockContent
    {
        private readonly ElementHost _elementHost;
        private readonly ComponentLibraryViewControl _componentLibraryViewControl;
        private readonly ComponentLibraryViewModel _viewModel;

        public ComponentLibraryView()
        {
            Text = FlowBlox.UICore.Resources.ComponentLibraryView.Title;
            Name = nameof(ComponentLibraryView);
            DockAreas = DockAreas.DockLeft | DockAreas.DockRight;
            Padding = new Padding(0, 25, 0, 0);

            _componentLibraryViewControl = new ComponentLibraryViewControl();
            _viewModel = _componentLibraryViewControl.ViewModel;
            if (_viewModel != null)
                _viewModel.ManageExtensionsRequested += ViewModel_ManageExtensionsRequested;

            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Child = _componentLibraryViewControl
            };

            Controls.Add(_elementHost);
        }

        internal void UpdateUI()
        {
        }

        internal new bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return false;
        }

        private void ViewModel_ManageExtensionsRequested(object sender, EventArgs e)
        {
            FlowBloxProject project = FlowBloxProjectManager.Instance.ActiveProject;
            var dialog = new ExtensionsWindow(project);
            var owner = ControlHelper.FindParentOfType<Form>(this, true);
            WindowsFormWPFHelper.ShowDialog(dialog, owner);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _viewModel != null)
            {
                _viewModel.ManageExtensionsRequested -= ViewModel_ManageExtensionsRequested;
                _viewModel.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

using FlowBlox.AppWindow.ContentFactories;
using FlowBlox.AppWindow.Contents;
using FlowBlox.Core;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow
{
    public partial class AppWindow
    {
        private void InitializeDockPanel(bool exceptProjectPanel = false, bool exceptAiAssistantView = false)
        {
            this.dockPanel.SuspendLayout();

            this.dockPanel.Theme = new VS2015DarkTheme();

            foreach (var dockContent in dockPanel.Contents
                .OfType<DockContent>()
                .Where(x =>
                    (!exceptProjectPanel || x is not ProjectPanel) &&
                    (!exceptAiAssistantView || x is not AIAssistantView))
                .ToList())
            {
                dockContent.Close();
            }

            if (!exceptProjectPanel)
            {
                var projectPanelFactory = new ProjectPanelFactory(dockPanel);
                _dockContentProjectPanel = projectPanelFactory.Create();
            }

            var componentLibraryPanelFactory = new ComponentLibraryPanelFactory(dockPanel);
            _componentLibraryPanel = componentLibraryPanelFactory.Create();

            var fieldViewPanelFactory = new FieldViewPanelFactory(dockPanel);
            _fieldViewPanel = fieldViewPanelFactory.Create();

            var managedObjectsViewPanelFactory = new ManagedObjectsViewPanelFactory(dockPanel);
            _managedObjectsViewPanel = managedObjectsViewPanelFactory.Create();

            var testViewPanelFactory = new TestViewPanelFactory(dockPanel);
            _testViewPanel = testViewPanelFactory.Create();

            if (!exceptAiAssistantView || _aiAssistantViewPanel == null)
            {
                var aiAssistantPanelFactory = new AIAssistantViewPanelFactory(dockPanel);
                _aiAssistantViewPanel = aiAssistantPanelFactory.Create();
            }

            var problemsViewPanelFactory = new ProblemsViewPanelFactory(dockPanel);
            _problemsViewPanel = problemsViewPanelFactory.Create();

            var runtimeViewPanelFactory = new RuntimeViewPanelFactory(dockPanel);
            _runtimeViewPanel = runtimeViewPanelFactory.Create();

            this.dockPanel.ResumeLayout();
            ApplyDefaultFieldViewActivationOnce();
        }

        private void ActivateFieldView()
        {
            if (_defaultFieldViewActivationApplied || _fieldViewPanel == null || _fieldViewPanel.IsDisposed)
                return;

            if (_fieldViewPanel.IsHidden ||
                _fieldViewPanel.DockState == DockState.Hidden ||
                _fieldViewPanel.DockState == DockState.Unknown)
            {
                return;
            }

            _fieldViewPanel.Activate();
            _defaultFieldViewActivationApplied = true;
        }

        private void ApplyDefaultFieldViewActivationOnce()
        {
            if (_defaultFieldViewActivationApplied || _fieldViewPanel == null)
                return;

            if (_fieldViewPanel.IsHidden ||
                _fieldViewPanel.DockState == DockState.Hidden ||
                _fieldViewPanel.DockState == DockState.Unknown)
            {
                return;
            }

            if (IsHandleCreated)
                BeginInvoke(new MethodInvoker(ActivateFieldView));
            else
                ActivateFieldView();
        }

        private void DockPanel_ContentAdded(object sender, DockContentEventArgs e)
        {
            var dockContent = ((DockContent)e.Content);

            var toolstripMenuItem = new ToolStripMenuItem()
            {
                Text = dockContent.Text
            };

            toolstripMenuItem.Image = DockContentIconResolver.Resolve(dockContent);

            toolstripMenuItem.Click += (s, e2) =>
            {
                dockContent.Show();
            };

            itmDockablePanels.DropDownItems.Add(toolstripMenuItem);

            FlowBloxStyle.ApplyStyle(this.menuStrip);
        }

        private void DockPanel_ContentRemoved(object sender, DockContentEventArgs e)
        {
            var removedContent = (DockContent)e.Content;
            foreach (ToolStripItem item in itmDockablePanels.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Text == removedContent.Name)
                {
                    itmDockablePanels.DropDownItems.Remove(item);
                    break;
                }
            }
        }
    }
}

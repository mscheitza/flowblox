using FlowBlox.AppWindow;
using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.WPF;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Grid.Provider;
using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.Views;
using FlowBlox.Views;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class AIAssistantView : DockContent
    {
        private readonly ElementHost _elementHost;
        private readonly AIAssistantControl _assistantControl;
        private readonly AIAssistantViewModel _viewModel;

        public AIAssistantView()
        {
            Text = "AI Assistant";
            Name = nameof(AIAssistantView);
            DockAreas = DockAreas.DockRight | DockAreas.DockLeft | DockAreas.DockBottom;

            _assistantControl = new AIAssistantControl();
            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _assistantControl
            };

            Controls.Add(_elementHost);
            _assistantControl.ConfigurationRequested += AssistantControl_ConfigurationRequested;

            _viewModel = _assistantControl.DataContext as AIAssistantViewModel;
            if (_viewModel != null)
            {
                _viewModel.FlowBlocksChanged += ViewModel_FlowBlocksChanged;
            }
        }

        internal void OnAfterUIRegistryInitialized()
        {
            _viewModel?.ResetForProjectInitialization();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _assistantControl.ConfigurationRequested -= AssistantControl_ConfigurationRequested;
                if (_viewModel != null)
                {
                    _viewModel.FlowBlocksChanged -= ViewModel_FlowBlocksChanged;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FlowBloxServiceLocator.Instance.RegisterServicesFromCurrentAppDomain();
            FlowBloxOptions.GetOptionInstance().InitDefaults(false);
        }

        private void AssistantControl_ConfigurationRequested(object sender, EventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AssistantControl_ConfigurationRequested(sender, e)));
                return;
            }

            var configuration = _viewModel.GetConfiguration(out var loadError);
            if (!string.IsNullOrWhiteSpace(loadError))
            {
                FlowBloxMessageBox.Show(
                    this,
                    loadError,
                    "AI Assistant Configuration",
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Warning);
                return;
            }

            var propertyWindow = new FlowBlox.UICore.Views.PropertyWindow(new PropertyWindowArgs(
                configuration,
                readOnly: false,
                deepCopy: false,
                canSave: true))
            {
                Title = "AI Assistant Configuration",
                Height = 760,
                Width = 980
            };

            var owner = ControlHelper.FindParentOfType<Form>(this, true);
            WindowsFormWPFHelper.ShowDialog(propertyWindow, owner);

            if (propertyWindow.DialogResult != true)
            {
                return;
            }

            if (!_viewModel.SaveConfiguration(configuration, out var saveError))
            {
                FlowBloxMessageBox.Show(
                    this,
                    saveError,
                    "AI Assistant Configuration",
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Error);
            }
        }

        private void ViewModel_FlowBlocksChanged(object sender, FlowBlocksChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ViewModel_FlowBlocksChanged(sender, e)));
                return;
            }

            var project = FlowBloxProjectManager.Instance.ActiveProject;
            if (project == null || e == null || !e.HasChanges)
            {
                return;
            }

            var appWindow = AppWindow.Instance;
            var projectPanel = appWindow.GetAccessibleComponent<ProjectPanel>();
            if (projectPanel == null)
            {
                return;
            }

            var componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();
            var uiRegistry = componentProvider.GetCurrentUIRegistry();
            if (uiRegistry == null)
            {
                return;
            }

            foreach (var flowBlock in e.AddedFlowBlocks)
            {
                if (flowBlock == null)
                {
                    continue;
                }

                if (uiRegistry.GetUIElementToGridElement(flowBlock) != null)
                {
                    continue;
                }

                var uiElement = projectPanel.CreateGridUIElement(flowBlock);
                uiRegistry.RegisterGridUIElement(uiElement);
            }

            foreach (var removedFlowBlockName in e.RemovedFlowBlockNames)
            {
                if (string.IsNullOrWhiteSpace(removedFlowBlockName))
                {
                    continue;
                }

                var uiElement = uiRegistry.UIElements.FirstOrDefault(x =>
                    string.Equals(x?.InternalFlowBlock?.Name, removedFlowBlockName, StringComparison.OrdinalIgnoreCase));

                if (uiElement == null)
                {
                    continue;
                }

                uiElement.Parent?.Controls.Remove(uiElement);
                uiRegistry.RemoveUIElement(uiElement);
                uiElement.Dispose();
            }

            appWindow.ReloadAllObjectManager();
            projectPanel.UpdateUI(gridUpdate: true, appWindowUpdate: true);
        }
    }
}

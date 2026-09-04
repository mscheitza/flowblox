using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Json;
using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.Views;
using FlowBlox.Views;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;
using FlowBlox.Util.Controls;
using FlowBlox.Util.WPF;

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
                _viewModel.ConfigureProjectStateAccess(CaptureCurrentProjectState, RestoreProjectStateAsync);
            }
        }

        internal void AfterProjectFullyInitialized()
        {
            _viewModel?.ResetForProjectInitialization();
        }

        private AIAssistantProjectStateSnapshot? CaptureCurrentProjectState()
        {
            var project = FlowBloxProjectManager.Instance.ActiveProject;
            if (project == null)
                return null;

            project.RefreshOrderedTopLevelCollectionsForSerialization();

            return new AIAssistantProjectStateSnapshot
            {
                ProjectGuid = project.ProjectGuid,
                ProjectName = project.ProjectName ?? string.Empty,
                ProjectJson = JsonConvert.SerializeObject(project, JsonSettings.ProjectExport()),
                ExtensionsJson = JsonConvert.SerializeObject(project.Extensions ?? new()),
                ProjectSpaceGuid = project.ProjectSpaceGuid ?? string.Empty,
                ProjectSpaceVersion = project.ProjectSpaceVersion,
                ProjectSpaceEndpointUri = project.ProjectSpaceEndpointUri ?? string.Empty
            };
        }

        private async Task<bool> RestoreProjectStateAsync(AIAssistantProjectStateSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.ProjectJson))
                return false;

            try
            {
                var restoredProject = FlowBloxProject.FromJsonContents(
                    snapshot.ProjectJson,
                    snapshot.ExtensionsJson,
                    string.IsNullOrWhiteSpace(snapshot.ProjectSpaceGuid) ? null : snapshot.ProjectSpaceGuid,
                    snapshot.ProjectSpaceVersion,
                    string.IsNullOrWhiteSpace(snapshot.ProjectSpaceEndpointUri) ? null : snapshot.ProjectSpaceEndpointUri);

                return await AppWindow.Instance.RestoreProjectStateWithoutConfirmationAsync(restoredProject);
            }
            catch (Exception ex)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();
                logger.Exception(ex);
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _assistantControl.ConfigurationRequested -= AssistantControl_ConfigurationRequested;
                if (_viewModel != null)
                {
                    _viewModel.Dispose();
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
                canSave: true,
                detached: true))
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

    }
}
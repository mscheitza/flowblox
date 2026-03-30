using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public class ManageInputTemplatesViewModel : INotifyPropertyChanged
    {
        private readonly Window _ownerWindow;
        private readonly FlowBloxProject _project;

        private FlowBloxInputFileTemplate _selectedTemplate;

        public RelayCommand CloseCommand { get; }
        public RelayCommand OpenInputDirectoryCommand { get; }

        public RelayCommand CreateTemplateCommand { get; }
        public RelayCommand UploadTemplateCommand { get; }
        public RelayCommand DownloadTemplateCommand { get; }
        public RelayCommand OpenTemplateInExplorerCommand { get; }
        public RelayCommand RemoveTemplateCommand { get; }

        public ObservableCollection<FlowBloxInputFileTemplate> Templates { get; }
        public ObservableCollection<FlowBloxInputTemplateSyncMode> SyncModes { get; }

        public string ProjectInputDirectory => _project?.ProjectInputDirectory ?? "";

        public FlowBloxInputFileTemplate SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                if (ReferenceEquals(_selectedTemplate, value))
                    return;

                _selectedTemplate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTemplateSelected));
            }
        }

        public bool IsTemplateSelected => SelectedTemplate != null;

        public ManageInputTemplatesViewModel(Window ownerWindow, FlowBloxProject project)
        {
            _ownerWindow = ownerWindow;
            _project = project;

            Templates = new ObservableCollection<FlowBloxInputFileTemplate>(_project?.InputTemplates ?? new System.Collections.Generic.List<FlowBloxInputFileTemplate>());
            SyncModes = new ObservableCollection<FlowBloxInputTemplateSyncMode>(
                Enum.GetValues(typeof(FlowBloxInputTemplateSyncMode)).Cast<FlowBloxInputTemplateSyncMode>());

            CloseCommand = new RelayCommand(() => _ownerWindow?.Close());

            OpenInputDirectoryCommand = new RelayCommand(OpenInputDirectory);

            CreateTemplateCommand = new RelayCommand(CreateTemplate);
            UploadTemplateCommand = new RelayCommand(UploadTemplate, () => IsTemplateSelected);
            DownloadTemplateCommand = new RelayCommand(DownloadTemplate, () => IsTemplateSelected);
            OpenTemplateInExplorerCommand = new RelayCommand(OpenTemplateInExplorer, () => IsTemplateSelected);
            RemoveTemplateCommand = new RelayCommand(RemoveTemplate, () => IsTemplateSelected);
        }

        private void OpenInputDirectory()
        {
            try
            {
                var dir = ProjectInputDirectory;
                if (string.IsNullOrWhiteSpace(dir))
                    return;

                Directory.CreateDirectory(dir);

                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_OpenInputDirectoryFailed", typeof(Resources.ManageInputTemplatesWindow)), ex.Message);
            }
        }

        private void CreateTemplate()
        {
            if (_project == null)
                return;

            var tpl = new FlowBloxInputFileTemplate
            {
                RelativePath = "new-file.txt",
                ContentBase64 = ""
            };

            Templates.Add(tpl);

            // Keep project list in sync (no explicit save button needed).
            _project.InputTemplates ??= new System.Collections.Generic.List<FlowBloxInputFileTemplate>();
            _project.InputTemplates.Add(tpl);

            SelectedTemplate = tpl;
        }

        private void UploadTemplate()
        {
            if (SelectedTemplate == null)
                return;

            var ofd = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() != true)
                return;

            try
            {
                var bytes = File.ReadAllBytes(ofd.FileName);
                SelectedTemplate.ContentBytes = bytes;

                // If RelativePath is empty, use the selected file name.
                if (string.IsNullOrWhiteSpace(SelectedTemplate.RelativePath))
                    SelectedTemplate.RelativePath = Path.GetFileName(ofd.FileName);

                OnPropertyChanged(nameof(Templates));
                OnPropertyChanged(nameof(SelectedTemplate));
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_UploadTemplateFailed", typeof(Resources.ManageInputTemplatesWindow)), ex.Message);
            }
        }

        private void DownloadTemplate()
        {
            if (SelectedTemplate == null)
                return;

            var defaultName = !string.IsNullOrWhiteSpace(SelectedTemplate.FileName) ? SelectedTemplate.FileName : "template.bin";

            var sfd = new SaveFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FileName = defaultName
            };

            if (sfd.ShowDialog() != true)
                return;

            try
            {
                var bytes = SelectedTemplate.ContentBytes ?? Array.Empty<byte>();
                File.WriteAllBytes(sfd.FileName, bytes);

                ShowNotification(FlowBloxResourceUtil.GetLocalizedString("Message_TemplateDownloaded", typeof(Resources.ManageInputTemplatesWindow)));
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_DownloadTemplateFailed", typeof(Resources.ManageInputTemplatesWindow)), ex.Message);
            }
        }

        private void OpenTemplateInExplorer()
        {
            if (SelectedTemplate == null)
                return;

            try
            {
                var inputDir = ProjectInputDirectory;
                if (string.IsNullOrWhiteSpace(inputDir))
                    return;

                var targetPath = FlowBloxInputTemplateHelper.BuildAbsoluteTargetPath(inputDir, SelectedTemplate.RelativePath);

                if (!File.Exists(targetPath))
                {
                    ShowError(
                        FlowBloxResourceUtil.GetLocalizedString("Error_TemplateFileMissing", typeof(Resources.ManageInputTemplatesWindow)),
                        targetPath);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{targetPath}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_OpenTemplateInExplorerFailed", typeof(Resources.ManageInputTemplatesWindow)), ex.Message);
            }
        }

        private void RemoveTemplate()
        {
            if (SelectedTemplate == null || _project == null)
                return;

            try
            {
                var toRemove = SelectedTemplate;

                Templates.Remove(toRemove);
                _project.InputTemplates?.Remove(toRemove);

                SelectedTemplate = null;

                // We do not delete the already materialized file automatically,
                // because it might contain user changes. Only the template reference is removed.
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_RemoveTemplateFailed", typeof(Resources.ManageInputTemplatesWindow)), ex.Message);
            }
        }

        private void ShowNotification(string message)
        {
            if (_ownerWindow is MetroWindow mw)
            {
                _ = MessageBoxHelper.ShowMessageBoxAsync(mw, MessageBoxType.Notification, message);
            }
        }

        private void ShowError(string titleOrMessage, string details)
        {
            if (_ownerWindow is MetroWindow mw)
            {
                _ = MessageBoxHelper.ShowMessageBoxAsync(mw, MessageBoxType.Error,
                    ApiErrorMessageHelper.BuildErrorMessage(titleOrMessage, details));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

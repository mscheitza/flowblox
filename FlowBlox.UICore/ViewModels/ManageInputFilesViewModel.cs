using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Util.ShellExecution;
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
    public class ManageInputFilesViewModel : INotifyPropertyChanged
    {
        private readonly Window _ownerWindow;
        private readonly FlowBloxProject _project;

        private FlowBloxInputFile _selectedInputFile;

        public RelayCommand CloseCommand { get; }
        public RelayCommand OpenInputDirectoryCommand { get; }

        public RelayCommand CreateInputFileCommand { get; }
        public RelayCommand UploadInputFileCommand { get; }
        public RelayCommand DownloadInputFileCommand { get; }
        public RelayCommand OpenInputFileInExplorerCommand { get; }
        public RelayCommand RemoveInputFileCommand { get; }
        public RelayCommand ExecuteInputFileCommandCommand { get; }

        public ObservableCollection<FlowBloxInputFile> InputFiles { get; }
        public ObservableCollection<FlowBloxInputFileSyncMode> SyncModes { get; }

        public string ProjectInputDirectory => _project?.ProjectInputDirectory ?? "";

        public FlowBloxInputFile SelectedInputFile
        {
            get => _selectedInputFile;
            set
            {
                if (ReferenceEquals(_selectedInputFile, value))
                    return;

                _selectedInputFile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInputFileSelected));
            }
        }

        public bool IsInputFileSelected => SelectedInputFile != null;

        public ManageInputFilesViewModel(Window ownerWindow, FlowBloxProject project)
        {
            _ownerWindow = ownerWindow;
            _project = project;

            InputFiles = new ObservableCollection<FlowBloxInputFile>(_project?.InputFiles ?? new System.Collections.Generic.List<FlowBloxInputFile>());
            SyncModes = new ObservableCollection<FlowBloxInputFileSyncMode>(
                Enum.GetValues(typeof(FlowBloxInputFileSyncMode)).Cast<FlowBloxInputFileSyncMode>());

            CloseCommand = new RelayCommand(() => _ownerWindow?.Close());

            OpenInputDirectoryCommand = new RelayCommand(OpenInputDirectory);

            CreateInputFileCommand = new RelayCommand(CreateInputFile);
            UploadInputFileCommand = new RelayCommand(UploadInputFile, () => IsInputFileSelected);
            DownloadInputFileCommand = new RelayCommand(DownloadInputFile, () => IsInputFileSelected);
            OpenInputFileInExplorerCommand = new RelayCommand(OpenInputFileInExplorer, () => IsInputFileSelected);
            RemoveInputFileCommand = new RelayCommand(RemoveInputFile, () => IsInputFileSelected);
            ExecuteInputFileCommandCommand = new RelayCommand(ExecuteSelectedInputFileCommand, () => IsInputFileSelected);
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
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_OpenInputDirectoryFailed", typeof(Resources.ManageInputFilesWindow)), ex.Message);
            }
        }

        private void CreateInputFile()
        {
            if (_project == null)
                return;

            var tpl = new FlowBloxInputFile
            {
                RelativePath = "new-file.txt",
                ContentBase64 = "",
                Command = "",
                ExecuteBeforeRuntime = false
            };

            InputFiles.Add(tpl);

            // Keep project list in sync (no explicit save button needed).
            _project.InputFiles ??= new System.Collections.Generic.List<FlowBloxInputFile>();
            _project.InputFiles.Add(tpl);

            SelectedInputFile = tpl;
        }

        private void UploadInputFile()
        {
            if (SelectedInputFile == null)
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
                SelectedInputFile.ContentBytes = bytes;

                // If RelativePath is empty, use the selected file name.
                if (string.IsNullOrWhiteSpace(SelectedInputFile.RelativePath))
                    SelectedInputFile.RelativePath = Path.GetFileName(ofd.FileName);

                OnPropertyChanged(nameof(InputFiles));
                OnPropertyChanged(nameof(SelectedInputFile));
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_UploadTemplateFailed", typeof(Resources.ManageInputFilesWindow)), ex.Message);
            }
        }

        private void DownloadInputFile()
        {
            if (SelectedInputFile == null)
                return;

            var defaultName = !string.IsNullOrWhiteSpace(SelectedInputFile.FileName) ? SelectedInputFile.FileName : "input-file.bin";

            var sfd = new SaveFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FileName = defaultName
            };

            if (sfd.ShowDialog() != true)
                return;

            try
            {
                var bytes = SelectedInputFile.ContentBytes ?? Array.Empty<byte>();
                File.WriteAllBytes(sfd.FileName, bytes);

                ShowNotification(FlowBloxResourceUtil.GetLocalizedString("Message_TemplateDownloaded", typeof(Resources.ManageInputFilesWindow)));
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_DownloadTemplateFailed", typeof(Resources.ManageInputFilesWindow)), ex.Message);
            }
        }

        private void OpenInputFileInExplorer()
        {
            if (SelectedInputFile == null)
                return;

            try
            {
                var inputDir = ProjectInputDirectory;
                if (string.IsNullOrWhiteSpace(inputDir))
                    return;

                var targetPath = FlowBloxInputFileHelper.BuildAbsoluteTargetPath(inputDir, SelectedInputFile.RelativePath);

                if (!File.Exists(targetPath))
                {
                    ShowError(
                        FlowBloxResourceUtil.GetLocalizedString("Error_TemplateFileMissing", typeof(Resources.ManageInputFilesWindow)),
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
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_OpenTemplateInExplorerFailed", typeof(Resources.ManageInputFilesWindow)), ex.Message);
            }
        }

        private void RemoveInputFile()
        {
            if (SelectedInputFile == null || _project == null)
                return;

            try
            {
                var toRemove = SelectedInputFile;

                InputFiles.Remove(toRemove);
                _project.InputFiles?.Remove(toRemove);

                SelectedInputFile = null;

                // We do not delete the already materialized file automatically,
                // because it might contain user changes. Only the managed input file entry is removed.
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_RemoveTemplateFailed", typeof(Resources.ManageInputFilesWindow)), ex.Message);
            }
        }

        private void ExecuteSelectedInputFileCommand()
        {
            ExecuteInputFileCommand(SelectedInputFile);
        }

        public void ExecuteInputFileCommand(FlowBloxInputFile inputFile)
        {
            if (inputFile == null)
                return;

            try
            {
                var rawCommand = inputFile.Command ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawCommand))
                {
                    ShowNotification(FlowBloxResourceUtil.GetLocalizedString("Message_InputFileCommandMissing", typeof(Resources.ManageInputFilesWindow)));
                    return;
                }

                var command = FlowBloxInputFileHelper.ReplaceInputFilePlaceholders(rawCommand, _project, inputFile);
                command = FlowBloxFieldHelper.ReplaceFieldsInString(command);

                var result = FlowBloxShellExecutor.Execute(new FlowBloxShellExecutionRequest
                {
                    Command = command,
                    WorkingDirectory = _project?.ProjectInputDirectory
                });

                if (result.Success)
                {
                    ShowNotification(FlowBloxResourceUtil.GetLocalizedString("Message_InputFileCommandSuccess", typeof(Resources.ManageInputFilesWindow)));
                    return;
                }

                var details = !string.IsNullOrWhiteSpace(result.ExceptionMessage)
                    ? result.ExceptionMessage
                    : $"ExitCode={result.ExitCode}";

                if (!string.IsNullOrWhiteSpace(result.StandardError))
                    details += Environment.NewLine + result.StandardError;

                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_ExecuteInputFileCommandFailed", typeof(Resources.ManageInputFilesWindow)), details);
            }
            catch (Exception ex)
            {
                ShowError(FlowBloxResourceUtil.GetLocalizedString("Error_ExecuteInputFileCommandFailed", typeof(Resources.ManageInputFilesWindow)), ex.Message);
            }
        }

        public IReadOnlyList<FlowBloxInputFilePlaceholderElement> GetInputFilePlaceholderElements(FlowBloxInputFile inputFile)
        {
            return _project?.GetInputFilePlaceholderElements(inputFile) ?? Array.Empty<FlowBloxInputFilePlaceholderElement>();
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




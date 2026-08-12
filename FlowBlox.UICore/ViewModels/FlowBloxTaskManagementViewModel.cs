using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Runner.Contracts;
using FlowBlox.Core.Runner.Serialization;
using FlowBlox.Core.TaskManagement;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels.PSProjects;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class FlowBloxTaskManagementViewModel : INotifyPropertyChanged
    {
        private readonly Window _ownerWindow;
        private readonly ITaskManagementService _taskManagementService;
        private readonly Dictionary<string, ObservableCollection<FlowBloxScheduledSession>> _sessionsCache = new(StringComparer.OrdinalIgnoreCase);
        private FlowBloxTaskItemViewModel _selectedTask;
        private FlowBloxScheduledSession _selectedSession;
        private bool _isBusy;
        private bool _isDirty;

        public ObservableCollection<FlowBloxTaskItemViewModel> Tasks { get; } = new();
        public ObservableCollection<FlowBloxTaskScheduleType> ScheduleTypes { get; }
        public ObservableCollection<FlowBloxScheduledSession> Sessions { get; private set; } = new();

        public RelayCommand AddProjectFileCommand { get; }
        public RelayCommand AddProjectSpaceCommand { get; }
        public RelayCommand RemoveTaskCommand { get; }
        public RelayCommand RunTaskCommand { get; }
        public RelayCommand StopTaskCommand { get; }
        public RelayCommand ToggleEnabledCommand { get; }
        public RelayCommand ApplyCommand { get; }
        public RelayCommand ReloadCommand { get; }
        public RelayCommand RefreshSessionsCommand { get; }
        public RelayCommand OpenTaskDirectoryCommand { get; }
        public RelayCommand OpenSessionDirectoryCommand { get; }
        public RelayCommand OpenSessionLogFileCommand { get; }
        public RelayCommand CloseCommand { get; }

        public FlowBloxTaskItemViewModel SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (ReferenceEquals(_selectedTask, value))
                    return;

                _selectedTask = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTaskSelected));
                RefreshSessions();
            }
        }

        public FlowBloxScheduledSession SelectedSession
        {
            get => _selectedSession;
            set
            {
                _selectedSession = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSessionSelected));
            }
        }

        public bool IsTaskSelected => SelectedTask != null;
        public bool IsSessionSelected => SelectedSession != null;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (_isDirty == value)
                    return;

                _isDirty = value;
                OnPropertyChanged();
                ApplyCommand.Invalidate();
                RunTaskCommand.Invalidate();
                StopTaskCommand.Invalidate();
            }
        }

        public FlowBloxTaskManagementViewModel(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
            _taskManagementService = TaskManagementProvider.GetService();

            ScheduleTypes = new ObservableCollection<FlowBloxTaskScheduleType>(
                Enum.GetValues(typeof(FlowBloxTaskScheduleType)).Cast<FlowBloxTaskScheduleType>());

            AddProjectFileCommand = new RelayCommand(AddProjectFile);
            AddProjectSpaceCommand = new RelayCommand(AddProjectSpace);
            RemoveTaskCommand = new RelayCommand(RemoveTask, () => IsTaskSelected);
            RunTaskCommand = new RelayCommand(RunTask, () => IsTaskSelected && SelectedTask.CanStart);
            StopTaskCommand = new RelayCommand(StopTask, () => IsTaskSelected && !SelectedTask.IsNew && !SelectedTask.IsDirty);
            ToggleEnabledCommand = new RelayCommand(ToggleEnabled, () => IsTaskSelected);
            ApplyCommand = new RelayCommand(Apply, () => IsDirty);
            ReloadCommand = new RelayCommand(ReloadTasks);
            RefreshSessionsCommand = new RelayCommand(RefreshSessionsFromDisk, () => IsTaskSelected);
            OpenTaskDirectoryCommand = new RelayCommand(OpenTaskDirectory, () => IsTaskSelected);
            OpenSessionDirectoryCommand = new RelayCommand(OpenSessionDirectory, () => IsSessionSelected);
            OpenSessionLogFileCommand = new RelayCommand(OpenSessionLogFile, () => IsSessionSelected && !string.IsNullOrWhiteSpace(SelectedSession?.LogFilePath));
            CloseCommand = new RelayCommand(() => _ownerWindow?.Close());

            LoadTasks(confirmDiscardChanges: false);
        }

        private async void ReloadTasks()
        {
            await LoadTasksAsync(confirmDiscardChanges: true);
        }

        private async void LoadTasks(bool confirmDiscardChanges)
        {
            await LoadTasksAsync(confirmDiscardChanges);
        }

        private async Task LoadTasksAsync(bool confirmDiscardChanges)
        {
            try
            {
                if (confirmDiscardChanges && IsDirty && _ownerWindow is MetroWindow mw)
                {
                    var discard = await MessageBoxHelper.ShowQuestionAsync(mw, L("Message_DiscardChangesQuestion"));
                    if (discard != true)
                        return;
                }

                IsBusy = true;
                UnsubscribeTaskItems();
                Tasks.Clear();
                _sessionsCache.Clear();

                var tasks = await _taskManagementService.GetTasksAsync();
                foreach (var task in tasks)
                    AddTaskItem(FlowBloxTaskItemViewModel.FromModel(task), markDirty: false);

                SelectedTask = Tasks.FirstOrDefault();
                IsDirty = false;
            }
            catch (Exception ex)
            {
                await ShowError("Error_LoadTasksFailed", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddProjectFile()
        {
            var initialDirectory = FlowBloxOptions.GetOptionInstance().GetOption("Paths.ProjectDir")?.Value;
            if (!string.IsNullOrWhiteSpace(initialDirectory))
            {
                initialDirectory = Environment.ExpandEnvironmentVariables(initialDirectory);
                Directory.CreateDirectory(initialDirectory);
            }

            var ofd = new OpenFileDialog
            {
                Filter = "FlowBlox Project (*.fbprj)|*.fbprj|All files (*.*)|*.*",
                InitialDirectory = initialDirectory
            };

            if (ofd.ShowDialog() != true)
                return;

            try
            {
                var project = FlowBloxProject.FromFile(ofd.FileName);
                AddTask(project.ProjectName, projectFile: ofd.FileName, projectSpaceGuid: null, projectSpaceVersion: null);
            }
            catch (Exception ex)
            {
                _ = ShowError("Error_LoadProjectFailed", ex.Message);
            }
        }

        private void AddProjectSpace()
        {
            var dialog = new PSProjectsWindow
            {
                Owner = _ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dialog.ShowDialog() != true)
                return;

            if (dialog.Tag is not PSProjectSelection selection || string.IsNullOrWhiteSpace(selection.Project?.Guid))
                return;

            AddTask(selection.Project.Name, null, selection.Project.Guid, selection.Version?.VersionNumber);
        }

        private void AddTask(string projectName, string projectFile, string projectSpaceGuid, int? projectSpaceVersion)
        {
            var safeProjectName = IOUtil.GetValidFileName(projectName ?? "Project").Trim('_');
            if (string.IsNullOrWhiteSpace(safeProjectName))
                safeProjectName = "Project";

            var taskName = CreateUniqueTaskName(BuildDefaultTaskName(projectName));
            var taskDirectory = BuildTaskDirectory(safeProjectName);
            var requestFile = Path.Combine(taskDirectory, "request.json");
            var responseTemplate = Path.Combine(taskDirectory, "logs", "%NewUID(8,0)%", "flowblox_response.json");

            var item = new FlowBloxTaskItemViewModel
            {
                IsNew = true,
                TaskName = taskName,
                OriginalTaskName = taskName,
                ProjectName = projectName,
                ProjectFile = projectFile,
                ProjectSpaceGuid = projectSpaceGuid,
                ProjectSpaceVersion = projectSpaceVersion,
                IsEnabled = true,
                ScheduleType = FlowBloxTaskScheduleType.Manual,
                StartAt = DateTime.Today.AddHours(8),
                IntervalMinutes = 60,
                TaskDirectory = taskDirectory,
                RequestFilePath = requestFile,
                ResponseFilePathTemplate = responseTemplate
            }.EnableChangeTracking();

            AddTaskItem(item, markDirty: true);
            SelectedTask = item;
        }

        private void AddTaskItem(FlowBloxTaskItemViewModel item, bool markDirty)
        {
            item.PropertyChanged += TaskItem_PropertyChanged;
            Tasks.Add(item);

            if (markDirty)
            {
                item.MarkDirty();
                IsDirty = true;
            }
        }

        private void UnsubscribeTaskItems()
        {
            foreach (var task in Tasks)
                task.PropertyChanged -= TaskItem_PropertyChanged;
        }

        private void TaskItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FlowBloxTaskItemViewModel.IsDirty) &&
                sender is FlowBloxTaskItemViewModel { IsDirty: true })
            {
                IsDirty = true;
            }

            if (e.PropertyName is nameof(FlowBloxTaskItemViewModel.IsDirty)
                or nameof(FlowBloxTaskItemViewModel.IsNew)
                or nameof(FlowBloxTaskItemViewModel.IsRunning))
            {
                RunTaskCommand.Invalidate();
                StopTaskCommand.Invalidate();
            }
        }

        private string CreateUniqueTaskName(string baseName)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "FlowBlox Project";
            else
                baseName = baseName.Trim();

            var name = baseName;
            var index = 2;
            while (Tasks.Any(x => string.Equals(x.TaskName, name, StringComparison.OrdinalIgnoreCase)))
            {
                name = $"{baseName} ({index})";
                index++;
            }

            return name;
        }

        private static string BuildDefaultTaskName(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                return "FlowBlox Project";

            return $"FlowBlox Project - {projectName.Trim()}";
        }

        private static bool IsValidTaskName(string taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName))
                return false;

            var trimmed = taskName.Trim();
            return string.Equals(taskName, trimmed, StringComparison.Ordinal) &&
                string.Equals(Path.GetFileName(taskName), taskName, StringComparison.Ordinal) &&
                taskName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        private static string BuildTaskDirectory(string safeProjectName)
        {
            var baseDir = FlowBloxOptions.GetOptionInstance().GetOption("Paths.ScheduledTasksDir")?.Value;
            if (string.IsNullOrWhiteSpace(baseDir))
                baseDir = @"%localappdata%\FlowBlox\tasks";

            baseDir = Environment.ExpandEnvironmentVariables(baseDir);
            return Path.Combine(baseDir, safeProjectName);
        }

        private void RemoveTask()
        {
            if (SelectedTask == null)
                return;

            SelectedTask.IsDeleted = true;
            SelectedTask.PropertyChanged -= TaskItem_PropertyChanged;
            Tasks.Remove(SelectedTask);
            IsDirty = true;
            SelectedTask = Tasks.FirstOrDefault();
        }

        private async void RunTask()
        {
            if (SelectedTask == null || SelectedTask.IsNew)
                return;

            try
            {
                if (SelectedTask.IsDirty)
                {
                    await ShowNotification("Message_SaveBeforeRun");
                    return;
                }

                if (SelectedTask.IsRunning)
                {
                    await ShowNotification("Message_TaskAlreadyRunning");
                    return;
                }

                if (await _taskManagementService.IsTaskRunningAsync(SelectedTask.OriginalTaskName))
                {
                    SelectedTask.IsRunning = true;
                    await ShowNotification("Message_TaskAlreadyRunning");
                    return;
                }

                await _taskManagementService.RunTaskAsync(SelectedTask.OriginalTaskName);
                SelectedTask.IsRunning = true;
            }
            catch (Exception ex)
            {
                await ShowSchedulerError("Error_RunTaskFailed", ex);
            }
        }

        private async void StopTask()
        {
            if (SelectedTask == null || SelectedTask.IsNew)
                return;

            try
            {
                if (SelectedTask.IsDirty)
                {
                    await ShowNotification("Message_SaveBeforeStop");
                    return;
                }

                var isActuallyRunning = await _taskManagementService.IsTaskRunningAsync(SelectedTask.OriginalTaskName);
                if (!SelectedTask.IsRunning || !isActuallyRunning)
                {
                    if (!isActuallyRunning)
                        SelectedTask.IsRunning = false;

                    await ShowNotification("Message_TaskNotRunning");
                    return;
                }

                await _taskManagementService.StopTaskAsync(SelectedTask.OriginalTaskName);
                SelectedTask.IsRunning = false;
            }
            catch (Exception ex)
            {
                await ShowSchedulerError("Error_StopTaskFailed", ex);
            }
        }

        private void ToggleEnabled()
        {
            if (SelectedTask == null)
                return;

            SelectedTask.IsEnabled = !SelectedTask.IsEnabled;
        }

        private async void Apply()
        {
            if (!ValidateTasks())
                return;

            try
            {
                IsBusy = true;

                var currentTasks = Tasks.ToList();
                var currentOriginalNames = currentTasks
                    .Where(x => !x.IsNew)
                    .Select(x => x.OriginalTaskName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var schedulerTasks = await _taskManagementService.GetTasksAsync();
                foreach (var schedulerTask in schedulerTasks.Where(x => !currentOriginalNames.Contains(x.TaskName)))
                    await _taskManagementService.DeleteTaskAsync(schedulerTask.TaskName);

                foreach (var task in currentTasks)
                {
                    EnsureTaskFiles(task);

                    if (!task.IsNew && !string.Equals(task.OriginalTaskName, task.TaskName, StringComparison.OrdinalIgnoreCase))
                        await _taskManagementService.DeleteTaskAsync(task.OriginalTaskName);

                    if (task.IsNew)
                        await _taskManagementService.CreateTaskAsync(task.ToModel());
                    else
                        await _taskManagementService.UpdateTaskAsync(task.ToModel());
                }

                foreach (var task in currentTasks)
                    task.AcceptChanges();

                IsDirty = false;
                await ShowNotification("Message_SaveSuccessful");
                await LoadTasksAsync(confirmDiscardChanges: false);
            }
            catch (Exception ex)
            {
                await ShowSchedulerError("Error_SaveFailed", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateTasks()
        {
            var duplicates = Tasks
                .GroupBy(x => x.TaskName?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => string.IsNullOrWhiteSpace(g.Key) || g.Count() > 1);

            if (duplicates != null)
            {
                _ = ShowError("Error_DuplicateTaskName", duplicates.Key);
                return false;
            }

            foreach (var task in Tasks)
            {
                if (!IsValidTaskName(task.TaskName))
                {
                    _ = ShowError("Error_InvalidTaskName", task.TaskName);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(task.ProjectFile) && string.IsNullOrWhiteSpace(task.ProjectSpaceGuid))
                {
                    _ = ShowError("Error_ProjectReferenceMissing", task.TaskName);
                    return false;
                }
            }

            return true;
        }

        private static void EnsureTaskFiles(FlowBloxTaskItemViewModel task)
        {
            Directory.CreateDirectory(task.TaskDirectory);
            Directory.CreateDirectory(Path.Combine(task.TaskDirectory, "logs"));

            if (string.IsNullOrWhiteSpace(task.RequestFilePath))
                task.RequestFilePath = Path.Combine(task.TaskDirectory, "request.json");

            if (string.IsNullOrWhiteSpace(task.ResponseFilePathTemplate))
                task.ResponseFilePathTemplate = Path.Combine(task.TaskDirectory, "logs", "%NewUID(8,0)%", "flowblox_response.json");

            var request = new RunnerRequest
            {
                ProjectFile = string.IsNullOrWhiteSpace(task.ProjectFile) ? null : task.ProjectFile,
                ProjectSpaceGuid = string.IsNullOrWhiteSpace(task.ProjectSpaceGuid) ? null : task.ProjectSpaceGuid,
                ProjectSpaceVersion = task.ProjectSpaceVersion,
                AutoRestart = false,
                AbortOnError = true,
                AbortOnWarning = false,
                OptionOverrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Paths.RuntimeLogDir"] = Path.Combine(task.TaskDirectory, "logs", "%NewUID(8,0)%")
                }
            };

            RunnerJson.WriteFile(task.RequestFilePath, request);
        }

        private void RefreshSessions()
        {
            SelectedSession = null;

            if (SelectedTask == null || string.IsNullOrWhiteSpace(SelectedTask.TaskDirectory))
            {
                Sessions = new ObservableCollection<FlowBloxScheduledSession>();
                OnPropertyChanged(nameof(Sessions));
                return;
            }

            if (!_sessionsCache.TryGetValue(SelectedTask.TaskName, out var cached))
            {
                cached = LoadSessions(SelectedTask.TaskDirectory);
                _sessionsCache[SelectedTask.TaskName] = cached;
            }

            Sessions = cached;
            OnPropertyChanged(nameof(Sessions));
        }

        private void RefreshSessionsFromDisk()
        {
            if (SelectedTask == null)
                return;

            _sessionsCache.Remove(SelectedTask.TaskName);
            RefreshSessions();
        }

        private static ObservableCollection<FlowBloxScheduledSession> LoadSessions(string taskDirectory)
        {
            var logsDirectory = Path.Combine(taskDirectory, "logs");
            if (!Directory.Exists(logsDirectory))
                return new ObservableCollection<FlowBloxScheduledSession>();

            var sessions = Directory.GetDirectories(logsDirectory)
                .Select(dir =>
                {
                    var log = Directory.GetFiles(dir, "*.log", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    var response = Directory.GetFiles(dir, "flowblox_response.json", SearchOption.TopDirectoryOnly).FirstOrDefault()
                        ?? Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly).FirstOrDefault(x => x.EndsWith("response.json", StringComparison.OrdinalIgnoreCase));
                    var info = new DirectoryInfo(dir);
                    var session = new FlowBloxScheduledSession
                    {
                        CreatedAt = File.Exists(log) ? File.GetCreationTime(log) : info.CreationTime,
                        SessionId = info.Name,
                        SessionDirectory = dir,
                        LogFilePath = log,
                        ResponseFilePath = response
                    };

                    ApplySessionStatus(session);
                    return session;
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return new ObservableCollection<FlowBloxScheduledSession>(sessions);
        }

        private static void ApplySessionStatus(FlowBloxScheduledSession session)
        {
            if (string.IsNullOrWhiteSpace(session.ResponseFilePath) || !File.Exists(session.ResponseFilePath))
            {
                session.Status = FlowBloxScheduledSessionStatus.Running;
                session.StatusText = L("SessionStatus_Running");
                session.StatusIconKind = "ProgressClock";
                session.StatusBrush = "#2F6DB3";
                session.Message = L("SessionMessage_Running");
                return;
            }

            try
            {
                var response = RunnerJson.ReadFile<RunnerResponse>(session.ResponseFilePath);
                session.Status = response?.Success == true
                    ? FlowBloxScheduledSessionStatus.Successful
                    : FlowBloxScheduledSessionStatus.Failed;
                session.StatusText = response?.Success == true
                    ? L("SessionStatus_Successful")
                    : L("SessionStatus_Failed");
                session.StatusIconKind = response?.Success == true
                    ? "CheckCircleOutline"
                    : "AlertCircleOutline";
                session.StatusBrush = response?.Success == true ? "#2E8B57" : "#C94F4F";
                session.Message = response?.Success == true
                    ? L("SessionMessage_Successful")
                    : response?.ErrorMessage ?? response?.CancellationReason ?? response?.Exception ?? string.Empty;
            }
            catch (Exception ex)
            {
                session.Status = FlowBloxScheduledSessionStatus.Failed;
                session.StatusText = L("SessionStatus_Failed");
                session.StatusIconKind = "AlertCircleOutline";
                session.StatusBrush = "#C94F4F";
                session.Message = ex.Message;
            }
        }

        private void OpenTaskDirectory()
        {
            if (SelectedTask == null)
                return;

            Directory.CreateDirectory(SelectedTask.TaskDirectory);
            OpenDirectory(SelectedTask.TaskDirectory);
        }

        private void OpenSessionDirectory()
        {
            if (SelectedSession == null || string.IsNullOrWhiteSpace(SelectedSession.SessionDirectory))
                return;

            OpenDirectory(SelectedSession.SessionDirectory);
        }

        private void OpenSessionLogFile()
        {
            if (SelectedSession == null || string.IsNullOrWhiteSpace(SelectedSession.LogFilePath) || !File.Exists(SelectedSession.LogFilePath))
                return;

            var dialog = new MultiValueSelectionDialog(
                FlowBloxResourceUtil.GetLocalizedString("FileOpenMode_Dialog_Title", typeof(Core.FlowBloxTexts)),
                FlowBloxResourceUtil.GetLocalizedString("FileOpenMode_Dialog_Message", typeof(Core.FlowBloxTexts)),
                new GenericSelectionHandler<FileOpenMode>(
                    Enum.GetValues(typeof(FileOpenMode)).Cast<FileOpenMode>().ToList(),
                    mode => mode.GetDisplayName()));

            if (dialog.ShowDialog() == true)
            {
                switch (dialog.SelectedItem.Value)
                {
                    case FileOpenMode.FlowBloxEditor:
                        FlowBloxEditingHelper.OpenUsingEditor(SelectedSession.LogFilePath);
                        break;
                    case FileOpenMode.WindowsDefaultApp:
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = SelectedSession.LogFilePath,
                            UseShellExecute = true
                        });
                        break;
                }
            }
        }

        private static void OpenDirectory(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
        }

        private Task ShowNotification(string resourceKey)
        {
            if (_ownerWindow is MetroWindow mw)
                return MessageBoxHelper.ShowMessageBoxAsync(mw, MessageBoxType.Notification, L(resourceKey));

            return Task.CompletedTask;
        }

        private Task ShowError(string resourceKey, string details)
        {
            if (_ownerWindow is MetroWindow mw)
                return MessageBoxHelper.ShowMessageBoxAsync(mw, MessageBoxType.Error, ApiErrorMessageHelper.BuildErrorMessage(L(resourceKey), details));

            return Task.CompletedTask;
        }

        private Task ShowSchedulerError(string fallbackResourceKey, Exception exception)
        {
            if (IsPermissionException(exception))
                return ShowError("Error_TaskSchedulerPermissionDenied", exception.Message);

            return ShowError(fallbackResourceKey, exception.Message);
        }

        private static bool IsPermissionException(Exception exception)
        {
            while (exception != null)
            {
                if (exception is UnauthorizedAccessException or SecurityException)
                    return true;

                if (exception is COMException comException &&
                    (uint)comException.HResult == 0x80070005)
                {
                    return true;
                }

                exception = exception.InnerException;
            }

            return false;
        }

        private static string L(string key) => FlowBloxResourceUtil.GetLocalizedString(key, typeof(Resources.FlowBloxTaskManagementWindow));

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

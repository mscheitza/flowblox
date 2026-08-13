using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.TaskManagement;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class FlowBloxTaskItemViewModel : INotifyPropertyChanged
    {
        private string _taskName;
        private bool _isEnabled = true;
        private FlowBloxTaskScheduleType _scheduleType;
        private DateTime? _startAt;
        private int _intervalMinutes = 60;
        private string _projectName;
        private string _projectFile;
        private string _projectSpaceGuid;
        private int? _projectSpaceVersion;
        private DateTime? _nextRunTime;
        private DateTime? _lastRunTime;
        private int? _lastResult;
        private string _taskDirectory;
        private string _requestFilePath;
        private string _responseFilePathTemplate;
        private bool _isNew;
        private bool _isDeleted;
        private bool _isDirty;
        private bool _isRunning;
        private bool _isTrackingChanges;
        private bool _inputParametersLoaded;

        public string OriginalTaskName { get; set; }

        public string TaskName { get => _taskName; set { if (_taskName == value) return; _taskName = value; OnPropertyChanged(); MarkDirty(); } }
        public string DisplayName { get => TaskName; set => TaskName = value; }
        public bool IsEnabled { get => _isEnabled; set { if (_isEnabled == value) return; _isEnabled = value; OnPropertyChanged(); MarkDirty(); } }
        public FlowBloxTaskScheduleType ScheduleType
        {
            get => _scheduleType;
            set
            {
                if (_scheduleType == value)
                    return;

                _scheduleType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStartAtEnabled));
                OnPropertyChanged(nameof(IsIntervalEnabled));
                MarkDirty();
            }
        }
        public DateTime? StartAt { get => _startAt; set { if (_startAt == value) return; _startAt = value; OnPropertyChanged(); MarkDirty(); } }
        public int IntervalMinutes { get => _intervalMinutes; set { if (_intervalMinutes == value) return; _intervalMinutes = value; OnPropertyChanged(); MarkDirty(); } }
        public string ProjectName { get => _projectName; set { if (_projectName == value) return; _projectName = value; OnPropertyChanged(); MarkDirty(); } }
        public string ProjectFile { get => _projectFile; set { if (_projectFile == value) return; _projectFile = value; ClearLoadedProject(); OnPropertyChanged(); OnPropertyChanged(nameof(ProjectReference)); MarkDirty(); } }
        public string ProjectSpaceGuid { get => _projectSpaceGuid; set { if (_projectSpaceGuid == value) return; _projectSpaceGuid = value; ClearLoadedProject(); OnPropertyChanged(); OnPropertyChanged(nameof(ProjectReference)); MarkDirty(); } }
        public int? ProjectSpaceVersion { get => _projectSpaceVersion; set { if (_projectSpaceVersion == value) return; _projectSpaceVersion = value; ClearLoadedProject(); OnPropertyChanged(); OnPropertyChanged(nameof(ProjectReference)); MarkDirty(); } }
        public DateTime? NextRunTime { get => _nextRunTime; set { _nextRunTime = value; OnPropertyChanged(); } }
        public DateTime? LastRunTime { get => _lastRunTime; set { _lastRunTime = value; OnPropertyChanged(); } }
        public int? LastResult { get => _lastResult; set { _lastResult = value; OnPropertyChanged(); } }
        public bool IsRunning { get => _isRunning; set { _isRunning = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanStart)); } }
        public string TaskDirectory { get => _taskDirectory; set { if (_taskDirectory == value) return; _taskDirectory = value; OnPropertyChanged(); MarkDirty(); } }
        public string RequestFilePath { get => _requestFilePath; set { if (_requestFilePath == value) return; _requestFilePath = value; OnPropertyChanged(); MarkDirty(); } }
        public string ResponseFilePathTemplate { get => _responseFilePathTemplate; set { if (_responseFilePathTemplate == value) return; _responseFilePathTemplate = value; OnPropertyChanged(); MarkDirty(); } }
        public bool IsNew { get => _isNew; set { _isNew = value; OnPropertyChanged(); } }
        public bool IsDeleted { get => _isDeleted; set { _isDeleted = value; OnPropertyChanged(); } }
        public bool IsDirty { get => _isDirty; private set { _isDirty = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanStart)); } }
        public bool CanStart => !IsNew && !IsDirty && !IsRunning;
        public bool IsStartAtEnabled => ScheduleType is FlowBloxTaskScheduleType.Daily or FlowBloxTaskScheduleType.Interval;
        public bool IsIntervalEnabled => ScheduleType == FlowBloxTaskScheduleType.Interval;
        public FlowBloxProject LoadedProject { get; set; }
        public ObservableCollection<FlowBloxTaskInputParameterViewModel> InputParameters { get; } = new();
        public Dictionary<string, string> UserFields { get; } = new(StringComparer.OrdinalIgnoreCase);
        public bool InputParametersLoaded { get => _inputParametersLoaded; set { if (_inputParametersLoaded == value) return; _inputParametersLoaded = value; OnPropertyChanged(); } }

        public string ProjectReference
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ProjectFile))
                    return ProjectFile;

                if (!string.IsNullOrWhiteSpace(ProjectSpaceGuid))
                    return ProjectSpaceVersion.HasValue
                        ? $"{ProjectSpaceGuid} (v{ProjectSpaceVersion.Value})"
                        : ProjectSpaceGuid;

                return string.Empty;
            }
        }

        public FlowBloxScheduledTask ToModel()
        {
            return new FlowBloxScheduledTask
            {
                TaskName = TaskName,
                DisplayName = TaskName,
                IsEnabled = IsEnabled,
                ScheduleType = ScheduleType,
                StartAt = StartAt,
                Interval = ScheduleType == FlowBloxTaskScheduleType.Interval ? TimeSpan.FromMinutes(Math.Max(1, IntervalMinutes)) : null,
                IsRunning = IsRunning,
                ProjectName = ProjectName,
                ProjectFile = ProjectFile,
                ProjectSpaceGuid = ProjectSpaceGuid,
                ProjectSpaceVersion = ProjectSpaceVersion,
                TaskDirectory = TaskDirectory,
                RequestFilePath = RequestFilePath,
                ResponseFilePathTemplate = ResponseFilePathTemplate,
                UserFields = new Dictionary<string, string>(UserFields, StringComparer.OrdinalIgnoreCase)
            };
        }

        public static FlowBloxTaskItemViewModel FromModel(FlowBloxScheduledTask task)
        {
            var item = new FlowBloxTaskItemViewModel
            {
                OriginalTaskName = task.TaskName,
                TaskName = task.TaskName,
                IsEnabled = task.IsEnabled,
                ScheduleType = task.ScheduleType,
                StartAt = task.StartAt,
                IntervalMinutes = (int)Math.Max(1, (task.Interval ?? TimeSpan.FromHours(1)).TotalMinutes),
                ProjectName = task.ProjectName,
                ProjectFile = task.ProjectFile,
                ProjectSpaceGuid = task.ProjectSpaceGuid,
                ProjectSpaceVersion = task.ProjectSpaceVersion,
                NextRunTime = task.NextRunTime,
                LastRunTime = task.LastRunTime,
                LastResult = task.LastResult,
                IsRunning = task.IsRunning,
                TaskDirectory = task.TaskDirectory,
                RequestFilePath = task.RequestFilePath,
                ResponseFilePathTemplate = task.ResponseFilePathTemplate
            };

            foreach (var userField in task.UserFields ?? new Dictionary<string, string>())
                item.UserFields[userField.Key] = userField.Value;

            return item.EnableChangeTracking();
        }

        public void ClearLoadedProject()
        {
            LoadedProject = null;
            InputParametersLoaded = false;
            InputParameters.Clear();
        }

        public FlowBloxTaskItemViewModel EnableChangeTracking()
        {
            IsDirty = false;
            _isTrackingChanges = true;
            return this;
        }

        public void AcceptChanges()
        {
            OriginalTaskName = TaskName;
            IsNew = false;
            IsDirty = false;
            _isTrackingChanges = true;
        }

        public void MarkDirty()
        {
            if (!_isTrackingChanges)
                return;

            IsDirty = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

using FlowBlox.Core.Authentication;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public class CreateOrUpdatePSProjectViewModel : INotifyPropertyChanged
    {
        private readonly Window _ownerWindow;
        private readonly FlowBloxProject _project;
        private readonly string _initialProjectGuid;
        private string _backupProjectGuid;
        private bool _suppressToggleEffects;

        public RelayCommand CloseCommand { get; }
        public RelayCommand SaveCommand { get; }

        private string _projectGuid;
        public string ProjectGuid
        {
            get => _projectGuid;
            set
            {
                if (_projectGuid == value)
                    return;

                _projectGuid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasProjectGuid));
            }
        }

        public bool HasProjectGuid => !string.IsNullOrWhiteSpace(ProjectGuid);

        public bool HasInitialProjectGuid => !string.IsNullOrWhiteSpace(_initialProjectGuid);

        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName == value)
                    return;

                _projectName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private string _projectDescription;
        public string ProjectDescription
        {
            get => _projectDescription;
            set
            {
                if (_projectDescription == value)
                    return;

                _projectDescription = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private FbProjectVisibility _visibility = FbProjectVisibility.Private;
        public FbProjectVisibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility == value)
                    return;

                _visibility = value;
                OnPropertyChanged();
            }
        }

        private string _createdAtText;
        public string CreatedAtText
        {
            get => _createdAtText;
            set
            {
                if (_createdAtText == value)
                    return;

                _createdAtText = value;
                OnPropertyChanged();
            }
        }

        private string _updatedAtText;
        public string UpdatedAtText
        {
            get => _updatedAtText;
            set
            {
                if (_updatedAtText == value)
                    return;

                _updatedAtText = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdateExistingSelected;
        public bool IsUpdateExistingSelected
        {
            get => _isUpdateExistingSelected;
            set
            {
                if (_isUpdateExistingSelected == value)
                    return;

                _isUpdateExistingSelected = value;
                OnPropertyChanged();

                if (_suppressToggleEffects || !value)
                    return;

                _suppressToggleEffects = true;
                IsCreateNewSelected = false;
                _suppressToggleEffects = false;

                ApplyUpdateExistingMode();
            }
        }

        private bool _isCreateNewSelected;
        public bool IsCreateNewSelected
        {
            get => _isCreateNewSelected;
            set
            {
                if (_isCreateNewSelected == value)
                    return;

                _isCreateNewSelected = value;
                OnPropertyChanged();

                if (_suppressToggleEffects || !value)
                    return;

                _suppressToggleEffects = true;
                IsUpdateExistingSelected = false;
                _suppressToggleEffects = false;

                ApplyCreateNewMode();
            }
        }


        public bool CanSave => ActiveUser != null
                               && !string.IsNullOrWhiteSpace(UserToken)
                               && !string.IsNullOrWhiteSpace(ProjectName)
                               && !string.IsNullOrWhiteSpace(ProjectDescription);

        public FbUserData ActiveUser
        {
            get => FlowBloxAccountManager.Instance.GetActiveUser(ApiUrl);
            set
            {
                FlowBloxAccountManager.Instance.SetActiveUser(ApiUrl, value);
                OnPropertyChanged(nameof(ActiveUser));
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public string UserToken
        {
            get => FlowBloxAccountManager.Instance.GetUserToken(ApiUrl);
            set
            {
                FlowBloxAccountManager.Instance.SetUserToken(ApiUrl, value);
                OnPropertyChanged(nameof(UserToken));
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private static string ApiUrl => FlowBloxOptions.GetOptionInstance()
            .OptionCollection["General.ProjectApiServiceBaseUrl"]
            .Value;

        private readonly Lazy<FlowBloxWebApiService> _flowBloxWebApiService = new Lazy<FlowBloxWebApiService>(() =>
        {
            return new FlowBloxWebApiService(ApiUrl);
        });

        public CreateOrUpdatePSProjectViewModel()
        {
            CloseCommand = new RelayCommand(() => _ownerWindow?.Close());
            SaveCommand = new RelayCommand(async () => await ExecuteSaveAsync());
        }

        public CreateOrUpdatePSProjectViewModel(Window ownerWindow, FlowBloxProject project) : this()
        {
            _ownerWindow = ownerWindow;
            _project = project;

            _initialProjectGuid = project.ProjectSpaceGuid;
            ProjectGuid = _initialProjectGuid;

            _suppressToggleEffects = true;
            IsUpdateExistingSelected = HasInitialProjectGuid;
            IsCreateNewSelected = !HasInitialProjectGuid;
            _suppressToggleEffects = false;

            OnPropertyChanged(nameof(HasInitialProjectGuid));

            InitializeFromLocalOrRemoteAsync();
        }

        private async Task ShowErrorAsync(string message) => await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, message);
        private async Task ShowNotificationAsync(string message) => await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Notification, message);


        private void ApplyCreateNewMode()
        {
            if (!string.IsNullOrWhiteSpace(ProjectGuid))
                _backupProjectGuid = ProjectGuid;

            ProjectGuid = null;
            CreatedAtText = string.Empty;
            UpdatedAtText = string.Empty;
            Visibility = FbProjectVisibility.Private;

            InitializeFromLocalOrRemoteAsync();
        }

        private void ApplyUpdateExistingMode()
        {
            var restoreGuid = !string.IsNullOrWhiteSpace(_initialProjectGuid)
                ? _initialProjectGuid
                : _backupProjectGuid;

            if (string.IsNullOrWhiteSpace(restoreGuid))
            {
                _suppressToggleEffects = true;
                IsCreateNewSelected = true;
                IsUpdateExistingSelected = false;
                _suppressToggleEffects = false;

                ApplyCreateNewMode();
                return;
            }

            ProjectGuid = restoreGuid;
            _project.ProjectSpaceGuid = restoreGuid;

            InitializeFromLocalOrRemoteAsync();
        }

        private async void InitializeFromLocalOrRemoteAsync()
        {
            if (!string.IsNullOrWhiteSpace(ProjectGuid) && !string.IsNullOrWhiteSpace(UserToken))
            {
                var remoteResp = await _flowBloxWebApiService.Value.GetProjectAsync(
                    new FbProjectRequest { Guid = ProjectGuid },
                    UserToken);

                if (remoteResp.Success && remoteResp.ResultObject != null)
                {
                    var remote = remoteResp.ResultObject;

                    ProjectName = remote.Name;
                    ProjectDescription = remote.Description;
                    Visibility = remote.Visibility;
                    CreatedAtText = remote.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
                    UpdatedAtText = remote.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
                    return;
                }
                else if (!remoteResp.Success)
                {
                    await ShowErrorAsync(ApiErrorMessageHelper.BuildErrorMessage(remoteResp.ErrorMessage));
                }
            }

            // Fallback: local project defaults
            ProjectName = _project.ProjectName;
            ProjectDescription = _project.ProjectDescription;
            CreatedAtText = string.Empty;
            UpdatedAtText = string.Empty;
        }

        private async Task ExecuteSaveAsync()
        {
            if (ActiveUser == null || string.IsNullOrWhiteSpace(UserToken))
            {
                await ShowErrorAsync(FlowBloxResourceUtil.GetLocalizedString("Error_NotLoggedIn", typeof(Resources.CreateOrUpdatePSProjectWindow)));
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(ProjectGuid))
                {
                    // Create project metadata
                    var create = await _flowBloxWebApiService.Value.CreateProjectAsync(UserToken, new FbProjectCreateRequest
                    {
                        Name = ProjectName,
                        Description = ProjectDescription,
                        Visibility = Visibility
                    });

                    if (create == null || !create.Success || string.IsNullOrWhiteSpace(create.ProjectGuid))
                    {
                        await ShowErrorAsync(
                           ApiErrorMessageHelper.BuildErrorMessage(
                               FlowBloxResourceUtil.GetLocalizedString("Error_CreateFailed", typeof(Resources.CreateOrUpdatePSProjectWindow)),
                               create?.ErrorMessage));

                        return;
                    }

                    ProjectGuid = create.ProjectGuid;
                    _project.ProjectSpaceGuid = ProjectGuid;
                }
                else
                {
                    // Update metadata
                    var updateMeta = await _flowBloxWebApiService.Value.UpdateProjectAsync(UserToken, new FbProjectChangeRequest
                    {
                        ProjectGuid = ProjectGuid,
                        Name = ProjectName,
                        Description = ProjectDescription,
                        Visibility = Visibility
                    });

                    if (updateMeta == null || !updateMeta.Success)
                    {
                        await ShowErrorAsync(
                           ApiErrorMessageHelper.BuildErrorMessage(
                               FlowBloxResourceUtil.GetLocalizedString("Error_UpdateFailed", typeof(Resources.CreateOrUpdatePSProjectWindow)),
                               updateMeta?.ErrorMessage));

                        return;
                    }
                }

                // Upload content ZIP
                var saveContent = await _project.SaveToProjectSpaceAsync(ProjectGuid, UserToken, _flowBloxWebApiService.Value);
                if (saveContent == null || !saveContent.Success)
                {
                    await ShowErrorAsync(ApiErrorMessageHelper.BuildErrorMessage(
                        FlowBloxResourceUtil.GetLocalizedString("Error_UploadFailed", typeof(Resources.CreateOrUpdatePSProjectWindow)),
                        saveContent?.ErrorMessage));

                    return;
                }

                // Refresh timestamps
                var remoteResp = await _flowBloxWebApiService.Value.GetProjectAsync(new FbProjectRequest { Guid = ProjectGuid }, UserToken);
                if (remoteResp.Success && remoteResp.ResultObject != null)
                {
                    var remote = remoteResp.ResultObject;

                    CreatedAtText = remote.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
                    UpdatedAtText = remote.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
                }
                else if (!remoteResp.Success)
                {
                    await ShowErrorAsync(ApiErrorMessageHelper.BuildErrorMessage(
                        FlowBloxResourceUtil.GetLocalizedString("Error_RefreshMetadataFailed", typeof(Resources.CreateOrUpdatePSProjectWindow)),
                        saveContent?.ErrorMessage));

                    return;
                }

                await ShowNotificationAsync(
                    FlowBloxResourceUtil.GetLocalizedString("Message_SaveSuccessful", typeof(Resources.CreateOrUpdatePSProjectWindow)));

                _ownerWindow.DialogResult = true;
                _ownerWindow.Close();
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                await ShowErrorAsync(ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
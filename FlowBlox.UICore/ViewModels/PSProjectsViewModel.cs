using FlowBlox.Core.Authentication;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public class PSProjectsViewModel : INotifyPropertyChanged
    {
        private Window _ownerWindow;

        private string _searchText;
        private bool _isMine;
        private FbProjectResult _selectedProject;
        private List<FbProjectResult> _searchResults;

        public RelayCommand CloseCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand OpenProjectCommand { get; }
        public RelayCommand EditProjectCommand { get; }
        public RelayCommand LoginCommand { get; }
        public RelayCommand RegisterCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand EditUserPropertiesCommand { get; }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public bool IsMine
        {
            get => _isMine;
            set
            {
                if (_isMine == value) return;
                _isMine = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEditProject));
                ExecuteSearch();
            }
        }

        public List<FbProjectResult> SearchResults
        {
            get => _searchResults;
            set { _searchResults = value; OnPropertyChanged(); }
        }

        public FbProjectResult SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject == value) return;
                _selectedProject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProjectSelected));
                OnPropertyChanged(nameof(CanEditProject));
            }
        }

        public bool IsProjectSelected => SelectedProject != null;

        public FbUserData ActiveUser
        {
            get => FlowBloxAccountManager.Instance.GetActiveUser(ApiUrl);
            set
            {
                FlowBloxAccountManager.Instance.SetActiveUser(ApiUrl, value);
                OnPropertyChanged(nameof(ActiveUser));
                OnPropertyChanged(nameof(CanLogin));
                OnPropertyChanged(nameof(CanLogout));
                OnPropertyChanged(nameof(CanToggleMyProjects));
            }
        }

        public string UserToken
        {
            get => FlowBloxAccountManager.Instance.GetUserToken(ApiUrl);
            set
            {
                FlowBloxAccountManager.Instance.SetUserToken(ApiUrl, value);
                OnPropertyChanged(nameof(UserToken));
            }
        }

        private FbApiMetadata _apiMetadata;
        public FbApiMetadata ApiMetadata
        {
            get => _apiMetadata;
            private set
            {
                if (!ReferenceEquals(_apiMetadata, value))
                {
                    _apiMetadata = value;
                    OnPropertyChanged(nameof(ApiMetadata));
                    OnPropertyChanged(nameof(CanLogin));
                    OnPropertyChanged(nameof(CanRegister));
                    OnPropertyChanged(nameof(CanLogout));
                    OnPropertyChanged(nameof(CanToggleMyProjects));
                }
            }
        }

        public bool CanLogin => ApiMetadata?.Capabilities?.CanLogin == true && ActiveUser == null;
        public bool CanRegister => ApiMetadata?.Capabilities?.CanRegister == true;
        public bool CanLogout => ApiMetadata?.Capabilities?.CanLogin == true && ActiveUser != null;
        public bool CanToggleMyProjects => ApiMetadata?.Capabilities?.CanLogin == true && ActiveUser != null;
        public bool CanEditUserProperties => ApiMetadata?.Capabilities?.CanLogin == true && ActiveUser != null && !string.IsNullOrWhiteSpace(UserToken);

        private static string ApiUrl => FlowBloxOptions.GetOptionInstance()
            .OptionCollection["General.ProjectApiServiceBaseUrl"]
            .Value;

        private Lazy<FlowBloxWebApiService> _flowBloxWebApiService = new Lazy<FlowBloxWebApiService>(() =>
        {
            return new FlowBloxWebApiService(ApiUrl);
        });

        public PSProjectsViewModel()
        {
            SearchResults = new List<FbProjectResult>();

            CloseCommand = new RelayCommand(() => _ownerWindow?.Close());
            SearchCommand = new RelayCommand(ExecuteSearch);
            OpenProjectCommand = new RelayCommand(OpenSelectedProject, CanOpenProject);
            EditProjectCommand = new RelayCommand(EditSelectedProject, CanEditProject);

            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(() =>
            {
                var registrationWindow = new RegistrationWindow(ApiUrl)
                {
                    Owner = this._ownerWindow
                };
                registrationWindow.ShowDialog();
            });
            LogoutCommand = new RelayCommand(() =>
            {
                ActiveUser = null;
                UserToken = null;

                if (IsMine)
                    IsMine = false;
                else
                    ExecuteSearch();
            });
            EditUserPropertiesCommand = new RelayCommand(_ => ExecuteEditUserProperties(), _ => CanEditUserProperties);
        }

        private void EditSelectedProject(object obj)
        {
            if (!IsProjectSelected)
                return;

            if (!IsMine)
                return;

            var editWindow = new EditProjectMetadataWindow(_ownerWindow, _flowBloxWebApiService.Value, UserToken, SelectedProject)
            {
                Owner = _ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (editWindow.ShowDialog() == true)
            {
                OnPropertyChanged(nameof(SelectedProject));
                OnPropertyChanged(nameof(SearchResults));
            }
        }

        private async Task LoadApiMetadataAsync()
        {
            ApiMetadata = await ApiMetadataHelper.LoadApiMetadataAsync(_flowBloxWebApiService.Value);
        }

        private bool CanOpenProject(object arg) => IsProjectSelected;

        private bool CanEditProject(object arg) => IsProjectSelected && IsMine;

        public PSProjectsViewModel(Window ownerWindow) : this()
        {
            _ownerWindow = ownerWindow;
            ExecuteSearch();
            _ = LoadApiMetadataAsync();
        }

        private void ExecuteLogin()
        {
            var loginWindow = new LoginWindow
            {
                Owner = _ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (loginWindow.ShowDialog() == true)
            {
                if (loginWindow.Tag != null)
                {
                    var (token, userData) = ((string, FbUserData))loginWindow.Tag;
                    ActiveUser = userData;
                    UserToken = token;
                }
                else
                {
                    ActiveUser = null;
                    UserToken = null;
                }

                ExecuteSearch();
            }
        }

        private void ExecuteEditUserProperties()
        {
            if (!CanEditUserProperties || _ownerWindow == null)
                return;

            var editUserPropertiesWindow = new EditUserPropertiesWindow(_flowBloxWebApiService.Value, UserToken, ActiveUser)
            {
                Owner = _ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editUserPropertiesWindow.ShowDialog();
            OnPropertyChanged(nameof(ActiveUser));
        }

        private async void ExecuteSearch()
        {
            var token = IsMine ? UserToken : "";

            var resp = await _flowBloxWebApiService.Value.GetProjectsAsync(
                userToken: token,
                mine: IsMine,
                searchForName: SearchText ?? "");

            if (!resp.Success)
            {
                await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error,
                    ApiErrorMessageHelper.BuildErrorMessage(
                        FlowBloxResourceUtil.GetLocalizedString("Error_SearchProjectsFailed", typeof(Resources.CreateOrUpdatePSProjectWindow)),
                        resp.ErrorMessage));

                return;
            }

            if (resp.ResultObject != null)
            {
                SearchResults = resp.ResultObject;
                SelectedProject = null;
            }
        }

        private void OpenSelectedProject(object obj)
        {
            OpenSelectedProject();
        }

        private void OpenSelectedProject()
        {
            if (!IsProjectSelected || _ownerWindow == null)
                return;

            // Return the guid to caller
            _ownerWindow.Tag = SelectedProject.Guid;
            _ownerWindow.DialogResult = true;
            _ownerWindow.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels
{
    public class EditProjectMetadataViewModel : INotifyPropertyChanged
    {
        private readonly MetroWindow _window;
        private readonly FlowBloxWebApiService _webApiService;
        private readonly string _userToken;
        private readonly FbProjectResult _project;

        private string _projectName;
        private string _projectDescription;
        private FbProjectVisibility _visibility;
        private string _errorText;

        public RelayCommand CloseCommand { get; }
        public RelayCommand SaveCommand { get; }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public string ProjectDescription
        {
            get => _projectDescription;
            set
            {
                _projectDescription = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public FbProjectVisibility Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public List<FbProjectVisibility> VisibilityValues { get; }

        public string ErrorText
        {
            get => _errorText;
            private set
            {
                _errorText = value;
                OnPropertyChanged();
            }
        }

        public bool CanSave
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_userToken))
                    return false;

                if (string.IsNullOrWhiteSpace(ProjectName))
                    return false;

                return true;
            }
        }

        public EditProjectMetadataViewModel()
        {
            CloseCommand = new RelayCommand(() => _window?.Close());
            SaveCommand = new RelayCommand(async () => await SaveAsync());
        }

        public EditProjectMetadataViewModel(MetroWindow window, FlowBloxWebApiService webApiService, string userToken, FbProjectResult project) : this()
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            _window = window;
            _webApiService = webApiService;
            _userToken = userToken;
            _project = project;

            ProjectName = project.Name ?? string.Empty;
            ProjectDescription = project.Description ?? string.Empty;

            VisibilityValues = Enum.GetValues(typeof(FbProjectVisibility))
                .Cast<FbProjectVisibility>()
                .Except([FbProjectVisibility.Dedicated])
                .ToList();

            Visibility = project.Visibility;
        }

        private async Task SaveAsync()
        {
            ErrorText = null;

            var updateMeta = await _webApiService.UpdateProjectAsync(
                _userToken,
                new FbProjectChangeRequest
                {
                    ProjectGuid = _project.Guid,
                    Name = ProjectName,
                    Description = ProjectDescription,
                    Visibility = Visibility
                });

            if (updateMeta == null || !updateMeta.Success)
            {
                ErrorText = ApiErrorMessageHelper.BuildErrorMessage(
                    FlowBloxResourceUtil.GetLocalizedString("Error_EditProjectMetadata_SaveFailed", typeof(Resources.EditProjectMetadataWindow)),
                    updateMeta?.ErrorMessage);

                return;
            }

            // Apply changes locally (no additional API call)
            _project.Name = ProjectName;
            _project.Description = ProjectDescription;
            _project.Visibility = Visibility;

            _window.DialogResult = true;
            _window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

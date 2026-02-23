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
    public class EditProjectVersionMetadataViewModel : INotifyPropertyChanged
    {
        private readonly MetroWindow _window;
        private readonly FlowBloxWebApiService _webApiService;
        private readonly string _userToken;
        private readonly FbProjectResult _project;
        private readonly FbProjectVersionResult _version;

        private string _comment;
        private string _errorText;

        public RelayCommand CloseCommand { get; }
        public RelayCommand SaveCommand { get; }

        public string VersionText => $"{_version?.VersionNumber}";

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public string ErrorText
        {
            get => _errorText;
            private set
            {
                _errorText = value;
                OnPropertyChanged();
            }
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(_userToken);

        public EditProjectVersionMetadataViewModel()
        {
            CloseCommand = new RelayCommand(() => _window?.Close());
            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => CanSave);
        }

        public EditProjectVersionMetadataViewModel(
            MetroWindow window,
            FlowBloxWebApiService webApiService,
            string userToken,
            FbProjectResult project,
            FbProjectVersionResult version) : this()
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _webApiService = webApiService ?? throw new ArgumentNullException(nameof(webApiService));
            _userToken = userToken;
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _version = version ?? throw new ArgumentNullException(nameof(version));

            Comment = _version.Comment ?? string.Empty;
        }

        private async Task SaveAsync()
        {
            ErrorText = null;

            var resp = await _webApiService.UpdateProjectVersionMetadataAsync(_userToken, new FbProjectVersionChangeRequest
            {
                ProjectGuid = _project.Guid,
                Version = _version.VersionNumber,
                Comment = Comment
            });

            if (resp == null || !resp.Success)
            {
                ErrorText = ApiErrorMessageHelper.BuildErrorMessage(
                    FlowBloxResourceUtil.GetLocalizedString("Error_EditProjectVersion_SaveFailed", typeof(Resources.EditProjectVersionMetadataWindow)),
                    resp?.ErrorMessage);

                return;
            }

            // Update locally
            _version.Comment = Comment;

            _window.DialogResult = true;
            _window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

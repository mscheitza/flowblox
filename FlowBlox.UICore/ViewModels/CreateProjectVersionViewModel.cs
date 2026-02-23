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
    public class CreateProjectVersionViewModel : INotifyPropertyChanged
    {
        private readonly MetroWindow _window;
        private readonly FlowBloxWebApiService _webApiService;
        private readonly string _userToken;
        private readonly FbProjectResult _project;

        private string _comment;
        private string _errorText;

        public RelayCommand CloseCommand { get; }
        public RelayCommand CreateCommand { get; }

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreate));
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

        public bool CanCreate => !string.IsNullOrWhiteSpace(_userToken);

        public CreateProjectVersionViewModel()
        {
            CloseCommand = new RelayCommand(() => _window?.Close());
            CreateCommand = new RelayCommand(async () => await CreateAsync(), () => CanCreate);
        }

        public CreateProjectVersionViewModel(MetroWindow window, FlowBloxWebApiService webApiService, string userToken, FbProjectResult project)
            : this()
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _webApiService = webApiService ?? throw new ArgumentNullException(nameof(webApiService));
            _userToken = userToken;
            _project = project ?? throw new ArgumentNullException(nameof(project));

            Comment = string.Empty;
        }

        private async Task CreateAsync()
        {
            ErrorText = null;

            if (string.IsNullOrWhiteSpace(_userToken))
            {
                ErrorText = FlowBloxResourceUtil.GetLocalizedString(
                    "Error_CreateProjectVersion_NotLoggedIn",
                    typeof(Resources.CreateProjectVersionWindow));

                return;
            }

            var resp = await _webApiService.CreateProjectVersionAsync(_userToken, new FbCreateProjectVersionRequest
            {
                ProjectGuid = _project.Guid,
                Comment = Comment
            });

            if (resp == null || !resp.Success || resp.ResultObject <= 0)
            {
                ErrorText = ApiErrorMessageHelper.BuildErrorMessage(
                    FlowBloxResourceUtil.GetLocalizedString("Error_CreateProjectVersion_Failed", typeof(Resources.CreateProjectVersionWindow)),
                    resp?.ErrorMessage);

                return;
            }

            // Return created version to caller via Tag
            _window.Tag = resp.ResultObject;
            _window.DialogResult = true;
            _window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

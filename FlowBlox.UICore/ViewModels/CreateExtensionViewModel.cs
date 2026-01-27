using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class CreateExtensionViewModel : INotifyPropertyChanged
    {
        private string _extensionName;
        private string _extensionDescription;
        private string _userToken;
        private MetroWindow _window;

        private Lazy<FlowBloxWebApiService> _flowBloxWebApiService = new Lazy<FlowBloxWebApiService>(() =>
        {
            var webApiServiceUrl = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ExtensionApiServiceBaseUrl"].Value;
            return new FlowBloxWebApiService(webApiServiceUrl);
        });

        public string ExtensionName
        {
            get => _extensionName;
            set
            {
                _extensionName = value;
                OnPropertyChanged(nameof(ExtensionName));
                OnPropertyChanged(nameof(CanCreate));
            }
        }

        public string ExtensionDescription
        {
            get => _extensionDescription;
            set
            {
                _extensionDescription = value;
                OnPropertyChanged(nameof(ExtensionDescription));
                OnPropertyChanged(nameof(CanCreate));
            }
        }

        public bool CanCreate =>
            !string.IsNullOrEmpty(ExtensionName) &&
            !string.IsNullOrEmpty(ExtensionDescription);

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public CreateExtensionViewModel()
        {
            CreateCommand = new RelayCommand(async () => await CreateAsync());
            CancelCommand = new RelayCommand(Cancel);
        }

        public CreateExtensionViewModel(MetroWindow window, string userToken) : this()
        {
            this._userToken = userToken;
            this._window = window;
        }

        private async Task CreateAsync()
        {
            var request = new FbCreateExtensionRequest
            {
                Name = _extensionName,
                Description = _extensionDescription
            };

            var response = await _flowBloxWebApiService.Value.CreateExtensionAsync(_userToken, request);

            if (response.Success)
            {
                _window.DialogResult = true;
                _window.Close();
            }
            else
            {
                await MessageBoxHelper.ShowMessageBoxAsync(_window, MessageBoxType.Error, ApiErrorMessageHelper.BuildErrorMessage(response.ErrorMessage));
            }
        }

        private void Cancel()
        {
            _window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
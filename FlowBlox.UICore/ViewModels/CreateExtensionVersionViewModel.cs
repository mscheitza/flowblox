using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities; // MessageBoxHelper namespace hinzufügen
using MahApps.Metro.Controls; // Für MetroWindow
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class CreateExtensionVersionViewModel : INotifyPropertyChanged
    {
        private string _version;
        private string _userToken;
        private FbExtensionResult _extension;
        private MetroWindow _window;
        private Lazy<FlowBloxWebApiService> _flowBloxWebApiService = new Lazy<FlowBloxWebApiService>(() =>
        {
            var webApiServiceUrl = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ExtensionApiServiceBaseUrl"].Value;
            return new FlowBloxWebApiService(webApiServiceUrl);
        });

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
                OnPropertyChanged(nameof(CanCreate));
            }
        }

        public bool CanCreate => !string.IsNullOrEmpty(Version) && Regex.IsMatch(Version, @"^\d+\.\d+\.\d+$");

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public CreateExtensionVersionViewModel()
        {
            CreateCommand = new RelayCommand(async () => await CreateAsync());
            CancelCommand = new RelayCommand(Cancel);
        }

        public CreateExtensionVersionViewModel(MetroWindow window, string userToken, FbExtensionResult extension) : this()
        {
            this._userToken = userToken;
            this._extension = extension;
            this._window = window; // Speichern des Fensters für spätere Verwendung
        }

        private async Task CreateAsync()
        {
            var response = await _flowBloxWebApiService.Value.CreateExtensionVersionAsync(_userToken, new FbCreateExtensionVersionRequest
            {
                Version = _version,
                ExtensionGuid = _extension.Guid,
            });

            if (response.Success)
            {
                _window.DialogResult = true;
                _window.Close();
            }
            else
            {
                await MessageBoxHelper.ShowMessageBoxAsync(_window, MessageBoxType.Error, response.ErrorMessage);
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
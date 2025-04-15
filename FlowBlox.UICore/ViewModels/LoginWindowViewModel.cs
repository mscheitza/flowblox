using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class LoginWindowViewModel : INotifyPropertyChanged
    {
        private string _email;
        private string _password;
        private bool _saveUserCredentials;
        private readonly Window _window;

        private Lazy<FlowBloxWebApiService> _flowBloxWebApiService = new Lazy<FlowBloxWebApiService>(() =>
        {
            var webApiServiceUrl = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ExtensionApiServiceBaseUrl"].Value;
            FlowBloxWebApiService service = new FlowBloxWebApiService(webApiServiceUrl);
            return service;
        });

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public bool SaveUserCredentials
        {
            get => _saveUserCredentials;
            set
            {
                _saveUserCredentials = value;
                OnPropertyChanged();
            }
        }

        public ICommand ForgotPasswordCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public LoginWindowViewModel()
        {
            ForgotPasswordCommand = new RelayCommand(OpenPasswordResetWindow);
            LoginCommand = new RelayCommand(async () => await LoginAsync());
            CancelCommand = new RelayCommand(Cancel);
        }

        public LoginWindowViewModel(Window window) : this()
        {
            _window = window;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenPasswordResetWindow()
        {
            var passwordResetWindow = new PasswordResetWindow
            {
                Owner = _window
            };
            passwordResetWindow.ShowDialog();
        }

        private async Task LoginAsync()
        {
            try
            {
                var loginData = new FbLoginData { Username = Email, Password = Password };
                var token = await _flowBloxWebApiService.Value.LoginAsync(loginData);

                if (!string.IsNullOrEmpty(token))
                {
                    var userData = await _flowBloxWebApiService.Value.GetUserInfoAsync(token);
                    if (userData != null)
                    {
                        if (SaveUserCredentials)
                        {
                            var serializedLoginData = JsonConvert.SerializeObject(loginData);
                            FlowBloxOptions.GetOptionInstance().GetOption("Account.LoginData").Value = serializedLoginData;
                            FlowBloxOptions.GetOptionInstance().Save();
                        }
                        else
                        {
                            FlowBloxOptions.GetOptionInstance().GetOption("Account.LoginData").Value = "";
                            FlowBloxOptions.GetOptionInstance().Save();
                        }

                        _window.DialogResult = true;
                        _window.Tag = (token, userData);
                        _window.Close();
                    }
                }
                else
                {
                    await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_window, MessageBoxType.Error, "Login failed, please check your credentials.");
                }
            }
            catch (Exception ex)
            {
                await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_window, MessageBoxType.Error, $"An error occurred: {ex.Message}");
            }
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            Password = passwordBox?.Password;
        }
    }
}

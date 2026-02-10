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
    public class EditUserPropertiesViewModel : INotifyPropertyChanged
    {
        private readonly MetroWindow _window;
        private readonly FlowBloxWebApiService _webApiService;
        private readonly string _userToken;
        private readonly FbUserData _user;

        private string _firstName;
        private string _lastName;
        private string _userName;
        private string _errorText;

        public RelayCommand CloseCommand { get; }
        public RelayCommand SaveCommand { get; }

        public string FirstName
        {
            get => _firstName;
            set 
            { 
                _firstName = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(CanSave)); 
            }
        }

        public string LastName
        {
            get => _lastName;
            set 
            { 
                _lastName = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(CanSave)); 
            }
        }

        public string UserName
        {
            get => _userName;
            set 
            { 
                _userName = value; 
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

        public bool CanSave
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_userToken))
                    return false;

                if (string.IsNullOrWhiteSpace(UserName))
                    return false;

                return true;
            }
        }

        public EditUserPropertiesViewModel()
        {
            CloseCommand = new RelayCommand(() => _window?.Close());
            SaveCommand = new RelayCommand(async () => await SaveAsync());
        }

        public EditUserPropertiesViewModel(
            MetroWindow window,
            FlowBloxWebApiService webApiService,
            string userToken,
            FbUserData user) : this()
        {
            _window = window;
            _webApiService = webApiService;
            _userToken = userToken;
            _user = user;

            FirstName = user?.FirstName ?? string.Empty;
            LastName = user?.LastName ?? string.Empty;
            UserName = user?.UserName ?? string.Empty;
        }

        private async Task SaveAsync()
        {
            ErrorText = null;

            if (_user == null)
            {
                ErrorText = FlowBloxResourceUtil.GetLocalizedString("Error_EditUserProperties_NoUserSelected", typeof(Resources.EditUserPropertiesWindow));
                return;
            }

            var resp = await _webApiService.UpdateUserInfoAsync(
                _userToken,
                new FbUserChangeRequest
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    UserName = UserName
                });

            if (!resp.Success)
            {
                ErrorText = ApiErrorMessageHelper.BuildErrorMessage(
                    FlowBloxResourceUtil.GetLocalizedString("Error_EditUserProperties_SaveFailed", typeof(Resources.EditUserPropertiesWindow)),
                    resp.ErrorMessage);

                return;
            }

            // Apply changes locally
            var updated = resp.ResultObject;
            if (updated != null)
            {
                _user.FirstName = updated.FirstName;
                _user.LastName = updated.LastName;
                _user.UserName = updated.UserName;
            }
            else
            {
                _user.FirstName = FirstName;
                _user.LastName = LastName;
                _user.UserName = UserName;
            }

            _window.DialogResult = true;
            _window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
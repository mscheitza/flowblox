using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using FlowBlox.UICore.Models;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi;

namespace FlowBlox.UICore.ViewModels
{
    public class PasswordResetViewModel : INotifyPropertyChanged
    {
        private Window _window;
        private string _email;
        private string _captchaCode;
        private ConvertedCaptchaResponse _captchaResponse;
        private string _resetCode;
        private string _newPassword;
        private string _newPasswordRepeat;
        private string _errorText;
        private bool _canGenerateResetCode;
        private bool _canChangePassword;
        private bool _isResetCodeSent;
        private string _apiUrl;
        
        private readonly Lazy<FlowBloxWebApiService> _flowBloxWebApiService;

        public string EmailOrUsername
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(EmailOrUsername)); ValidateInput(); }
        }

        public string CaptchaCode
        {
            get => _captchaCode;
            set { _captchaCode = value; OnPropertyChanged(nameof(CaptchaCode)); ValidateInput(); }
        }

        public string ResetCode
        {
            get => _resetCode;
            set { _resetCode = value; OnPropertyChanged(nameof(ResetCode)); ValidateInput(); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(nameof(NewPassword)); ValidateInput(); }
        }

        public string NewPasswordRepeat
        {
            get => _newPasswordRepeat;
            set { _newPasswordRepeat = value; OnPropertyChanged(nameof(NewPasswordRepeat)); ValidateInput(); }
        }

        public string ErrorText
        {
            get => _errorText;
            set { _errorText = value; OnPropertyChanged(nameof(ErrorText)); }
        }

        public bool CanGenerateResetCode
        {
            get => _canGenerateResetCode;
            set { _canGenerateResetCode = value; OnPropertyChanged(nameof(CanGenerateResetCode)); }
        }

        public bool CanChangePassword
        {
            get => _canChangePassword;
            set { _canChangePassword = value; OnPropertyChanged(nameof(CanChangePassword)); }
        }

        public bool IsResetCodeSent
        {
            get => _isResetCodeSent;
            set { _isResetCodeSent = value; OnPropertyChanged(nameof(IsResetCodeSent)); }
        }

        public ConvertedCaptchaResponse CaptchaResponse
        {
            get => _captchaResponse;
            set { _captchaResponse = value; OnPropertyChanged(nameof(CaptchaResponse)); }
        }


        public ICommand GenerateResetCodeCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand CloseCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public PasswordResetViewModel()
        {
            GenerateResetCodeCommand = new RelayCommand(GenerateResetCode, () => CanGenerateResetCode);
            ChangePasswordCommand = new RelayCommand(ChangePassword, () => CanChangePassword);
            CloseCommand = new RelayCommand(() => _window?.Close());
        }

        public PasswordResetViewModel(Window window, string apiUrl) : this()
        {
            this._apiUrl = apiUrl;
            this._flowBloxWebApiService = new Lazy<FlowBloxWebApiService>(() =>
            {
                return new FlowBloxWebApiService(_apiUrl);
            });
            this._window = window;
            LoadCaptcha();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ValidateInput()
        {
            ErrorText = string.Empty;
            CanGenerateResetCode = !string.IsNullOrEmpty(EmailOrUsername) &&
                                   !string.IsNullOrEmpty(CaptchaCode);

            CanChangePassword = IsResetCodeSent &&
                                !string.IsNullOrEmpty(ResetCode) &&
                                !string.IsNullOrEmpty(NewPassword) &&
                                !string.IsNullOrEmpty(NewPasswordRepeat) &&
                                NewPassword == NewPasswordRepeat;

            ((RelayCommand)GenerateResetCodeCommand).Invalidate();
            ((RelayCommand)ChangePasswordCommand).Invalidate();
        }

        private async void LoadCaptcha()
        {
            var resp = await _flowBloxWebApiService.Value.GetCaptchaAsync();

            if (!resp.Success || resp.ResultObject == null)
            {
                await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_window, MessageBoxType.Error, 
                    ApiErrorMessageHelper.BuildErrorMessage(
                        FlowBloxResourceUtil.GetLocalizedString("Error_LoadCaptchaFailed", typeof(Resources.PasswordResetWindow)),
                        resp.ErrorMessage));

                return;
            }

            var result = resp.ResultObject;

            this.CaptchaResponse = new ConvertedCaptchaResponse
            {
                CaptchaId = result.CaptchaId,
                CaptchaImage = ConvertBase64ToBitmapImage(result.CaptchaImageBase64)
            };
        }


        private async void GenerateResetCode()
        {
            var result = await _flowBloxWebApiService.Value.GeneratePasswordResetCodeAsync(new FbPasswordResetRequest
            {
                EmailOrUsername = EmailOrUsername,
                CaptchaId = CaptchaResponse.CaptchaId,
                CaptchaCode = CaptchaCode
            });

            if (result.Success)
            {
                await MessageBoxHelper.ShowMessageBoxAsync(_window as MetroWindow, MessageBoxType.Notification, FlowBloxResourceUtil.GetLocalizedString("Message_EmailSent", typeof(Resources.PasswordResetWindow)));
                IsResetCodeSent = true;
            }
            else
            {
                ErrorText = result.ErrorMessage;
                LoadCaptcha();
            }
        }

        private async void ChangePassword()
        {
            var result = await _flowBloxWebApiService.Value.ChangePasswordAsync(new FbChangePasswordRequest
            {
                EmailOrUsername = EmailOrUsername,
                ResetCode = ResetCode,
                NewPassword = NewPassword
            });

            if (result.Success)
            {
                await MessageBoxHelper.ShowMessageBoxAsync(_window as MetroWindow, MessageBoxType.Notification, FlowBloxResourceUtil.GetLocalizedString("Message_PasswordChanged", typeof(Resources.PasswordResetWindow)));
                _window.Close();
            }
            else
            {
                ErrorText = result.ErrorMessage;
            }
        }

        public static BitmapImage ConvertBase64ToBitmapImage(string base64Data)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Data);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }
}

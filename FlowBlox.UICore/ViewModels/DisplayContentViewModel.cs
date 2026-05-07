using FlowBlox.UICore.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class DisplayContentViewModel : INotifyPropertyChanged
    {
        private readonly Window _window;
        private string _contentText = string.Empty;

        public DisplayContentViewModel(Window window, string contentText)
        {
            _window = window;
            _contentText = contentText ?? string.Empty;

            CopyCommand = new RelayCommand(_ => CopyToClipboard());
            CloseCommand = new RelayCommand(_ => Close());
        }

        public string ContentText
        {
            get => _contentText;
            set
            {
                if (_contentText == value)
                    return;

                _contentText = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public ICommand CopyCommand { get; }
        public ICommand CloseCommand { get; }

        private void CopyToClipboard()
        {
            Clipboard.SetText(ContentText ?? string.Empty);
        }

        private void Close()
        {
            _window.DialogResult = true;
            _window.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

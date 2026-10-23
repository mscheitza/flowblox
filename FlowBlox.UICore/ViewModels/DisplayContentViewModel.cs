using System.Windows;
using System.Windows.Input;
using FlowBlox.UICore.Commands;

namespace FlowBlox.UICore.ViewModels;

public sealed class DisplayContentViewModel : InnerContentViewModel
{
    private readonly Window _window;

    public DisplayContentViewModel(Window window, string contentText)
    {
        _window = window;
        ContentText = contentText;
        IsReadOnly = true;
        CloseCommand = new RelayCommand(Close);
    }

    public ICommand CloseCommand { get; }

    private void Close()
    {
        _window.DialogResult = true;
        _window.Close();
    }
}

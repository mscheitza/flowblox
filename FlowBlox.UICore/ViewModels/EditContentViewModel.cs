using System.Windows;
using System.Windows.Input;
using FlowBlox.UICore.Commands;

namespace FlowBlox.UICore.ViewModels;

public sealed class EditContentViewModel : InnerContentViewModel
{
    private readonly Window? _window;

    public EditContentViewModel()
    {
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    public EditContentViewModel(Window window) : this()
    {
        _window = window;
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private void Save()
    {
        if (IsReadOnly || _window == null)
            return;

        _window.DialogResult = true;
        _window.Close();
    }

    private void Cancel()
    {
        if (_window == null)
            return;

        _window.DialogResult = false;
        _window.Close();
    }
}

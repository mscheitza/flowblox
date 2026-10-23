using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Input;

namespace FlowBlox.UICore.Views;

public partial class AiAssistantDebugView : MetroWindow
{
    public AiAssistantDebugView(AiAssistantDebugViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Closed += (_, _) => viewModel.Dispose();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
            return;

        e.Handled = true;
        Close();
    }
}

using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views;

public partial class AiAssistantDebugView : MetroWindow
{
    public AiAssistantDebugView(AiAssistantDebugViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Closed += (_, _) => viewModel.Dispose();
    }
}
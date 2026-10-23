using System.Windows;
using System.Windows.Controls;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Factory.Adapter;
using FlowBlox.UICore.Models.FieldSelection;
using FlowBlox.UICore.ViewModels;

namespace FlowBlox.UICore.Views;

public partial class InnerContentView : UserControl
{
    public InnerContentView()
    {
        InitializeComponent();
    }

    private void InsertFieldPlaceholderButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as InnerContentViewModel;
        if (viewModel == null || 
            !viewModel.CanEdit || 
            !viewModel.EnableFieldSelection)
            return;

        var args = new FieldSelectionWindowArgs
        {
            SelectionMode = FieldSelectionMode.Options,
            IsRequired = false,
            HideRequired = true,
            AllowedFieldSelectionModes =
            [
                FieldSelectionMode.ProjectProperties,
                FieldSelectionMode.Options
            ]
        };
        Utilities.TextBoxHelper.ShowFieldSelectionDialog(
            target: null,
            args,
            new AvalonEditAdapter(Editor),
            Window.GetWindow(this));
    }
}

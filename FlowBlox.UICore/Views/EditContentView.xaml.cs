using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Factory.Adapter;
using FlowBlox.UICore.Models.FieldSelection;
using MahApps.Metro.Controls;
using System.Windows;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaction logic for EditContentView.xaml
    /// </summary>
    public partial class EditContentView : MetroWindow
    {
        private EditContentViewModel _viewModel;

        public EditContentView(string content, bool enableFieldSelection = false)
        {
            InitializeComponent();
            _viewModel = new EditContentViewModel(this);
            _viewModel.ContentText = content;
            this.DataContext = _viewModel;
            InsertFieldPlaceholderButton.Visibility = enableFieldSelection
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void InsertFieldPlaceholderButton_Click(object sender, RoutedEventArgs e)
        {
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
                this);
        }

        public string ContentText
        {
            get
            {
                return _viewModel.ContentText;
            }
        }
    }
}

using FlowBlox.Core.Models.Components;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.ViewModels.FieldSelection;
using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class FieldSelectionWindow : MetroWindow
    {
        private readonly FieldSelectionWindowArgs _args;
        private readonly FieldSelectionWindowViewModel _vm;

        public FieldSelectionWindowResult Result { get; private set; }

        public FieldSelectionWindow(FieldSelectionWindowArgs args)
        {
            InitializeComponent();

            _args = args ?? new FieldSelectionWindowArgs();
            _vm = new FieldSelectionWindowViewModel(this, _args);
            DataContext = _vm;

            // Apply selection modes based on MultiSelect.
            FieldsListView.SelectionMode = _args.MultiSelect ? SelectionMode.Extended : SelectionMode.Single;
            OptionsListView.SelectionMode = _args.MultiSelect ? SelectionMode.Extended : SelectionMode.Single;

            FieldsListView.SelectionChanged += (_, __) => RefreshOkEnabled();
            OptionsListView.SelectionChanged += (_, __) => RefreshOkEnabled();

            RefreshOkEnabled();
        }

        private void RefreshOkEnabled()
        {
            if (_vm.IsFieldsMode)
            {
                OkButton.IsEnabled = 
                    FieldsListView.SelectedItems != null && 
                    FieldsListView.SelectedItems.Count > 0;
            }
            else
            {
                OkButton.IsEnabled = 
                    OptionsListView.SelectedItems != null && 
                    OptionsListView.SelectedItems.Count > 0;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (DialogResult != true)
                return;

            var res = new FieldSelectionWindowResult
            {
                SelectionMode = _vm.SelectionMode,
                IsRequired = _vm.IsRequired
            };

            if (_vm.IsFieldsMode)
            {
                // SelectedItems contains FieldRowViewModel objects.
                var selected = FieldsListView.SelectedItems.Cast<object>()
                    .Select(x => x as FieldRowViewModel)
                    .Where(x => x != null)
                    .Select(x => x.FieldElement)
                    .Where(x => x != null)
                    .ToList();

                res.SelectedFields = selected;
                res.SelectedOptions = new List<OptionElement>();
            }
            else
            {
                // SelectedItems contains OptionRowViewModel objects.
                var selected = OptionsListView.SelectedItems.Cast<object>()
                    .Select(x => x as OptionRowViewModel)
                    .Where(x => x != null)
                    .Select(x => x.OptionElement)
                    .Where(x => x != null)
                    .ToList();

                res.SelectedOptions = selected;
                res.SelectedFields = new List<FieldElement>();
                res.IsRequired = false;
            }

            Result = res;
        }

        private void FieldsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OkButton.IsEnabled)
                _vm.OkCommand.Execute(null);
        }

        private void OptionsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OkButton.IsEnabled)
                _vm.OkCommand.Execute(null);
        }
    }
}
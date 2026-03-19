using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.ViewModels.FieldView;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class FieldViewControl : UserControl
    {
        public FieldViewControl()
        {
            InitializeComponent();
            FieldListView.SelectionChanged += FieldListView_SelectionChanged;
            FieldListView.MouseDoubleClick += FieldListView_MouseDoubleClick;
            FilterTextBox.PreviewKeyDown += FilterTextBox_PreviewKeyDown;
            DataContextChanged += FieldViewControl_DataContextChanged;
            Loaded += FieldViewControl_Loaded;
            AttachViewModelPropertyChangedHandler(DataContext, subscribe: true);
        }

        public void OnAfterUIRegistryInitialized()
        {
            if (DataContext is FieldViewModel vm)
            {
                vm.OnAfterUIRegistryInitialized();
            }
        }

        private void FilterTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {
                FilterTextBox.SelectAll();
                e.Handled = true;
            }
        }

        private void FieldListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not FieldViewModel vm)
                return;

            if (vm.OpenFieldValueCommand.CanExecute(null))
            {
                vm.OpenFieldValueCommand.Execute(null);
            }
        }

        private void FieldListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not FieldViewModel vm)
                return;

            var selectedRows = FieldListView.SelectedItems.Cast<FieldEntryViewModel>().ToList();
            vm.UpdateSelection(selectedRows);
        }

        private void FieldViewControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateFlowBlockColumnWidth();
        }

        private void FieldViewControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            AttachViewModelPropertyChangedHandler(e.OldValue, subscribe: false);
            AttachViewModelPropertyChangedHandler(e.NewValue, subscribe: true);

            UpdateFlowBlockColumnWidth();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FieldViewModel.ShowFlowBlock) ||
                e.PropertyName == nameof(FieldViewModel.FlowBlockColumnWidth))
            {
                if (Dispatcher.CheckAccess())
                    UpdateFlowBlockColumnWidth();
                else
                    Dispatcher.BeginInvoke(UpdateFlowBlockColumnWidth);
            }
        }

        private void FlowBlockToggle_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateFlowBlockColumnWidth();
        }

        private void AttachViewModelPropertyChangedHandler(object dataContext, bool subscribe)
        {
            if (dataContext is not INotifyPropertyChanged notify)
                return;

            if (subscribe)
                notify.PropertyChanged += ViewModel_PropertyChanged;
            else
                notify.PropertyChanged -= ViewModel_PropertyChanged;
        }

        private void UpdateFlowBlockColumnWidth()
        {
            if (DataContext is FieldViewModel vm && FlowBlockColumn != null)
            {
                FlowBlockColumn.Width = vm.FlowBlockColumnWidth;
            }
        }
    }
}

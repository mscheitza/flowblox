using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.ViewModels.ManagedObjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Views
{
    public partial class ManagedObjectsViewControl : UserControl
    {
        public ManagedObjectsViewControl()
        {
            InitializeComponent();
            DataContextChanged += ManagedObjectsViewControl_DataContextChanged;
            Loaded += ManagedObjectsViewControl_Loaded;
            Unloaded += ManagedObjectsViewControl_Unloaded;
        }

        public void OnAfterUIRegistryInitialized()
        {
            if (DataContext is ManagedObjectsViewModel vm)
                vm.OnAfterUIRegistryInitialized();
        }

        public void UpdateRuntimeState(bool isRuntimeActive)
        {
            if (DataContext is ManagedObjectsViewModel vm)
                vm.SetRuntimeActive(isRuntimeActive);
        }

        private void ManagedObjectsViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ManagedObjectsViewModel vm)
                vm.OnAfterUIRegistryInitialized();
        }

        private void ManagedObjectsViewControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ManagedObjectsViewModel oldVm)
                oldVm.Dispose();
        }

        private void ManagedObjectsViewControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ManagedObjectsViewModel vm)
                vm.Dispose();
        }

        private void TypeTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is not ManagedObjectsViewModel vm)
                return;

            if (e.NewValue is ManagedObjectTypeNodeViewModel node)
                vm.SelectedTypeNode = node;
        }

        private void ManagedObjectsListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListView listView)
                return;

            var current = e.OriginalSource as DependencyObject;
            while (current != null && current is not ListViewItem)
            {
                current = VisualTreeHelper.GetParent(current);
            }

            if (current is ListViewItem item)
            {
                item.IsSelected = true;
                item.Focus();
            }
        }
    }
}

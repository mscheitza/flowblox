using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.ViewModels.ManagedObjects;
using System.Windows;
using System.Windows.Controls;

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
    }
}

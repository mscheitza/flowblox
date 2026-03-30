using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.ViewModels.TestView;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Views
{
    public partial class TestViewControl : UserControl
    {
        public TestViewControl()
        {
            InitializeComponent();
            TestCasesListView.SelectionChanged += TestCasesListView_SelectionChanged;
            TestCasesListView.MouseDoubleClick += TestCasesListView_MouseDoubleClick;
            TestCasesListView.PreviewKeyDown += TestCasesListView_PreviewKeyDown;
            DataContextChanged += TestViewControl_DataContextChanged;
            Loaded += TestViewControl_Loaded;
            Unloaded += TestViewControl_Unloaded;
        }

        public void OnAfterUIRegistryInitialized()
        {
            if (DataContext is TestViewModel vm)
                vm.OnAfterUIRegistryInitialized();
        }

        public void UpdateRuntimeState(bool isRuntimeActive)
        {
            if (DataContext is TestViewModel vm)
                vm.SetRuntimeActive(isRuntimeActive);
        }

        private void TestViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TestViewModel vm)
                vm.OnAfterUIRegistryInitialized();
        }

        private void TestViewControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TestViewModel vm)
                vm.Dispose();
        }

        private void TestViewControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TestViewModel oldVm)
                oldVm.Dispose();
        }

        private void TestCasesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not TestViewModel vm)
                return;

            var selectedRows = TestCasesListView.SelectedItems.Cast<TestCaseEntryViewModel>().ToList();
            vm.UpdateSelection(selectedRows);
        }

        private void TestCasesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not TestViewModel vm)
                return;

            if (vm.EditCommand.CanExecute(null))
                vm.EditCommand.Execute(null);
        }

        private void TestCasesListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {
                TestCasesListView.SelectAll();
                e.Handled = true;
            }
        }

        private void TestCasesListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListView listView)
                return;

            var current = e.OriginalSource as DependencyObject;
            while (current != null && current is not ListViewItem)
            {
                current = VisualTreeHelper.GetParent(current);
            }

            if (current is ListViewItem item && !item.IsSelected)
            {
                listView.SelectedItems.Clear();
                item.IsSelected = true;
                item.Focus();
            }
        }
    }
}

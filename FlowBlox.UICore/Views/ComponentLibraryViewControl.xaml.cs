using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.ViewModels.ComponentLibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class ComponentLibraryViewControl : UserControl
    {
        public ComponentLibraryViewControl()
        {
            InitializeComponent();
        }

        public ComponentLibraryViewModel ViewModel => DataContext as ComponentLibraryViewModel;

        private void LibraryTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (TryFindAncestor<TreeViewItem>(e.OriginalSource as DependencyObject) is not { DataContext: ComponentLibraryNodeViewModel node })
                return;

            if (!node.IsFlowBlock || node.FlowBlock == null)
                return;

            var dataObject = new DataObject();
            dataObject.SetData(FlowBloxFlowBlockDragDropFormats.FlowBlockType, node.FlowBlock.GetType().FullName);
            DragDrop.DoDragDrop(LibraryTreeView, dataObject, DragDropEffects.Copy);
        }

        private static T TryFindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                    return match;

                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}

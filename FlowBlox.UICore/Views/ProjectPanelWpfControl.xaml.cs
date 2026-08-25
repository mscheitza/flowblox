using FlowBlox.UICore.ViewModels.ProjectPanel;
using FlowBlox.UICore.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Views
{
    public partial class ProjectPanelWpfControl : UserControl
    {
        private FlowBlockNodeViewModel _draggedNode;
        private Point _dragStart;
        private double _nodeStartX;
        private double _nodeStartY;
        private bool _isCanvasPanning;
        private Point _panStart;
        private double _panStartHorizontalOffset;
        private double _panStartVerticalOffset;
        private bool _isMarqueeSelecting;
        private Point _marqueeStart;
        private FlowBlockNodeViewModel _connectionStartNode;
        private bool _isConnectingNodes;
        private Window _hostWindow;

        public ProjectPanelWpfViewModel ViewModel { get; }

        public ProjectPanelWpfControl()
        {
            InitializeComponent();
            ViewModel = new ProjectPanelWpfViewModel();
            DataContext = ViewModel;
            Loaded += ProjectPanelWpfControl_Loaded;
            Unloaded += ProjectPanelWpfControl_Unloaded;
            LostKeyboardFocus += ProjectPanelWpfControl_LostKeyboardFocus;
        }

        public void RefreshProject() => ViewModel.Refresh();

        public void UpdateRuntimeState(bool isRuntimeActive, bool isRuntimePaused)
            => ViewModel.UpdateRuntimeState(isRuntimeActive, isRuntimePaused);

        public void MarkRuntimeFocus(FlowBlox.Core.Models.FlowBlocks.Base.BaseFlowBlock flowBlock)
            => ViewModel.MarkRuntimeFocus(flowBlock);

        private void ProjectPanelWpfControl_Loaded(object sender, RoutedEventArgs e)
        {
            _hostWindow = Window.GetWindow(this);
            if (_hostWindow == null)
                return;

            _hostWindow.PreviewKeyDown -= HostWindow_PreviewKeyChanged;
            _hostWindow.PreviewKeyUp -= HostWindow_PreviewKeyChanged;
            _hostWindow.PreviewKeyDown += HostWindow_PreviewKeyChanged;
            _hostWindow.PreviewKeyUp += HostWindow_PreviewKeyChanged;
        }

        private void ProjectPanelWpfControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_hostWindow != null)
            {
                _hostWindow.PreviewKeyDown -= HostWindow_PreviewKeyChanged;
                _hostWindow.PreviewKeyUp -= HostWindow_PreviewKeyChanged;
                _hostWindow = null;
            }

            ViewModel.SetTemporaryConnectionMode(false);
        }

        private void ProjectPanelWpfControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
                ViewModel.SetTemporaryConnectionMode(false);
        }

        private void HostWindow_PreviewKeyChanged(object sender, KeyEventArgs e)
            => UpdateTemporaryConnectionMode();

        private void UpdateTemporaryConnectionMode()
        {
            var isShortcutActive =
                (IsKeyboardFocusWithin || IsMouseOver) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                ViewModel.ConnectionModeCommand.CanExecute(null);

            ViewModel.SetTemporaryConnectionMode(isShortcutActive);
        }

        private void Arrow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FlowBlockArrowViewModel arrow)
                return;

            ViewModel.SelectArrow(arrow);
            e.Handled = true;
        }

        private void Arrow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FlowBlockArrowViewModel arrow)
                return;

            ViewModel.SelectArrow(arrow);
        }

        private void FlowBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FlowBlockNodeViewModel node)
                return;

            if (IsScrollBarInteraction(e.OriginalSource))
                return;

            if (IsFlowBlockRowInteraction(e.OriginalSource))
                return;

            if (IsSelectableTextInteraction(e.OriginalSource))
                return;

            var isHeaderInteraction = IsFlowBlockHeaderInteraction(e.OriginalSource);

            if (ViewModel.IsConnectionMode)
            {
                if (!ViewModel.CanStartConnectionFrom(node))
                    return;

                _connectionStartNode = node;
                _isConnectingNodes = true;
                var current = e.GetPosition(ProjectCanvas);
                UpdateConnectionPreview(node, current);
                Mouse.Capture(ProjectCanvas);
                e.Handled = true;
                return;
            }

            if (e.ClickCount > 1)
            {
                ViewModel.SelectNode(node, toggle: false, extend: false);
                if (ViewModel.EditSelectionCommand.CanExecute(null))
                    ViewModel.EditSelectionCommand.Execute(null);

                e.Handled = true;
                return;
            }

            var toggle = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            var extend = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            ViewModel.SelectNode(node, toggle, extend);

            if (!isHeaderInteraction)
            {
                e.Handled = true;
                return;
            }

            _draggedNode = node;
            _dragStart = e.GetPosition(this);
            _nodeStartX = node.X;
            _nodeStartY = node.Y;
            Mouse.Capture(sender as IInputElement);
            e.Handled = true;
        }

        private void FlowBlockRow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FlowBlockRenderRowViewModel row)
                return;

            ViewModel.SelectRow(row);
            if (e.ClickCount > 1 && row.CanNavigate)
                ViewModel.OpenRowTarget(row);

            e.Handled = true;
        }

        private void FlowBlockRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FlowBlockRenderRowViewModel row)
                return;

            ViewModel.SelectRow(row);
        }

        private void FlowBlock_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FlowBlockNodeViewModel node)
                return;

            if (!node.IsSelected)
                ViewModel.SelectNode(node, toggle: false, extend: false);
        }

        private void FlowBlock_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isConnectingNodes)
            {
                UpdateConnectionPreview(_connectionStartNode, e.GetPosition(ProjectCanvas));
                e.Handled = true;
                return;
            }

            if (_draggedNode == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            var current = e.GetPosition(this);
            var delta = current - _dragStart;
            var snappedPosition = ViewModel.GetSnappedNodePosition(_draggedNode, _nodeStartX + delta.X, _nodeStartY + delta.Y);
            _draggedNode.X = snappedPosition.X;
            _draggedNode.Y = snappedPosition.Y;
            ViewModel.EnsureCanvasContainsNode(
                _draggedNode,
                CanvasScrollViewer.ViewportWidth / 2d,
                CanvasScrollViewer.ViewportHeight / 2d);
            e.Handled = true;
        }

        private void FlowBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isConnectingNodes)
            {
                FinishConnection(e.GetPosition(ProjectCanvas));
                e.Handled = true;
                return;
            }

            if (_draggedNode == null)
                return;

            var from = new System.Drawing.Point((int)Math.Round(_nodeStartX), (int)Math.Round(_nodeStartY));
            var to = new System.Drawing.Point((int)Math.Round(_draggedNode.X), (int)Math.Round(_draggedNode.Y));
            ViewModel.CommitNodeMove(_draggedNode, from, to);

            Mouse.Capture(null);
            _draggedNode = null;
            e.Handled = true;
        }

        private void CanvasScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle)
                return;

            _isCanvasPanning = true;
            _panStart = e.GetPosition(CanvasScrollViewer);
            _panStartHorizontalOffset = CanvasScrollViewer.HorizontalOffset;
            _panStartVerticalOffset = CanvasScrollViewer.VerticalOffset;
            CanvasScrollViewer.Cursor = Cursors.SizeAll;
            Mouse.Capture(CanvasScrollViewer);
            e.Handled = true;
        }

        private void CanvasScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isCanvasPanning)
                return;

            var current = e.GetPosition(CanvasScrollViewer);
            var delta = current - _panStart;
            CanvasScrollViewer.ScrollToHorizontalOffset(_panStartHorizontalOffset - delta.X);
            CanvasScrollViewer.ScrollToVerticalOffset(_panStartVerticalOffset - delta.Y);
            e.Handled = true;
        }

        private void CanvasScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
                EndCanvasPan();
        }

        private void CanvasScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isCanvasPanning && e.MiddleButton != MouseButtonState.Pressed)
                EndCanvasPan();
        }

        private void ProjectCanvas_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = ViewModel.CanCreateFlowBlockFromDrop(e.Data)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void ProjectCanvas_Drop(object sender, DragEventArgs e)
        {
            if (!ViewModel.CanCreateFlowBlockFromDrop(e.Data))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            ViewModel.CreateFlowBlockFromDrop(
                e.Data,
                e.GetPosition(ProjectCanvas),
                CanvasScrollViewer.ViewportWidth / 2d,
                CanvasScrollViewer.ViewportHeight / 2d);

            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void ProjectCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!ViewModel.CanStartMarqueeSelection)
                return;

            _isMarqueeSelecting = true;
            _marqueeStart = e.GetPosition(ProjectCanvas);
            UpdateMarqueeSelectionRectangle(_marqueeStart);

            ProjectCanvas.CaptureMouse();
            e.Handled = true;
        }

        private void ProjectCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isConnectingNodes)
            {
                UpdateConnectionPreview(_connectionStartNode, e.GetPosition(ProjectCanvas));
                e.Handled = true;
                return;
            }

            if (!_isMarqueeSelecting || e.LeftButton != MouseButtonState.Pressed)
                return;

            UpdateMarqueeSelectionRectangle(e.GetPosition(ProjectCanvas));
            e.Handled = true;
        }

        private void ProjectCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isConnectingNodes)
            {
                FinishConnection(e.GetPosition(ProjectCanvas));
                e.Handled = true;
                return;
            }

            if (!_isMarqueeSelecting)
                return;

            var selectionBounds = GetMarqueeSelectionBounds(e.GetPosition(ProjectCanvas));
            EndMarqueeSelection();
            ViewModel.SelectNodesInRectangle(selectionBounds);
            e.Handled = true;
        }

        private void EndCanvasPan()
        {
            _isCanvasPanning = false;
            CanvasScrollViewer.Cursor = Cursors.Arrow;
            Mouse.Capture(null);
        }

        private void EndMarqueeSelection()
        {
            _isMarqueeSelecting = false;
            MarqueeSelectionRectangle.Visibility = Visibility.Collapsed;
            ProjectCanvas.ReleaseMouseCapture();
        }

        private void FinishConnection(Point endPoint)
        {
            var endNode = ViewModel.GetNodeAt(endPoint);
            ViewModel.ConnectNodes(_connectionStartNode, endNode);
            EndConnectionPreview();
        }

        private void EndConnectionPreview()
        {
            _isConnectingNodes = false;
            _connectionStartNode = null;
            ConnectionPreviewLine.Visibility = Visibility.Collapsed;
            ConnectionPreviewStartDot.Visibility = Visibility.Collapsed;
            ConnectionPreviewArrowHead.Visibility = Visibility.Collapsed;
            Mouse.Capture(null);
        }

        private void UpdateConnectionPreview(FlowBlockNodeViewModel startNode, Point rawEnd)
        {
            if (startNode == null)
                return;

            var startCenter = GetNodeCenter(startNode);
            var endNode = ViewModel.GetNodeAt(rawEnd);
            var end = endNode != null && ViewModel.CanPreviewConnection(startNode, endNode)
                ? ArrowGeometryHelper.GetEdgePoint(GetNodeBounds(endNode), GetNodeCenter(endNode), startCenter)
                : rawEnd;
            var start = ArrowGeometryHelper.GetEdgePoint(GetNodeBounds(startNode), startCenter, end);

            ConnectionPreviewLine.X1 = start.X;
            ConnectionPreviewLine.Y1 = start.Y;
            ConnectionPreviewLine.X2 = end.X;
            ConnectionPreviewLine.Y2 = end.Y;
            ConnectionPreviewLine.Visibility = Visibility.Visible;

            Canvas.SetLeft(ConnectionPreviewStartDot, start.X - (ConnectionPreviewStartDot.Width / 2d));
            Canvas.SetTop(ConnectionPreviewStartDot, start.Y - (ConnectionPreviewStartDot.Height / 2d));
            ConnectionPreviewStartDot.Visibility = Visibility.Visible;

            ConnectionPreviewArrowHead.Points = ArrowGeometryHelper.CreateArrowHeadPoints(start, end);
            ConnectionPreviewArrowHead.Visibility = Visibility.Visible;
        }

        private static Point GetNodeCenter(FlowBlockNodeViewModel node)
            => node == null
                ? new Point()
                : new Point(node.X + (node.Width / 2d), node.Y + (node.Height / 2d));

        private static Rect GetNodeBounds(FlowBlockNodeViewModel node)
            => new(node.X, node.Y, node.Width, node.Height);

        private void UpdateMarqueeSelectionRectangle(Point current)
        {
            var bounds = GetMarqueeSelectionBounds(current);
            Canvas.SetLeft(MarqueeSelectionRectangle, bounds.Left);
            Canvas.SetTop(MarqueeSelectionRectangle, bounds.Top);
            MarqueeSelectionRectangle.Width = bounds.Width;
            MarqueeSelectionRectangle.Height = bounds.Height;
            MarqueeSelectionRectangle.Visibility = Visibility.Visible;
        }

        private Rect GetMarqueeSelectionBounds(Point current)
            => new(
                Math.Min(_marqueeStart.X, current.X),
                Math.Min(_marqueeStart.Y, current.Y),
                Math.Abs(current.X - _marqueeStart.X),
                Math.Abs(current.Y - _marqueeStart.Y));

        private static bool IsScrollBarInteraction(object originalSource)
        {
            if (originalSource is not DependencyObject current)
                return false;

            while (current != null)
            {
                if (current is ScrollBar or Thumb or RepeatButton)
                    return true;

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private static bool IsFlowBlockRowInteraction(object originalSource)
        {
            if (originalSource is not DependencyObject current)
                return false;

            while (current != null)
            {
                if (current is FrameworkElement { DataContext: FlowBlockRenderRowViewModel })
                    return true;

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private static bool IsFlowBlockHeaderInteraction(object originalSource)
        {
            if (originalSource is not DependencyObject current)
                return false;

            while (current != null)
            {
                if (current is FrameworkElement { Tag: "FlowBlockHeader" })
                    return true;

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private static bool IsSelectableTextInteraction(object originalSource)
        {
            if (originalSource is not DependencyObject current)
                return false;

            while (current != null)
            {
                if (current is TextBox)
                    return true;

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
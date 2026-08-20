using FlowBlox.Core.Actions;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Events;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.ControlFlow;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Events;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FlowBlox.UICore.ViewModels.ProjectPanel
{
    public sealed class ProjectPanelWpfViewModel : INotifyPropertyChanged, IDisposable
    {
        private const double DefaultCanvasWidth = 3000d;
        private const double DefaultCanvasHeight = 1400d;
        private const double CenterSnapTolerance = 10d;
        private readonly Dictionary<BaseFlowBlock, FlowBlockNodeViewModel> _nodesByFlowBlock = new();
        private readonly IFlowBloxProjectComponentProvider _componentProvider;
        private readonly IDialogService _dialogService;
        private readonly IRuntimeStateService _runtimeStateService;
        private readonly SynchronizationContext _uiContext;
        private FlowBloxRegistry _registry;
        private FlowBloxProject _project;
        private FlowBlockNodeViewModel _selectedNode;
        private FlowBlockArrowViewModel _selectedArrow;
        private bool _isConnectionMode;
        private bool _isRuntimeActive;
        private bool _isRuntimePaused;

        public ProjectPanelWpfViewModel()
        {
            _uiContext = SynchronizationContext.Current;
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<IFlowBloxProjectComponentProvider>();
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            _runtimeStateService = FlowBloxServiceLocator.Instance.GetService<IRuntimeStateService>();

            SelectModeCommand = new RelayCommand(() => IsConnectionMode = false, CanEditGrid);
            ConnectionModeCommand = new RelayCommand(() => IsConnectionMode = true, CanEditGrid);
            SelectAllCommand = new RelayCommand(SelectAll, CanEditGrid);
            SelectLeftCommand = new RelayCommand(() => SelectDirectional(Direction.Left), CanEditGrid);
            SelectRightCommand = new RelayCommand(() => SelectDirectional(Direction.Right), CanEditGrid);
            SelectUpCommand = new RelayCommand(() => SelectDirectional(Direction.Up), CanEditGrid);
            SelectDownCommand = new RelayCommand(() => SelectDirectional(Direction.Down), CanEditGrid);
            AutoLayoutCommand = new RelayCommand(AutoLayout, () => HasProject() && !IsRuntimeActive);
            DeleteSelectionCommand = new RelayCommand(DeleteSelection, () => HasProject() && SelectedArrow == null && SelectedNodes.Any() && !IsRuntimeActive);
            EditSelectionCommand = new RelayCommand(EditSelection, () => SelectedNode != null);
            RefreshCommand = new RelayCommand(Refresh);
            ExecuteRuntimeCommand = new RelayCommand(() => ExecuteRuntimeRequested?.Invoke(this, EventArgs.Empty), () => CanExecuteRuntime);
            PauseRuntimeCommand = new RelayCommand(() => PauseRuntimeRequested?.Invoke(this, EventArgs.Empty), () => CanPauseRuntime);
            StopRuntimeCommand = new RelayCommand(() => StopRuntimeRequested?.Invoke(this, EventArgs.Empty), () => CanStopRuntime);
            GridSettingsCommand = new RelayCommand(ShowGridSettings, () => HasProject() && (!IsRuntimeActive || IsRuntimePaused));
            ToggleBreakpointCommand = new RelayCommand(ToggleBreakpoint, () => SelectedNode != null);
            DefineExecutionIndexCommand = new RelayCommand(DefineExecutionIndex, () => CanManageExecutionIndex);
            RemoveExecutionIndexCommand = new RelayCommand(RemoveExecutionIndex, () => CanManageExecutionIndex);
            ShowInputInsightCommand = new RelayCommand(ShowInputInsight, () => CanShowInputInsight);
            ShowOutputInsightCommand = new RelayCommand(ShowOutputInsight, () => CanShowOutputInsight);
            ManageNotificationsCommand = new RelayCommand(ManageNotifications, () => CanManageNotifications);
            CopyRowValueCommand = new RelayCommand(
                parameter => CopyRowValue(parameter as FlowBlockRenderRowViewModel),
                parameter => parameter is FlowBlockRenderRowViewModel row && row.CanCopyValue);
            RemoveConnectionCommand = new RelayCommand(
                parameter => RemoveConnection(parameter as FlowBlockArrowViewModel),
                parameter => parameter is FlowBlockArrowViewModel arrow && arrow.CanRemove && HasProject() && !IsRuntimeActive);

            FlowBloxProjectManager.Instance.ProjectChanged += ProjectManager_ProjectChanged;
            if (_runtimeStateService != null)
            {
                _runtimeStateService.StateChanged += RuntimeStateService_StateChanged;
                UpdateRuntimeState(_runtimeStateService.IsRuntimeActive, _runtimeStateService.IsRuntimePaused);
            }

            Rebind(FlowBloxProjectManager.Instance.ActiveProject);
        }

        public ObservableCollection<FlowBlockNodeViewModel> Nodes { get; } = new();
        public ObservableCollection<FlowBlockArrowViewModel> Arrows { get; } = new();

        public RelayCommand SelectModeCommand { get; }
        public RelayCommand ConnectionModeCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand SelectLeftCommand { get; }
        public RelayCommand SelectRightCommand { get; }
        public RelayCommand SelectUpCommand { get; }
        public RelayCommand SelectDownCommand { get; }
        public RelayCommand AutoLayoutCommand { get; }
        public RelayCommand DeleteSelectionCommand { get; }
        public RelayCommand EditSelectionCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand ExecuteRuntimeCommand { get; }
        public RelayCommand PauseRuntimeCommand { get; }
        public RelayCommand StopRuntimeCommand { get; }
        public RelayCommand GridSettingsCommand { get; }
        public RelayCommand ToggleBreakpointCommand { get; }
        public RelayCommand DefineExecutionIndexCommand { get; }
        public RelayCommand RemoveExecutionIndexCommand { get; }
        public RelayCommand ShowInputInsightCommand { get; }
        public RelayCommand ShowOutputInsightCommand { get; }
        public RelayCommand ManageNotificationsCommand { get; }
        public RelayCommand CopyRowValueCommand { get; }
        public RelayCommand RemoveConnectionCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ExecuteRuntimeRequested;
        public event EventHandler PauseRuntimeRequested;
        public event EventHandler StopRuntimeRequested;

        public FlowBlockNodeViewModel SelectedNode
        {
            get => _selectedNode;
            private set
            {
                if (ReferenceEquals(_selectedNode, value))
                    return;

                _selectedNode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanManageExecutionIndex));
                OnPropertyChanged(nameof(CanManageNotifications));
                OnPropertyChanged(nameof(CanShowInputInsight));
                OnPropertyChanged(nameof(CanShowOutputInsight));
                OnPropertyChanged(nameof(EditSelectionHeader));
                OnPropertyChanged(nameof(ToggleBreakpointHeader));
                EditSelectionCommand.Invalidate();
                InvalidateSelectionCommands();
            }
        }

        public FlowBlockArrowViewModel SelectedArrow
        {
            get => _selectedArrow;
            private set
            {
                if (ReferenceEquals(_selectedArrow, value))
                    return;

                if (_selectedArrow != null)
                    _selectedArrow.IsSelected = false;

                _selectedArrow = value;

                if (_selectedArrow != null)
                    _selectedArrow.IsSelected = true;

                OnPropertyChanged();
                InvalidateSelectionCommands();
            }
        }

        public bool IsConnectionMode
        {
            get => _isConnectionMode;
            set
            {
                if (_isConnectionMode == value)
                    return;

                _isConnectionMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSelectionMode));
                OnPropertyChanged(nameof(ModeText));
                InvalidateSelectionCommands();
            }
        }

        public bool IsRuntimeActive
        {
            get => _isRuntimeActive;
            private set
            {
                if (_isRuntimeActive == value)
                    return;

                _isRuntimeActive = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditSelectionHeader));
                OnPropertyChanged(nameof(CanManageExecutionIndex));
                InvalidateAllCommands();
            }
        }

        public bool IsRuntimePaused
        {
            get => _isRuntimePaused;
            private set
            {
                if (_isRuntimePaused == value)
                    return;

                _isRuntimePaused = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanExecuteRuntime));
                OnPropertyChanged(nameof(CanPauseRuntime));
                OnPropertyChanged(nameof(CanStopRuntime));
                InvalidateAllCommands();
            }
        }

        public IEnumerable<FlowBlockNodeViewModel> SelectedNodes => Nodes.Where(x => x.IsSelected);
        public bool HasNodes => Nodes.Count > 0;
        public bool IsSelectionMode => !IsConnectionMode;
        public string ModeText => IsConnectionMode
            ? FlowBloxResourceUtil.GetLocalizedString("Toolbar_Connect", typeof(Resources.ProjectPanel))
            : FlowBloxResourceUtil.GetLocalizedString("Toolbar_Select", typeof(Resources.ProjectPanel));
        public string ToggleBreakpointHeader => SelectedNode?.InternalFlowBlock.BreakPoint == true
            ? FlowBloxResourceUtil.GetLocalizedString("ContextMenu_RemoveBreakpoint", typeof(Resources.ProjectPanel))
            : FlowBloxResourceUtil.GetLocalizedString("ContextMenu_SetBreakpoint", typeof(Resources.ProjectPanel));
        public double CanvasWidth => Math.Max(DefaultCanvasWidth, _project?.GridSizeX ?? DefaultCanvasWidth);
        public double CanvasHeight => Math.Max(DefaultCanvasHeight, _project?.GridSizeY ?? DefaultCanvasHeight);
        public string EditSelectionHeader => IsRuntimeActive
            ? FlowBloxResourceUtil.GetLocalizedString("ContextMenu_ViewProperties", typeof(Resources.ProjectPanel))
            : FlowBloxResourceUtil.GetLocalizedString("ContextMenu_EditProperties", typeof(Resources.ProjectPanel));
        public bool CanManageExecutionIndex => SelectedNode != null && SelectedNode.InternalFlowBlock is not NoteFlowBlock && !IsRuntimeActive;
        public bool CanManageNotifications => SelectedNode?.InternalFlowBlock.NotificationTypes?.Any() == true;
        public bool CanShowInputInsight => SelectedNode?.InternalFlowBlock.InputDataset_CurrentlyProcessing != null;
        public bool CanShowOutputInsight => SelectedNode?.InternalFlowBlock is BaseResultFlowBlock resultFlowBlock &&
                                            resultFlowBlock.OutputDataset_CurrentlyProcessing != null;
        public bool CanStartMarqueeSelection => CanEditGrid() && !IsConnectionMode;
        public bool CanExecuteRuntime => HasProject() && (!IsRuntimeActive || IsRuntimePaused);
        public bool CanPauseRuntime => HasProject() && IsRuntimeActive && !IsRuntimePaused;
        public bool CanStopRuntime => HasProject() && IsRuntimeActive;

        public void SelectNode(FlowBlockNodeViewModel node, bool toggle, bool extend)
        {
            if (node == null)
                return;

            SelectedArrow = null;

            if (!toggle && !extend)
            {
                foreach (var item in Nodes)
                    item.IsSelected = false;
            }

            node.IsSelected = toggle ? !node.IsSelected : true;
            SelectedNode = node.IsSelected ? node : SelectedNodes.LastOrDefault();
            MarkReferences();
            InvalidateSelectionCommands();
        }

        public void SelectArrow(FlowBlockArrowViewModel arrow)
        {
            if (arrow == null)
                return;

            foreach (var node in Nodes)
                node.IsSelected = false;

            SelectedNode = null;
            SelectedArrow = arrow;
            MarkReferences();
            InvalidateSelectionCommands();
        }

        public void SelectNodesInRectangle(Rect selectionBounds)
        {
            SelectedArrow = null;

            FlowBlockNodeViewModel lastSelected = null;
            foreach (var node in Nodes)
            {
                var isInside = node.X > selectionBounds.Left &&
                               node.X < selectionBounds.Right &&
                               node.Y > selectionBounds.Top &&
                               node.Y < selectionBounds.Bottom;

                node.IsSelected = isInside;
                if (isInside)
                    lastSelected = node;
            }

            SelectedNode = lastSelected;
            MarkReferences();
            InvalidateSelectionCommands();
        }

        public FlowBlockNodeViewModel GetNodeAt(Point location)
            => Nodes.LastOrDefault(node =>
                location.X >= node.X &&
                location.X <= node.X + node.Width &&
                location.Y >= node.Y &&
                location.Y <= node.Y + node.Height);

        public bool ConnectNodes(FlowBlockNodeViewModel startNode, FlowBlockNodeViewModel endNode)
        {
            if (!CanConnect(startNode, endNode))
                return false;

            FlowBloxBaseAction connectAction;
            if (startNode.InternalFlowBlock is RecursiveCallFlowBlock recursiveCallFlowBlock)
            {
                connectAction = new FlowBloxInvokeAction
                {
                    From = recursiveCallFlowBlock,
                    To = endNode.InternalFlowBlock
                };
            }
            else
            {
                connectAction = new FlowBloxConnectAction
                {
                    From = startNode.InternalFlowBlock,
                    To = endNode.InternalFlowBlock
                };
            }

            connectAction.Invoke();
            _componentProvider?.GetCurrentChangelist()?.AddChange(connectAction);
            RefreshArrows();
            return true;
        }

        public Point GetSnappedNodePosition(FlowBlockNodeViewModel movedNode, double proposedX, double proposedY)
        {
            if (movedNode == null || SelectedNodes.Count() > 1)
                return new Point(proposedX, proposedY);

            var movedCenterX = proposedX + movedNode.Width / 2d;
            var movedCenterY = proposedY + movedNode.Height / 2d;
            var otherNodes = Nodes.Where(node => !ReferenceEquals(node, movedNode)).ToList();

            var horizontalCandidates = otherNodes
                .Where(node => node.Y < movedCenterY && node.Y + node.Height > movedCenterY)
                .ToList();
            var verticalCandidates = otherNodes
                .Where(node => node.X < movedCenterX && node.X + node.Width > movedCenterX)
                .ToList();

            var neighborNodes = new List<FlowBlockNodeViewModel>();
            var left = horizontalCandidates.Where(node => node.X < proposedX).OrderBy(node => node.X).LastOrDefault();
            var right = horizontalCandidates.Where(node => node.X > proposedX).OrderBy(node => node.X).FirstOrDefault();
            var up = verticalCandidates.Where(node => node.Y < proposedY).OrderBy(node => node.Y).LastOrDefault();
            var down = verticalCandidates.Where(node => node.Y > proposedY).OrderBy(node => node.Y).FirstOrDefault();

            if (left != null)
                neighborNodes.Add(left);
            if (right != null)
                neighborNodes.Add(right);
            if (up != null)
                neighborNodes.Add(up);
            if (down != null)
                neighborNodes.Add(down);

            var snappedX = proposedX;
            var snappedY = proposedY;
            var snappedHorizontal = false;
            var snappedVertical = false;

            foreach (var neighbor in neighborNodes)
            {
                var neighborCenterX = neighbor.X + neighbor.Width / 2d;
                var neighborCenterY = neighbor.Y + neighbor.Height / 2d;

                if (!snappedHorizontal && Math.Abs(movedCenterX - neighborCenterX) < CenterSnapTolerance)
                {
                    snappedHorizontal = true;
                    snappedX = neighborCenterX - movedNode.Width / 2d;
                }

                if (!snappedVertical && Math.Abs(movedCenterY - neighborCenterY) < CenterSnapTolerance)
                {
                    snappedVertical = true;
                    snappedY = neighborCenterY - movedNode.Height / 2d;
                }
            }

            return new Point(Math.Max(0d, snappedX), Math.Max(0d, snappedY));
        }

        public void NotifyNodeMoved()
        {
            RefreshArrowGeometry();
        }

        public void MarkRuntimeFocus(BaseFlowBlock flowBlock)
        {
            foreach (var node in Nodes)
                node.IsRuntimeFocused = flowBlock != null && ReferenceEquals(node.InternalFlowBlock, flowBlock);
        }

        public void CommitNodeMove(FlowBlockNodeViewModel node, System.Drawing.Point from, System.Drawing.Point to)
        {
            if (node == null || from == to)
                return;

            var moveAction = new FlowBloxMoveAction
            {
                FlowBlock = node.InternalFlowBlock,
                From = from,
                To = to
            };

            _componentProvider?.GetCurrentChangelist()?.AddChange(moveAction);
        }

        public void Refresh()
        {
            foreach (var node in Nodes)
                node.RefreshRows();

            RefreshArrows();
        }

        public bool CanCreateFlowBlockFromDrop(IDataObject dataObject)
            => ResolveDraggedFlowBlockType(dataObject) != null && _registry != null && !IsRuntimeActive;

        public void CreateFlowBlockFromDrop(IDataObject dataObject, Point location)
        {
            var flowBlockType = ResolveDraggedFlowBlockType(dataObject);
            if (flowBlockType == null || _registry == null)
                return;

            var createdFlowBlock = _registry.CreateFlowBlockUnregistered(flowBlockType);
            createdFlowBlock.Location = new System.Drawing.Point(
                Math.Max(0, (int)Math.Round(location.X)),
                Math.Max(0, (int)Math.Round(location.Y)));

            if (!AssignFlowBlockName(createdFlowBlock))
                return;

            _registry.PostProcessFlowBlockCreated(createdFlowBlock);

            var createAction = new FlowBloxCreateAction
            {
                FlowBlock = createdFlowBlock
            };
            createAction.Invoke();
            _componentProvider?.GetCurrentChangelist()?.AddChange(createAction);

            if (_nodesByFlowBlock.TryGetValue(createdFlowBlock, out var node))
                SelectNode(node, toggle: false, extend: false);

            RefreshArrows();
        }

        private void ProjectManager_ProjectChanged(object sender, ProjectChangedEventArgs e)
            => SynchronizationContextHelper.PostToUi(_uiContext, () => Rebind(e.NewProject));

        private void Rebind(FlowBloxProject project)
        {
            UnsubscribeRegistry();
            ClearNodes();

            _project = project;
            _registry = project?.FlowBloxRegistry;
            if (_registry != null)
            {
                _registry.OnFlowBlockAdded += Registry_OnFlowBlockAdded;
                _registry.OnFlowBlockRemoved += Registry_OnFlowBlockRemoved;

                foreach (var flowBlock in _registry.GetFlowBlocks())
                    AddNode(flowBlock);
            }

            RefreshArrows();
            OnPropertyChanged(nameof(HasNodes));
            OnPropertyChanged(nameof(CanvasWidth));
            OnPropertyChanged(nameof(CanvasHeight));
            AutoLayoutCommand.Invalidate();
            GridSettingsCommand.Invalidate();
        }

        private void Registry_OnFlowBlockAdded(FlowBlockAddedEventArgs eventArgs)
        {
            SynchronizationContextHelper.PostToUi(_uiContext, () =>
            {
                AddNode(eventArgs.AddedFlowBlock);
                RefreshArrows();
            });
        }

        private void Registry_OnFlowBlockRemoved(FlowBlockRemovedEventArgs eventArgs)
        {
            SynchronizationContextHelper.PostToUi(_uiContext, () =>
            {
                RemoveNode(eventArgs.RemovedFlowBlock);
                RefreshArrows();
            });
        }

        private void AddNode(BaseFlowBlock flowBlock)
        {
            if (flowBlock == null || _nodesByFlowBlock.ContainsKey(flowBlock))
                return;

            var node = new FlowBlockNodeViewModel(flowBlock);
            node.PropertyChanged += Node_PropertyChanged;
            _nodesByFlowBlock[flowBlock] = node;
            Nodes.Add(node);
            OnPropertyChanged(nameof(HasNodes));
        }

        private void RemoveNode(BaseFlowBlock flowBlock)
        {
            if (flowBlock == null || !_nodesByFlowBlock.TryGetValue(flowBlock, out var node))
                return;

            node.PropertyChanged -= Node_PropertyChanged;
            node.Dispose();
            _nodesByFlowBlock.Remove(flowBlock);
            Nodes.Remove(node);
            if (ReferenceEquals(SelectedNode, node))
                SelectedNode = SelectedNodes.LastOrDefault();
            OnPropertyChanged(nameof(HasNodes));
        }

        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FlowBlockNodeViewModel.X) ||
                e.PropertyName == nameof(FlowBlockNodeViewModel.Y) ||
                e.PropertyName == nameof(FlowBlockNodeViewModel.Height))
            {
                RefreshArrowGeometry(sender as FlowBlockNodeViewModel);
            }
            else if (e.PropertyName == nameof(FlowBlockNodeViewModel.HasBreakpoint) &&
                     ReferenceEquals(sender, SelectedNode))
            {
                OnPropertyChanged(nameof(ToggleBreakpointHeader));
            }
        }

        private void RefreshArrowGeometry(FlowBlockNodeViewModel node = null)
        {
            foreach (var arrow in Arrows)
            {
                if (node == null ||
                    ReferenceEquals(arrow.From, node) ||
                    ReferenceEquals(arrow.To, node))
                {
                    arrow.NotifyGeometryChanged();
                }
            }
        }

        private void RefreshArrows()
        {
            var selectedArrow = SelectedArrow;
            Arrows.Clear();
            foreach (var node in Nodes)
            {
                foreach (var reference in node.InternalFlowBlock.ReferencedFlowBlocks)
                {
                    if (_nodesByFlowBlock.TryGetValue(reference, out var from))
                        AddArrow(new FlowBlockArrowViewModel(from, node, "invoke"), selectedArrow);
                }

                if (node.InternalFlowBlock.HasInputReference &&
                    node.InternalFlowBlock.IterationContext != null &&
                    _nodesByFlowBlock.TryGetValue(node.InternalFlowBlock.IterationContext, out var iterationSource))
                {
                    AddArrow(new FlowBlockArrowViewModel(node, iterationSource, "iteration", 10d), selectedArrow);
                }

                if (node.InternalFlowBlock is RecursiveCallFlowBlock recursive &&
                    recursive.TargetFlowBlock != null &&
                    _nodesByFlowBlock.TryGetValue(recursive.TargetFlowBlock, out var recursionTarget))
                {
                    AddArrow(new FlowBlockArrowViewModel(
                        node,
                        recursionTarget,
                        "recursive",
                        18d,
                        FlowBloxResourceUtil.GetLocalizedString("ArrowLabel_RecursiveCall", typeof(Resources.ProjectPanel))), selectedArrow);
                }
            }

            if (selectedArrow != null && !Arrows.Any(x => x.IsSelected))
                SelectedArrow = null;
        }

        private void AddArrow(FlowBlockArrowViewModel arrow, FlowBlockArrowViewModel selectedArrow)
        {
            if (arrow.HasSameIdentity(selectedArrow))
            {
                arrow.IsSelected = true;
                _selectedArrow = arrow;
            }

            Arrows.Add(arrow);
        }

        private void SelectAll()
        {
            foreach (var node in Nodes)
                node.IsSelected = true;
            SelectedNode = Nodes.LastOrDefault();
            MarkReferences();
            InvalidateSelectionCommands();
        }

        private void SelectDirectional(Direction direction)
        {
            var anchor = SelectedNode ?? SelectedNodes.LastOrDefault();
            if (anchor == null)
                return;

            foreach (var node in Nodes)
            {
                if (direction == Direction.Left && node.X < anchor.X + 20d)
                    node.IsSelected = true;
                else if (direction == Direction.Right && node.X > anchor.X - 20d)
                    node.IsSelected = true;
                else if (direction == Direction.Up && node.Y < anchor.Y + 20d)
                    node.IsSelected = true;
                else if (direction == Direction.Down && node.Y > anchor.Y - 20d)
                    node.IsSelected = true;
            }

            MarkReferences();
            InvalidateSelectionCommands();
        }

        private void MarkReferences()
        {
            foreach (var node in Nodes)
                node.IsReference = false;

            foreach (var selected in SelectedNodes)
            {
                var context = selected.InternalFlowBlock.IterationContext;
                if (context != null && _nodesByFlowBlock.TryGetValue(context, out var referenceNode))
                    referenceNode.IsReference = true;
            }
        }

        private void AutoLayout()
        {
            if (!HasProject())
                return;

            SyncNodeSizesToModel();
            FlowBlockAutoLayoutAdjuster.Adjust(_registry.GetFlowBlocks());
            var moveActions = FlowBlockAutoLayoutAdjuster.GetRecordedMoveActions();
            FlowBloxServiceLocator.Instance
                .GetService<IFlowBloxActionHistoryService>()
                ?.RegisterAutoLayoutMoves(moveActions);
            RefreshArrows();
        }

        private void SyncNodeSizesToModel()
        {
            foreach (var node in Nodes)
            {
                node.InternalFlowBlock.Size = new System.Drawing.Size(
                    (int)Math.Round(node.Width),
                    (int)Math.Round(node.Height));
            }
        }

        private void DeleteSelection()
        {
            if (!DeleteSelectionCommand.CanExecute(null))
                return;

            var selectedFlowBlocks = SelectedNodes.Select(x => x.InternalFlowBlock).ToList();
            var firstFlowBlock = selectedFlowBlocks.FirstOrDefault();
            if (firstFlowBlock == null)
                return;

            var deleteAction = new FlowBloxDeleteAction
            {
                FlowBlock = firstFlowBlock
            };

            deleteAction.AssociatedActions.AddRange(selectedFlowBlocks.Skip(1).Select(flowBlock => new FlowBloxDeleteAction
            {
                FlowBlock = flowBlock
            }));

            deleteAction.Invoke();
            _componentProvider?.GetCurrentChangelist()?.AddChange(deleteAction);
        }

        private void EditSelection()
        {
            if (SelectedNode == null)
                return;

            ShowPropertyWindow(SelectedNode.InternalFlowBlock);
            SelectedNode.RefreshRows();
        }

        public void OpenRowTarget(FlowBlockRenderRowViewModel row)
        {
            if (row?.CanNavigate != true)
                return;

            ShowPropertyWindow(
                row.Target,
                row.PreselectedProperty,
                row.PreselectedInstance as FlowBloxReactiveObject);
            Refresh();
        }

        public void SelectRow(FlowBlockRenderRowViewModel row)
        {
            foreach (var item in Nodes.SelectMany(x => x.Rows))
                item.IsSelected = ReferenceEquals(item, row);

            CopyRowValueCommand.Invalidate();
        }

        private void CopyRowValue(FlowBlockRenderRowViewModel row)
        {
            if (row?.CanCopyValue != true)
                return;

            Clipboard.SetText(row.Value);
        }

        private void ShowPropertyWindow(object target, string preselectedProperty = null, FlowBloxReactiveObject preselectedInstance = null)
        {
            if (target == null)
                return;

            var dialog = new FlowBlox.UICore.Views.PropertyWindow(new FlowBlox.UICore.Views.PropertyWindowArgs(
                target,
                readOnly: IsRuntimeActive,
                preselectedProperty: preselectedProperty,
                preselectedInstance: preselectedInstance));
            ShowDialog(dialog);
        }

        private void ShowGridSettings()
        {
            if (_project == null)
                return;

            var dialog = new ProjectPanelGridSettingsWindow(_project);
            if (ShowDialog(dialog) == true)
            {
                OnPropertyChanged(nameof(CanvasWidth));
                OnPropertyChanged(nameof(CanvasHeight));
            }
        }

        private void ToggleBreakpoint()
        {
            if (SelectedNode == null)
                return;

            var action = new FlowBloxPropertyChangeAction
            {
                Target = SelectedNode.InternalFlowBlock,
                PropertyName = nameof(BaseFlowBlock.BreakPoint),
                OldValue = SelectedNode.InternalFlowBlock.BreakPoint,
                NewValue = !SelectedNode.InternalFlowBlock.BreakPoint
            };
            action.Invoke();
            _componentProvider?.GetCurrentChangelist()?.AddChange(action);
            SelectedNode.NotifyRuntimeStateChanged();
            OnPropertyChanged(nameof(ToggleBreakpointHeader));
        }

        private void DefineExecutionIndex()
        {
            if (!CanManageExecutionIndex)
                return;

            var dialog = new ExecutionIndexWindow(SelectedNode.InternalFlowBlock.ExecutionIndex);
            if (ShowDialog(dialog) == true)
            {
                var action = new FlowBloxPropertyChangeAction
                {
                    Target = SelectedNode.InternalFlowBlock,
                    PropertyName = nameof(BaseFlowBlock.ExecutionIndex),
                    OldValue = SelectedNode.InternalFlowBlock.ExecutionIndex,
                    NewValue = dialog.Result
                };
                action.Invoke();
                _componentProvider?.GetCurrentChangelist()?.AddChange(action);
                SelectedNode.RefreshRows();
            }
        }

        private void RemoveExecutionIndex()
        {
            if (!CanManageExecutionIndex)
                return;

            var action = new FlowBloxPropertyChangeAction
            {
                Target = SelectedNode.InternalFlowBlock,
                PropertyName = nameof(BaseFlowBlock.ExecutionIndex),
                OldValue = SelectedNode.InternalFlowBlock.ExecutionIndex,
                NewValue = -1
            };
            action.Invoke();
            _componentProvider?.GetCurrentChangelist()?.AddChange(action);
            SelectedNode.RefreshRows();
        }

        private void ShowInputInsight()
        {
            var flowBlock = SelectedNode?.InternalFlowBlock;
            var results = flowBlock?.InputDatasets;
            var currentResult = flowBlock?.InputDataset_CurrentlyProcessing;
            if (results == null || currentResult == null)
                return;

            ShowDialog(new InsightWindow(results, currentResult));
        }

        private void ShowOutputInsight()
        {
            if (SelectedNode?.InternalFlowBlock is not BaseResultFlowBlock flowBlock)
                return;

            var results = flowBlock.GridElementResult.Results;
            var currentResult = flowBlock.OutputDataset_CurrentlyProcessing;
            if (results == null || currentResult == null)
                return;

            ShowDialog(new InsightWindow(results, currentResult));
        }

        private void ManageNotifications()
        {
            if (!CanManageNotifications)
                return;

            ShowDialog(new ManageNotificationsWindow(SelectedNode.InternalFlowBlock));
            SelectedNode.NotifyRuntimeStateChanged();
        }

        private void RemoveConnection(FlowBlockArrowViewModel arrow)
        {
            if (arrow == null || !RemoveConnectionCommand.CanExecute(arrow))
                return;

            if (arrow.Kind == "invoke")
            {
                var disconnectAction = new FlowBloxDisconnectAction
                {
                    From = arrow.From.InternalFlowBlock,
                    To = arrow.To.InternalFlowBlock
                };
                disconnectAction.Invoke();
                _componentProvider?.GetCurrentChangelist()?.AddChange(disconnectAction);
            }
            else if (arrow.Kind == "recursive" && arrow.From.InternalFlowBlock is RecursiveCallFlowBlock recursive)
            {
                var disconnectAction = new FlowBloxRecursiveCallDisconnectAction
                {
                    RecursiveCallFlowBlock = recursive,
                    TargetFlowBlock = recursive.TargetFlowBlock
                };
                disconnectAction.Invoke();
                _componentProvider?.GetCurrentChangelist()?.AddChange(disconnectAction);
            }

            SelectedArrow = null;
            RefreshArrows();
        }

        private static bool CanConnect(FlowBlockNodeViewModel startNode, FlowBlockNodeViewModel endNode)
        {
            if (startNode == null || endNode == null)
                return false;

            if (ReferenceEquals(startNode, endNode))
                return false;

            var startFlowBlock = startNode.InternalFlowBlock;
            var endFlowBlock = endNode.InternalFlowBlock;
            if (startFlowBlock == null || endFlowBlock == null)
                return false;

            if (endFlowBlock.GetInputCardinality() == FlowBlockCardinalities.None)
                return false;

            if (endFlowBlock.GetInputCardinality() == FlowBlockCardinalities.One &&
                endFlowBlock.ReferencedFlowBlocks.Count > 0)
                return false;

            if (endFlowBlock.ReferencedFlowBlocks.Contains(startFlowBlock))
                return false;

            if (startFlowBlock.ReferencedFlowBlocks.Contains(endFlowBlock))
                return false;

            return true;
        }

        private bool AssignFlowBlockName(BaseFlowBlock flowBlock)
        {
            var editValueWindow = new EditValueWindow(flowBlock.Name, false, false)
            {
                Title = string.Format(FlowBloxResourceUtil.GetLocalizedString("AssignFlowBlockName_Title", typeof(Resources.ProjectPanel)), FlowBloxComponentHelper.GetDisplayName(flowBlock)),
                Description = FlowBloxResourceUtil.GetLocalizedString("AssignFlowBlockName_Description", typeof(Resources.ProjectPanel)),
                SelectionStart = flowBlock.NamePrefix.Length,
                SelectionLength = flowBlock.Name.Length - flowBlock.NamePrefix.Length
            };

            if (ShowDialog(editValueWindow) == true)
            {
                var oldName = flowBlock.Name;
                flowBlock.Name = editValueWindow.GetValue();

                if (!ValidationUtil.ValidateProperty(flowBlock, nameof(BaseFlowBlock.Name), out var message))
                {
                    flowBlock.Name = oldName;
                    MessageBox.Show(message);
                    return AssignFlowBlockName(flowBlock);
                }

                return true;
            }

            return false;
        }

        public void UpdateRuntimeState(bool isRuntimeActive, bool isRuntimePaused)
        {
            IsRuntimePaused = isRuntimePaused;
            IsRuntimeActive = isRuntimeActive;
            OnPropertyChanged(nameof(CanExecuteRuntime));
            OnPropertyChanged(nameof(CanPauseRuntime));
            OnPropertyChanged(nameof(CanStopRuntime));
            ExecuteRuntimeCommand.Invalidate();
            PauseRuntimeCommand.Invalidate();
            StopRuntimeCommand.Invalidate();
        }

        private void RuntimeStateService_StateChanged(object? sender, RuntimeStateChangedEventArgs e)
            => SynchronizationContextHelper.PostToUi(_uiContext, () => UpdateRuntimeState(e.IsRuntimeActive, e.IsRuntimePaused));

        private static Type ResolveDraggedFlowBlockType(IDataObject dataObject)
        {
            if (dataObject?.GetDataPresent(FlowBloxFlowBlockDragDropFormats.FlowBlockType) == true &&
                dataObject.GetData(FlowBloxFlowBlockDragDropFormats.FlowBlockType) is string typeName)
                return ResolveFlowBlockType(typeName);

            return null;
        }

        private static Type ResolveFlowBlockType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName, throwOnError: false))
                .FirstOrDefault(type => type != null && typeof(BaseFlowBlock).IsAssignableFrom(type));
        }

        private bool HasProject() => _registry != null && FlowBloxProjectManager.Instance.ActiveProject != null;

        private bool CanEditGrid() => HasProject() && !IsRuntimeActive;

        private bool? ShowDialog(Window window)
            => _dialogService?.ShowWPFDialog(window) ?? window.ShowDialog();

        private void InvalidateSelectionCommands()
        {
            DeleteSelectionCommand.Invalidate();
            EditSelectionCommand.Invalidate();
            ToggleBreakpointCommand.Invalidate();
            DefineExecutionIndexCommand.Invalidate();
            RemoveExecutionIndexCommand.Invalidate();
            ShowInputInsightCommand.Invalidate();
            ShowOutputInsightCommand.Invalidate();
            ManageNotificationsCommand.Invalidate();
            RemoveConnectionCommand.Invalidate();
        }

        private void InvalidateAllCommands()
        {
            SelectModeCommand.Invalidate();
            ConnectionModeCommand.Invalidate();
            SelectAllCommand.Invalidate();
            SelectLeftCommand.Invalidate();
            SelectRightCommand.Invalidate();
            SelectUpCommand.Invalidate();
            SelectDownCommand.Invalidate();
            AutoLayoutCommand.Invalidate();
            ExecuteRuntimeCommand.Invalidate();
            PauseRuntimeCommand.Invalidate();
            StopRuntimeCommand.Invalidate();
            GridSettingsCommand.Invalidate();
            InvalidateSelectionCommands();
        }

        private void ClearNodes()
        {
            foreach (var node in Nodes)
            {
                node.PropertyChanged -= Node_PropertyChanged;
                node.Dispose();
            }

            _nodesByFlowBlock.Clear();
            Nodes.Clear();
            Arrows.Clear();
            SelectedNode = null;
        }

        private void UnsubscribeRegistry()
        {
            if (_registry == null)
                return;

            _registry.OnFlowBlockAdded -= Registry_OnFlowBlockAdded;
            _registry.OnFlowBlockRemoved -= Registry_OnFlowBlockRemoved;
            _registry = null;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void Dispose()
        {
            FlowBloxProjectManager.Instance.ProjectChanged -= ProjectManager_ProjectChanged;
            if (_runtimeStateService != null)
                _runtimeStateService.StateChanged -= RuntimeStateService_StateChanged;

            UnsubscribeRegistry();
            ClearNodes();
        }

        private enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }
    }
}

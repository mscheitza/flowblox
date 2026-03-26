using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid;
using FlowBlox.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using FlowBlox.AppWindow.Handler;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util;
using FlowBlox.Core.Exceptions;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Core.Util.WPF;
using FlowBlox.UICore.Views;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Grid.Provider;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core;
using FlowBlox.Core.Models.Drawing;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.Core.Models.Base;
using FlowBlox.Actions;
using FlowBlox.UICore.Actions;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Utilities;
using SkiaSharp;
using FlowBlox.Core.Constants;

namespace FlowBlox.AppWindow.Contents
{
    public partial class ProjectPanel : DockContent
    {
        private const int GridCenterY = 360;
        private const int GridStepX = 350;
        private const int GridStepY = 160;
        private const int DefaultGridWidth = 2000;
        private const int DefaultGridHeight = 1000;

        private FlowBloxProjectComponentProvider _componentProvider;

        private CustomScrollHandler _customScrollHandler;

        public FlowBlockUIElement _recentFlowBlock = null;

        private LoadProjectView LoadProjectView = null;
        private FlowBloxRuntime Runtime = null;
        private System.Threading.Thread RuntimeThread = null;

        private bool _blockGridUpdate { get; set; }

        internal void EnableGridUpdate() => _blockGridUpdate = false;

        internal void DisableGridUpdate() => _blockGridUpdate = true;

        private string OnInit_FilePath = string.Empty;

        public bool IsRuntimeActive
        {
            get
            {
                return Runtime?.Running == true && !Runtime.Aborted;
            }
        }

        public string RuntimeLogfilePath
        {
            get
            {
                return Runtime?.GetLogfilePath();
            }
        }

        private FlowBloxRegistry FlowBloxRegistry => _componentProvider.GetCurrentRegistry();

        private FlowBloxUIRegistry FlowBloxUIRegistry => _componentProvider.GetCurrentUIRegistry();

        private ProjectChangelist ProjectChangelist => _componentProvider.GetCurrentChangelist();

        public object CopyAction { get; private set; }

        private ShortcutManager _shortcutManager;

        public ProjectPanel()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
            Initialize();
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();
        }

        internal new bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return false;
        }

        private void Initialize()
        {
            pictureBoxInvoke.Image = CreateLegendArrowImage(FlowBloxArrowColors.InvokeArrow);
            pictureBoxRecursiveCall.Image = CreateLegendArrowImage(FlowBloxArrowColors.RecursiveCallArrow);
            pictureBoxIterationContext.Image = CreateLegendArrowImage(FlowBloxArrowColors.IterationContextArrow);
            
            _shortcutManager = new ShortcutManager(toolStrip_Mode);
            _shortcutManager.RegisterShortcut(Keys.Control | Keys.Shift, btConnectionMode);

            this.KeyDown += new KeyEventHandler(_shortcutManager.HandleKeyDown);
            this.KeyUp += new KeyEventHandler(_shortcutManager.HandleKeyUp);

            mainPanel.MouseWheel += new MouseEventHandler(mainPanel_MouseWheel);
            ControlHelper.EnableDoubleBuffer(toolStrip_Mode);
            ControlHelper.EnableOptimizedDoubleBuffer(toolStrip_Mode);
            ControlHelper.EnableDoubleBuffer(toolStrip_Runtime);
            ControlHelper.EnableOptimizedDoubleBuffer(toolStrip_Runtime);
            InitDoubleBuffer();
        }

        private static Image CreateLegendArrowImage(Color tintColor)
        {
            var sKColor = new SKColor(tintColor.R, tintColor.G, tintColor.B, tintColor.A);
            var baseSkImage = FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.arrow_right_thin, 16, sKColor);
            return SkiaToSystemDrawingHelper.ToSystemDrawingImage(baseSkImage);
        }

        private void mainPanel_DoScroll()
        {
            if (_latestGridInteraction != GridInteractions.Scroll)
            {
                this.ScrollGrid();
            }

            if (MouseButtons != MouseButtons.Left)
            {
                this._blockGridUpdate = false;

                if (!background_Scroll.IsBusy)
                {
                    background_Scroll.RunWorkerAsync();
                }
            }
        }

        private void mainPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            mainPanel_DoScroll();
        }

        private void mainPanel_Scroll(object sender, ScrollEventArgs e)
        {
            mainPanel_DoScroll();
        }

        private void mainPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void mainPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                BaseFlowBlock templateFlowBlock = draggedNode.Tag as BaseFlowBlock;

                if (templateFlowBlock != null)
                {
                    Point location = mainPanel.PointToClient(new Point(e.X, e.Y));
                    BaseFlowBlock createdFlowBlock = this.FlowBloxRegistry.CreateFlowBlockUnregistered(templateFlowBlock.GetType());
                    if (AssignFlowBlockName(createdFlowBlock))
                    {
                        var uiElement = CreateGridUIElement(createdFlowBlock, location);
                        FlowBloxUIRegistry.RegisterGridUIElement(uiElement);
                        this.FlowBloxRegistry.PostProcessFlowBlockCreated(createdFlowBlock);
                        this.FlowBloxRegistry.RegisterFlowBlock(createdFlowBlock);
                    }
                }
            }
        }

        private delegate void UpdateUIMethod(bool GridUpdate, bool appWindowUpdate);

        public void UpdateUI(bool gridUpdate = true, bool appWindowUpdate = false)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateUIMethod(UpdateUI), new object[] { gridUpdate, appWindowUpdate });
                return;
            }

            var project = FlowBloxProjectManager.Instance.ActiveProject;

            BackColor = (project != null) ? Color.FromKnownColor(KnownColor.Control) : Color.FromArgb(70, 70, 70);
            btExecute.Enabled = (project != null) && (!IsRuntimeActive || Runtime.Pause);
            btGridSettings.Enabled = (project != null) && (!IsRuntimeActive || Runtime.Pause);
            btStopExecution.Enabled = (project != null) && IsRuntimeActive;
            btPause.Enabled = (project != null) && IsRuntimeActive && !Runtime.Pause;
            itmEditElement.Enabled = (_recentFlowBlock != null);

            string editElementText = !IsRuntimeActive ?
                FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_itmEditElement_Edit_Text", typeof(FlowBloxMainUITexts)) :
                FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_itmEditElement_View_Text", typeof(FlowBloxMainUITexts));
            itmEditElement.Text = editElementText;

            itmDeleteConnection.Visible = (project != null) && _selectedArrows.Any();
            itmDeleteConnection.Enabled = (project != null) && _selectedArrows.Any() && !IsRuntimeActive;
            itmDeleteElement.Visible = (project != null) && !_selectedArrows.Any();
            itmDeleteElement.Enabled = (project != null) && HasSelectedGridElements() && !IsRuntimeActive;
            itmIndex.Enabled = (_recentFlowBlock != null) && !(_recentFlowBlock.InternalFlowBlock is NoteFlowBlock) && !IsRuntimeActive;
            itmBreakPoint.Enabled = (_recentFlowBlock != null) && !(_recentFlowBlock.InternalFlowBlock is NoteFlowBlock);

            string breakPointText = ((_recentFlowBlock != null) && (_recentFlowBlock.BreakPoint)) ?
                FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_itmBreakPoint_Remove_Text", typeof(FlowBloxMainUITexts)) :
                FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_itmBreakPoint_Set_Text", typeof(FlowBloxMainUITexts));
            itmBreakPoint.Text = breakPointText;
            itmBreakPoint.Enabled = _recentFlowBlock != null;

            itmManageNotifications.Enabled = (_recentFlowBlock != null) && _recentFlowBlock.InternalFlowBlock.NotificationTypes?.Any() == true;
            itmInsightInput.Enabled = (_recentFlowBlock?.InternalFlowBlock as BaseFlowBlock)?.InputDataset_CurrentlyProcessing != null;
            itmInsightOutput.Enabled = (_recentFlowBlock?.InternalFlowBlock as BaseResultFlowBlock)?.OutputDataset_CurrentlyProcessing != null;

            this.btSelectionMode.Enabled = (project != null) && (!IsRuntimeActive);
            this.btConnectionMode.Enabled = (project != null) && (!IsRuntimeActive);

            if (gridUpdate)
            {
                if (project != null && !background_PrintGrid.IsBusy)
                    background_PrintGrid.RunWorkerAsync();
            }

            if (appWindowUpdate)
                AppWindow.Instance.UpdateUI();
        }

        internal void OnAfterUIRegistryInitialized()
        {
            // Toolbar
            if (!this.btSelectionMode.Checked &&
                !this.btConnectionMode.Checked)
            {
                this.btSelectionMode.Checked = true;
                this.btConnectionMode.Checked = false;
            }

            if (this.btSelectionMode.Checked)
                this.ActivateSelectionMode();

            if (this.btConnectionMode.Checked)
                this.ActivateConnectionMode();


            // Handler
            _customScrollHandler = new CustomScrollHandler(mainPanel);
            _customScrollHandler.Register();
        }

        internal void OnAfterProjectOpened(FlowBloxProject project)
        {
            InitDoubleBuffer();

            mainPanel.Controls.Clear();
            mainPanel.VerticalScroll.Value = 0;
            mainPanel.HorizontalScroll.Value = 0;

            foreach (var element in FlowBloxRegistry.GetFlowBlocks())
            {
                var uiElement = this.CreateGridUIElement(element);
                this.FlowBloxUIRegistry.RegisterGridUIElement(uiElement);
            }

            this.mainPanel.AutoScrollPosition = new Point(0, 0);

            if ((project.GridSizeX > 0) &&
                (project.GridSizeY > 0))
            {
                this.mainPanel.AutoScrollMinSize = new Size(project.GridSizeX, project.GridSizeY);
            }

            this._recentFlowBlock = null;

            this.UpdateUI();
        }

        private void UpdateGridWarnings()
        {
            if (_notExecutedElements.Count > 0)
            {
                AppWindow.Instance.labelWarning.Text = string.Format(
                    FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_Warning_NotExecutedElements_Text", typeof(FlowBloxMainUITexts)),
                    string.Join(", ", _notExecutedElements));
                AppWindow.Instance.WarningStrip.Visible = true;
            }
            else
            {
                AppWindow.Instance.WarningStrip.Visible = false;
            }
        }

        private int GetCreationIndex(BaseFlowBlock createdElement)
        {
            int creationIndex = 0;
            foreach (BaseFlowBlock element in FlowBloxRegistry.GetFlowBlocks())
            {
                if (element.GetType() == createdElement.GetType())
                    creationIndex++;
            }
            return creationIndex;
        }

        private bool AssignFlowBlockName(BaseFlowBlock flowBlock)
        {
            EditValueWindow editValueWindow = new EditValueWindow(flowBlock.Name, false, false)
            {
                Title = string.Format(FlowBloxResourceUtil.GetLocalizedString($"ProjectPanel_AssignFlowBlockName_Title"), FlowBloxComponentHelper.GetDisplayName(flowBlock)),
                Description = FlowBloxResourceUtil.GetLocalizedString($"ProjectPanel_AssignFlowBlockName_Description"),
                SelectionStart = flowBlock.NamePrefix.Length,
                SelectionLength = flowBlock.Name.Length - flowBlock.NamePrefix.Length
            };

            if (editValueWindow.ShowDialog(this) == DialogResult.OK)
            {
                string oldName = flowBlock.Name;
                string newName = editValueWindow.GetValue();
                flowBlock.Name = newName;

                string message;
                if (!ValidationUtil.ValidateProperty(flowBlock, nameof(BaseFlowBlock.Name), out message))
                {
                    flowBlock.Name = oldName;
                    FlowBloxMessageBox.Show(this, message);
                    return AssignFlowBlockName(flowBlock);
                }
                return true;
            }
            return false;
        }

        private void EditReactiveObject(FlowBlockUIElement flowBlockUIElement, FlowBloxReactiveObject reactiveObject, string propertyName = null, object selectedInstance = null)
        {
            var propertyWindowArgs = new PropertyWindowArgs(
                reactiveObject,
                readOnly: IsRuntimeActive,
                preselectedProperty: propertyName,
                preselectedInstance: (FlowBloxReactiveObject)selectedInstance);

            var propertyViewWpf = new UICore.Views.PropertyWindow(propertyWindowArgs);
            var owner = ControlHelper.FindParentOfType<Form>(this, true);
            WindowsFormWPFHelper.ShowDialog(propertyViewWpf, owner);

            flowBlockUIElement.RefreshSize();
            UpdateUI(appWindowUpdate: true);
        }

        private void EditFlowBlock(FlowBlockUIElement flowBlockUIElement, string propertyName = null, object selectedInstance = null)
        {
            var propertyWindowArgs = new PropertyWindowArgs(
                flowBlockUIElement.InternalFlowBlock,
                readOnly: IsRuntimeActive,
                preselectedProperty: propertyName);

            var propertyViewWpf = new UICore.Views.PropertyWindow(propertyWindowArgs);
            var owner = ControlHelper.FindParentOfType<Form>(this, true);
            WindowsFormWPFHelper.ShowDialog(propertyViewWpf, owner);

            flowBlockUIElement.RefreshSize();
            UpdateUI(appWindowUpdate: true);
        }

        private void itmEditElement_Click(object sender, EventArgs e)
        {
            if (_recentFlowBlock == null)
                return;

            EditFlowBlock(_recentFlowBlock);
        }

        private bool HasSelectedGridElements()
        {
            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                foreach (var uiElement in FlowBloxUIRegistry.UIElements)
                {
                    if (uiElement.ElementSelected)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private IEnumerable<FlowBlockUIElement> GetSelectedGridElements()
        {
            List<FlowBlockUIElement> selectedGridElements = new List<FlowBlockUIElement>();
            if (FlowBloxUIRegistry == null)
                return selectedGridElements;

            foreach (FlowBlockUIElement uiElement in FlowBloxUIRegistry.UIElements
                .Where(x => x.ElementSelected)
                .OrderBy(x => x.ElementSelectedAt))
            {
                selectedGridElements.Add(uiElement);
            }
            return selectedGridElements;
        }

        private void itmDeleteElement_Click(object sender, EventArgs e)
        {
            var gridUIElements = GetSelectedGridElements();
            if (gridUIElements.Count() > 0)
            {
                var selectedFlowBlocks = gridUIElements.Select(x => x.InternalFlowBlock);
                var selectedDefinedManagedObjects = selectedFlowBlocks.SelectMany(x => x.DefinedManagedObjects);
                var allFlowBlocks = FlowBloxRegistry.GetFlowBlocks();
                var allReferences = new List<string>();

                foreach (var selectedFlowBlock in selectedFlowBlocks)
                {
                    if (!selectedFlowBlock.IsDeletable(out List<BaseFlowBlock> dependendFlowBlocks))
                    {
                        dependendFlowBlocks.RemoveAll(x => selectedFlowBlocks.Contains(x));
                        if (dependendFlowBlocks.Any())
                            dependendFlowBlocks.ForEach(flowBlock => allReferences.Add(
                                string.Format(FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Message_Entry"), 
                                    selectedFlowBlock, 
                                    flowBlock)));
                    }

                    foreach (var managedObject in selectedFlowBlock.DefinedManagedObjects)
                    {
                        if (!managedObject.IsDeletable(out List<IFlowBloxComponent> dependendComponents))
                        {
                            dependendComponents.RemoveAll(x => selectedFlowBlocks.Contains(x));
                            dependendComponents.RemoveAll(x => selectedDefinedManagedObjects.Contains(x));
                            if (dependendComponents.Any())
                                dependendComponents.ForEach(comp => allReferences.Add(
                                    string.Format(FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Message_Entry"), 
                                        managedObject, 
                                        comp)));
                        }
                    }
                }

                if (allReferences.Any())
                {
                    FlowBloxMessageBox.Show
                        (
                            this,
                            string.Format(FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Message"), string.Join(Environment.NewLine, allReferences.Select(description => string.Concat(" - ", description)))),
                            FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Title"),
                            FlowBloxMessageBox.Buttons.OK,
                            FlowBloxMessageBox.Icons.Info
                        );

                    UpdateUI();

                    return;
                }

                var deleteAction = new FlowBloxDeleteAction()
                {
                    MainPanel = this.mainPanel,
                    UIElement = gridUIElements.First()
                };

                deleteAction.AssociatedActions.AddRange(gridUIElements.Skip(1).Select(uiElement =>
                {
                    return new FlowBloxDeleteAction()
                    {
                        MainPanel = this.mainPanel,
                        UIElement = uiElement
                    };
                }));

                deleteAction.Invoke();

                this.ProjectChangelist.AddChange(deleteAction);

                this.PrintGrid(true);
                this.UpdateUI(false, true);

                AppWindow.Instance.UpdateUI();
            }
        }

        internal void OnAfterProjectCreated()
        {
            InitDoubleBuffer();

            int gridSizeX = DefaultGridWidth;
            int gridSizeY = DefaultGridHeight;

            if (FlowBloxOptions.GetOptionInstance().HasOption("Grid.DefaultSize"))
            {
                string gridSize = FlowBloxOptions.GetOptionInstance().OptionCollection["Grid.DefaultSize"].Value;
                string[] globalGridSize = gridSize.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                int GlobalGridSizeX = int.Parse(globalGridSize[0].Trim());
                int GlobalGridSizeY = int.Parse(globalGridSize[1].Trim());

                if ((GlobalGridSizeX > 0) && (GlobalGridSizeY > 0))
                {
                    gridSizeX = GlobalGridSizeX;
                    gridSizeY = GlobalGridSizeY;
                }
            }

            this.mainPanel.AutoScrollPosition = new Point(0, 0);
            this.mainPanel.AutoScrollMinSize = new Size(gridSizeX, gridSizeY);
        }

        internal void OnAfterProjectClosed()
        {
            this._recentFlowBlock = null;
            this.mainPanel.Controls.Clear();
        }

        private void Background_PrintGrid_DoWork(object sender, DoWorkEventArgs e)
        {
            int timeunit = PrintGridTimeunit;
            bool result = true;
            if (e.Argument is object[])
            {
                timeunit = (int)((object[])e.Argument)[0];
                result = (bool)((object[])e.Argument)[1];
            }
            System.Threading.Thread.Sleep(timeunit);
            e.Result = result;
        }

        private void Background_PrintGrid_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateExecutionStatus();
            UpdateGridWarnings();
            PrintGrid((bool)e.Result);
            UpdateAllElementBorders();
        }

        private void itmStopExecution_Click(object sender, EventArgs e)
        {
            Runtime.Aborted = true;
            UpdateUI();
        }

        private void btPause_Click(object sender, EventArgs e)
        {
            Runtime.Pause = true;
            UpdateUI();
        }

        void Runtime_PauseContinue(bool IsPaused)
        {
            UpdateUI();
        }

        private void btStopExecution_Click(object sender, EventArgs e)
        {
            Runtime.Aborted = true;
            UpdateUI();
        }

        internal void OnBeforeSaveProject(FlowBloxProject project)
        {
            project.GridSizeX = this.mainPanel.AutoScrollMinSize.Width;
            project.GridSizeY = this.mainPanel.AutoScrollMinSize.Height;
        }

        private void itmExecute_Click(object sender, EventArgs e)
        {
            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                if (IsRuntimeActive && this.Runtime.Pause)
                {
                    this.Runtime.Pause = false;
                    UpdateUI();
                }
                else
                {
                    try
                    {
                        FlowBloxRuntime runtime = new FlowBloxRuntime(FlowBloxProjectManager.Instance.ActiveProject);
                        this.RuntimeThread = new System.Threading.Thread(new System.Threading.ThreadStart(runtime.Execute));
                        runtime.RuntimeStarted += new BaseRuntime.RuntimeStartedEventHandler(Runtime_Started);
                        runtime.Finish += new BaseRuntime.FinishedEventHandler(Runtime_Finish);
                        runtime.FocusChanged += new BaseRuntime.FocusChangedEventHandler(Runtime_FocusChanged);
                        runtime.PauseContinue += new BaseRuntime.PauseEventHandler(Runtime_PauseContinue);
                        this.Runtime = runtime;
                        this.Runtime.Running = true;

                        AppWindow.Instance.OnBeforeRuntimeStarted(runtime);

                        RuntimeThread.Start();
                        UpdateUI();
                    }
                    catch (Exception ex)
                    {
                        FlowBloxMessageBox.Show(this,
                            string.Format(
                                FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_RuntimeStartFailed_Message", typeof(FlowBloxMainUITexts)),
                                ex.Message),
                            FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_RuntimeStartFailed_Title", typeof(FlowBloxMainUITexts)),
                            FlowBloxMessageBox.Buttons.OK,
                            FlowBloxMessageBox.Icons.Info);
                    }
                }
            }
        }

        void Runtime_FocusChanged(BaseFlowBlock gridElement)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FlowBloxRuntime.FocusChangedEventHandler(Runtime_FocusChanged), new object[1] { gridElement });
                return;
            }

            if (gridElement != null)
            {
                var uiElement = FlowBloxUIRegistry.GetUIElementToGridElement(gridElement);
                foreach (var otherUIElement in FlowBloxUIRegistry.UIElements)
                {
                    otherUIElement.MarkElementRuntime(false);
                }
                uiElement?.MarkElementRuntime(true);
            }
        }

        private void Runtime_Started(BaseRuntime runtime)
        {
            foreach (var uiElement in FlowBloxUIRegistry.UIElements)
            {
                uiElement.AnchorSize = uiElement.Size;
            }
        }

        void Runtime_Finish(object result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BaseRuntime.FinishedEventHandler(Runtime_Finish), new object[1] { result });
                return;
            }

            foreach (var uiElement in this.FlowBloxUIRegistry.UIElements)
            {
                uiElement.InternalFlowBlock.ResetNotifications(this.Runtime);
                uiElement.AnchorSize = null;
            }

            if (result is Exception)
            {
                var exception = (Exception)result;
                if (!(exception is RuntimeCancellationException))
                {
                    FlowBloxMessageBox.Show(this,
                        string.Format(
                            FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_RuntimeAborted_Message", typeof(FlowBloxMainUITexts)),
                            exception.ToString(),
                            Environment.NewLine),
                        FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_RuntimeAborted_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Error);
                }
            }

            this.Runtime = null;
            AppWindow.Instance.OnAfterRuntimeFinished();
            UpdateUI();
        }

        void RuntimeWindow_VisibleChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void btGridSettings_Click(object sender, EventArgs e)
        {
            GridView GridView = new GridView(mainPanel.AutoScrollMinSize.Width, mainPanel.AutoScrollMinSize.Height);
            GridView.ShowDialog(this);
            mainPanel.AutoScrollMinSize = new Size(GridView.Width, GridView.Height);
            UpdateUI();
        }

        private void itmDefineIndex_Click(object sender, EventArgs e)
        {
            if (_recentFlowBlock != null)
            {
                var gridElement = _recentFlowBlock.InternalFlowBlock;
                string value = (gridElement.ElementIndex >= 0) ? gridElement.ElementIndex.ToString() : string.Empty;

                EditValueWindow editValueWindow;
                if (value.Equals(string.Empty))
                {
                    editValueWindow = new EditValueWindow(false, false);
                }
                else
                {
                    editValueWindow = new EditValueWindow(value, false, false);
                }

                editValueWindow.SetParameterName("Element.Index");
                editValueWindow.SetMode(EditMode.Developer);
                editValueWindow.ShowDialog(this);
                if (!string.IsNullOrEmpty(editValueWindow.GetValue()))
                {
                    int index;
                    if (int.TryParse(editValueWindow.GetValue(), out index))
                        gridElement.ElementIndex = index;
                }
                _recentFlowBlock.UpdateFlags();
                _recentFlowBlock.RefreshSize();
                UpdateUI();
            }
        }

        private void mainPanel_Click(object sender, EventArgs e)
        {
            foreach (var uiElement in FlowBloxUIRegistry.UIElements)
            {
                if (_lastMouseButton == MouseButtons.Left)
                    uiElement.MarkElement(ElementState.Unmarked);
            }
            _recentFlowBlock = null;
            if (_latestGridInteraction != GridInteractions.PrintReferenceLines)
                PrintGrid(true);

            UpdateAllElementBorders();
            UpdateUI(false);
        }

        private void itmSelection_Left_Click(object sender, EventArgs e)
        {
            DoSelection(Keys.Left);
        }

        private void itmSelection_Right_Click(object sender, EventArgs e)
        {
            DoSelection(Keys.Right);
        }

        private void itmSelection_Up_Click(object sender, EventArgs e)
        {
            DoSelection(Keys.Up);
        }

        private void itmSelection_Down_Click(object sender, EventArgs e)
        {
            DoSelection(Keys.Down);
        }

        private void itmSelection_All_Click(object sender, EventArgs e)
        {
            DoSelection(Keys.A);
        }

        private void itmBreakPoint_Click(object sender, EventArgs e)
        {
            if (_recentFlowBlock != null)
            {
                _recentFlowBlock.BreakPoint = !_recentFlowBlock.BreakPoint;
                _recentFlowBlock.UpdateFlags();
                _recentFlowBlock.RefreshSize();
            }
            UpdateUI();
        }

        private void mainPanel_Resize(object sender, EventArgs e)
        {
            InitDoubleBuffer();
            UpdateUI();
        }

        internal void Undo()
        {
            var changelist = this.ProjectChangelist;
            if (changelist.ChangeIndex == -1)
                return;


            var action = changelist.Changes[changelist.ChangeIndex];
            action.Undo();
            changelist.ChangeIndex--;
            PrintGrid(action is not FlowBloxMoveAction);
            UpdateUI(action is not FlowBloxMoveAction, true);
        }

        internal void Redo()
        {
            var changelist = this.ProjectChangelist;
            if (changelist.ChangeIndex == changelist.Changes.Count - 1)
                return;

            changelist.ChangeIndex++;
            var action = changelist.Changes[changelist.ChangeIndex];
            action.Invoke();
            PrintGrid(action is not FlowBloxMoveAction);
            UpdateUI(action is not FlowBloxMoveAction, true);
        }

        private void Background_MoveFinished_DoWork(object sender, DoWorkEventArgs e) { System.Threading.Thread.Sleep(PrintGridTimeunit); }

        private void Background_MoveFinished_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_latestGridInteraction != GridInteractions.PrintReferenceLines)
            {
                this._blockGridUpdate = false;

                if (!background_PrintGrid.IsBusy)
                    background_PrintGrid.RunWorkerAsync();
            }

            UpdateUI(false, true);
        }


        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(BufferedPPC, 0, 0, mainPanel.ClientRectangle.Width, mainPanel.ClientRectangle.Height);
        }

        private HashSet<FlowBloxArrow> _selectedArrows = new HashSet<FlowBloxArrow>();
        private MouseButtons _lastMouseButton;
        private void mainPanel_MouseDown(object sender, MouseEventArgs e)
        {
            _lastMouseButton = e.Button;

            if (!mainPanel.Enabled || !mainPanel.Visible)
                return;

            if (_modeHandler != null)
            {
                _modeHandler.DoMouseDown(e.Button, e.Location);
                _blockGridUpdate = false;
            }


            // Überprüfung, ob ein Arrow ausgewählt wurde:
            _selectedArrows.Clear();

            foreach (var arrow in _drawnArrows)
            {
                if (arrow.IntersectsWith(e.Location))
                {
                    _selectedArrows.Add(arrow);
                    break;
                }
            }
        }

        private void mainPanel_MouseMove(object sender, MouseEventArgs e)
        {
            _modeHandler?.DoMouseMove(e.Button, e.Location);
        }

        private void mainPanel_MouseUp(object sender, MouseEventArgs e)
        {
            _modeHandler?.DoMouseUp(e.Button);
            UpdateUI();
        }

        private void background_Scroll_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ScrollGrid();
        }

        private void background_Scroll_DoWork(object sender, DoWorkEventArgs e) 
        { 
            System.Threading.Thread.Sleep(ScrollGridTimeunit); 
        }

        private void itmRemoveIndex_Click(object sender, EventArgs e)
        {
            if (_recentFlowBlock != null)
            {
                _recentFlowBlock.InternalFlowBlock.ElementIndex = -1;
                _recentFlowBlock.UpdateFlags();
                _recentFlowBlock.RefreshSize();
                UpdateUI();
            }
        }

        private ModeHandlerBase _modeHandler;

        private void ActivateSelectionMode()
        {
            btSelectionMode.Checked = true;
            btConnectionMode.Checked = false;
        }

        // Diese Methode wird aufgerufen, wenn der "Auswahl"-Button angeklickt wird.
        private void buttonSelectionMode_Click(object sender, EventArgs e)
        {
            ActivateSelectionMode();
        }

        private void buttonSelectionMode_CheckedChanged(object sender, EventArgs e)
        {
            if (btSelectionMode.Checked)
            {
                SetFlowBlockSelectionEnabled(true);
                _modeHandler = new SelectionModeHandler(background_PrintGrid, PrintGridMouseMoveTimeunit);
            }
        }

        private bool _isFlowBlockSelectionEnabled;
        private void SetFlowBlockSelectionEnabled(bool enabled)
        {
            this._isFlowBlockSelectionEnabled = enabled;
            foreach (var uiElement in FlowBloxUIRegistry.UIElements.Where(x => x.CanSelect != enabled))
            {
                uiElement.CanSelect = enabled;
            }
        }

        private void ActivateConnectionMode()
        {
            btSelectionMode.Checked = false;
            btConnectionMode.Checked = true;
        }

        // Diese Methode wird aufgerufen, wenn der "Verbinden"-Button angeklickt wird.
        private void buttonConnectionMode_Click(object sender, EventArgs e)
        {
            ActivateConnectionMode();
        }

        private void buttonConnectionMode_CheckedChanged(object sender, EventArgs e)
        {
            if (btConnectionMode.Checked)
            {
                SetFlowBlockSelectionEnabled(false);
                _modeHandler = new ConnectionModeHandler(background_PrintGrid, PrintGridMouseMoveTimeunit);
            }
        }

        private List<BaseFlowBlock> _copiedFlowBlocks = new List<BaseFlowBlock>();

        internal void Copy()
        {
            _copiedFlowBlocks.Clear();
            foreach (var uiElement in GetSelectedGridElements())
            {
                DynamicDeepCopier dynamicDeepCopier = new DynamicDeepCopier(FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(uiElement.InternalFlowBlock));
                var copy = (BaseFlowBlock)dynamicDeepCopier.Copy(uiElement.InternalFlowBlock);
                copy.Location = new Point(uiElement.Location.X + uiElement.Width + 20, uiElement.Location.Y);
                copy.Name = string.Format(
                    FlowBloxResourceUtil.GetLocalizedString("ProjectPanel_Copy_NameFormat", typeof(FlowBloxMainUITexts)),
                    uiElement.Name);
                _copiedFlowBlocks.Add(copy);
            }
        }

        internal void Paste()
        {
            foreach (var copy in _copiedFlowBlocks)
            {
                var uiElement = CreateGridUIElement(copy, copy.Location);

                FlowBloxRegistry.RegisterFlowBlock(copy);
                FlowBloxUIRegistry.RegisterGridUIElement(uiElement);
            }
        }

        internal void SaveInnerPanelBitmap(string fileName)
        {
            Panel2Bitmap.SaveBitmap(mainPanel, fileName);
        }

        private void itmRefresh_Click(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void itmInsightInput_Click(object sender, EventArgs e)
        {
            var flowBlock = _recentFlowBlock?.InternalFlowBlock as BaseFlowBlock;
            if (flowBlock == null)
                return;

            var results = flowBlock.InputDatasets;
            if (results == null)
                return;

            var currentResult = flowBlock.InputDataset_CurrentlyProcessing;
            if (currentResult == null)
                return;

            WindowsFormWPFHelper.ShowDialog(new InsightWindow(results, currentResult), this.FindForm());
        }

        private void itmInsightOutput_Click(object sender, EventArgs e)
        {
            var flowBlock = _recentFlowBlock?.InternalFlowBlock as BaseResultFlowBlock;
            if (flowBlock == null)
                return;

            var results = flowBlock.GridElementResult.Results;
            if (results == null)
                return;

            var currentResult = flowBlock.OutputDataset_CurrentlyProcessing;
            if (currentResult == null)
                return;

            WindowsFormWPFHelper.ShowDialog(new InsightWindow(results, currentResult), this.FindForm());
        }

        private void itmManageNotifications_Click(object sender, EventArgs e)
        {
            if (_recentFlowBlock == null)
                return;

            var dialog = new ManageNotificationsWindow(_recentFlowBlock.InternalFlowBlock);
            var owner = ControlHelper.FindParentOfType<Form>(this, true);
            WindowsFormWPFHelper.ShowDialog(dialog, owner);
            _recentFlowBlock.UpdateFlags();
        }

        private void itmDeleteConnection_Click(object sender, EventArgs e)
        {
            foreach(var selectedArrow in _selectedArrows)
            {
                var disconnectAction = new FlowBloxDisconnectAction()
                {
                    From = selectedArrow.From.InternalFlowBlock,
                    To = selectedArrow.To.InternalFlowBlock
                };

                disconnectAction.Invoke();

                _componentProvider.GetCurrentChangelist().AddChange(disconnectAction);

                UpdateUI(true);
            }
        }
    }
}

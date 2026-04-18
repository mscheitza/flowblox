using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FlowBlox.Grid;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Drawing.Drawing2D;
using FlowBlox.Core.Provider;
using FlowBlox.AppWindow.Handler;
using FlowBlox.Grid.Elements.UserControls;
using static FlowBlox.Grid.Elements.UserControls.FlowBlockUIElement;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Util.Drawing;
using FlowBlox.Core.Logging;
using FlowBlox.Grid.Elements.UI;
using FlowBlox.Core.Models.Drawing;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Components.Modifier;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Constants;

namespace FlowBlox.AppWindow.Contents
{
    // In diesem Teil werden die folgenden Grid Interaktionen abgebildet:
    // 
    // - Verhalten bei Selektion eines Grid Elements
    // - Verhalten bei Doppelklick eines Grid Elements
    // - Kardinalität ausgeben bei Klick auf mindestens ein Grid Element
    // - Scrollverhalten
    // - Neue Grid Elemente auf dem Grid platzieren

    public partial class ProjectPanel
    {
        public const int DefaultGridElementSizeX = 314;
        public const int DefaultGridElementSizeY = 0;

        private const int PrintGridTimeunit = 40;
        private const int PrintGridMoveTimeunit = 5;
        private const int PrintGridMouseMoveTimeunit = 15;
        private const int ScrollGridTimeunit = 25;

        private enum GridInteractions 
        { 
            None, 
            Print, 
            PrintReferenceLines, 
            HandlerPrintInteraction, 
            Scroll, 
            MoveElement 
        };

        private GridInteractions _latestGridInteraction = GridInteractions.None;
        private List<BaseFlowBlock> _notExecutedElements = new List<BaseFlowBlock>();

        /// <summary>
        /// Double Buffer: Bitmap Objekt zur Zwischenspeicherung des <c>ProjectPanel</c> Contents. Wird über den EventHandler <c>mainPanel_Paint</c>
        /// auf das <c>ProjectPanel</c> ausgegeben.
        /// </summary>
        private Bitmap BufferedPPC = null;

        /// <summary>
        /// Initialisiert den DoubleBuffer und befüllt ihn mit dem Standard Background Image.
        /// </summary>
        private void InitDoubleBuffer()
        {
            this.BufferedPPC = new Bitmap(mainPanel.ClientRectangle.Width, mainPanel.ClientRectangle.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics Graphics = Graphics.FromImage(BufferedPPC);
            PrintBackground(Graphics);
            Graphics.Dispose();
        }

        public FlowBlockUIElement CreateGridUIElement(BaseFlowBlock gridElement)
        {
            return CreateGridUIElement(gridElement, new Point(0, 0));
        }

        public FlowBlockUIElement CreateGridUIElement(BaseFlowBlock gridElement, Point location)
        {
            if (gridElement == null)
                throw new ArgumentNullException(nameof(gridElement));

            if ((location.X == 0) && (location.Y == 0))
                location = gridElement.Location;

            if (location.X < 0)
                location.X = 0;

            if (location.Y < 0)
                location.Y = 0;

            var gridUIElement = gridElement is NoteFlowBlock ? 
                new NoteUIElement(gridElement) : 
                new FlowBlockUIElement(gridElement);

            gridUIElement.CanSelect = _isFlowBlockSelectionEnabled;

            gridUIElement.ContextMenuStrip = contextMenuStrip;

            gridUIElement.MouseEnter += new EventHandler(baseGridElement_MouseEnter);
            gridUIElement.MouseHover += new EventHandler(baseGridElement_MouseEnter);
            gridUIElement.GetHeader().MouseClick += new MouseEventHandler(baseGridElement_MouseClick);
            gridUIElement.GetHeader().MouseDown += new MouseEventHandler(baseGridElement_MouseDown);
            gridUIElement.GetHeader().MouseMove += new MouseEventHandler(baseGridElement_MouseMove);
            gridUIElement.GetHeader().MouseUp += new MouseEventHandler(baseGridElement_MouseUp);
            gridUIElement.Move += new EventHandler(baseGridElement_Move);
            gridUIElement.GridElementMovedByUser += new GridElementMovedEventHandler(Element_GridElementMovedByUser);
            gridUIElement.MoveFinished += new MoveFinishedEventHandler(baseGridElement_MoveFinished);
            gridUIElement.DoubleClick += new DoubleClickEventHandler(baseGridElement_DoubleClick);
            gridUIElement.PropertyDoubleClick += baseGridElement_PropertyDoubleClick;
            gridUIElement.ListPropertyDoubleClick += baseGridElement_ListPropertyDoubleClick;
            gridUIElement.ResultFieldDoubleClick += baseGridElement_ResultFieldDoubleClick;
            gridUIElement.RequiredFieldDoubleClick += baseGridElement_RequiredFieldDoubleClick;
            gridUIElement.ConditionDoubleClick += baseGridElement_ConditionDoubleClick;
            gridUIElement.ModifierDoubleClick += baseGridElement_ModifierDoubleClick;

            mainPanel.Controls.Add(gridUIElement);

            gridUIElement.BorderStyle = BorderStyle.FixedSingle;
            gridUIElement.Location = location;
            gridUIElement.Name = string.IsNullOrEmpty(gridElement.Name) ? gridElement.GetType().Name + GetCreationIndex(gridElement).ToString() : gridElement.Name;
            gridUIElement.Size = new Size(DefaultGridElementSizeX, DefaultGridElementSizeY);
            gridUIElement.RefreshSize();

            gridUIElement.ApplicationWindowRef = AppWindow.Instance;

            return gridUIElement;
        }

        private void baseGridElement_ConditionDoubleClick(FlowBlockUIElement sender, FlowBloxReactiveObject condition) => EditReactiveObject(sender, condition);

        private void baseGridElement_ModifierDoubleClick(FlowBlockUIElement sender, ModifierBase modifier) => EditReactiveObject(sender, modifier);

        private void baseGridElement_RequiredFieldDoubleClick(FlowBlockUIElement sender, FlowBloxComponent component, FieldElement fieldElement)
        {
            if (component is BaseFlowBlock)
                EditFlowBlock(sender, nameof(FlowBloxComponent.RequiredFields), fieldElement);
            else
                EditReactiveObject(sender, component, nameof(FlowBloxComponent.RequiredFields), fieldElement);
        }

        private void baseGridElement_ResultFieldDoubleClick(FlowBlockUIElement sender, FieldElement fieldElement) => EditReactiveObject(sender, fieldElement);

        private void baseGridElement_PropertyDoubleClick(FlowBlockUIElement sender, string propertyName)
        {
            EditFlowBlock(sender, propertyName);
        }

        private void baseGridElement_ListPropertyDoubleClick(FlowBlockUIElement sender, string propertyName, object propertyInstance)
        {
            EditFlowBlock(sender, propertyName, propertyInstance);
        }

        private void baseGridElement_DoubleClick(object sender)
        {
            _recentFlowBlock = (FlowBlockUIElement)sender;
            _recentFlowBlock.BlockMoveEvent = true;
            itmEditElement_Click(null, null);
        }

        /// <summary>
        /// Methode zur automatischen Ausrichtung von Grid Elementen.
        /// </summary>
        /// <param name="movedElement"></param>
        internal void AlignElement(FlowBlockUIElement movedElement)
        {
            if ((FlowBloxProjectManager.Instance.ActiveProject != null) && !movedElement.IsMultiSelect())
            {
                if (!background_Align.IsBusy)
                {
                    background_Align.RunWorkerAsync(movedElement);
                }
            }
        }

        private void Background_Align_DoWork(object sender, DoWorkEventArgs e)
        {
            const int Tolerance = 10;
            bool BlockLocationUpdateX = false;
            bool BlockLocationUpdateY = false;
            FlowBlockUIElement movedElement = (FlowBlockUIElement)e.Argument;
            Point SetLocation = movedElement.Location;
            try
            {
                int MovedMiddleX = movedElement.BufferedLocation.X + (movedElement.Width / 2);
                int MovedMiddleY = movedElement.BufferedLocation.Y + (movedElement.Height / 2);

                var el = FlowBloxUIRegistry.UIElements.Where(G => (G != movedElement));
                var lr = el.Where(G => (G.Location.Y < MovedMiddleY) && ((G.Location.Y + G.Height) > MovedMiddleY));
                var ul = el.Where(G => (G.Location.X < MovedMiddleX) && ((G.Location.X + G.Width) > MovedMiddleX));
                var l = lr.Where(G => G.Location.X < movedElement.Location.X);
                var r = lr.Where(G => G.Location.X > movedElement.Location.X);
                var up = ul.Where(G => G.Location.Y < movedElement.Location.Y);
                var lo = ul.Where(G => G.Location.Y > movedElement.Location.Y);

                var nextElements = new List<FlowBlockUIElement>();

                if (l.Count() > 0) nextElements.Add(l.OrderBy(G => G.Location.X).Last());
                if (r.Count() > 0) nextElements.Add(r.OrderBy(G => G.Location.X).First());
                if (up.Count() > 0) nextElements.Add(up.OrderBy(G => G.Location.Y).Last());
                if (lo.Count() > 0) nextElements.Add(lo.OrderBy(G => G.Location.Y).First());

                var NextElementMiddleXY = nextElements.Select(G => new int[] { G.Location.X + (G.Width / 2), G.Location.Y + (G.Height / 2) });

                foreach (int[] GXY in NextElementMiddleXY)
                {
                    int GridMiddleX = GXY[0];
                    int GridMiddleY = GXY[1];

                    if (!BlockLocationUpdateX)
                    {
                        if ((MovedMiddleX > (GridMiddleX - Tolerance)) &&
                            (MovedMiddleX < (GridMiddleX + Tolerance)))
                        {
                            BlockLocationUpdateX = true;
                            SetLocation = new Point(GridMiddleX - (movedElement.Width / 2), SetLocation.Y);
                        }
                    }

                    if (!BlockLocationUpdateY)
                    {
                        if ((MovedMiddleY > (GridMiddleY - Tolerance)) &&
                            (MovedMiddleY < (GridMiddleY + Tolerance)))
                        {
                            if (!BlockLocationUpdateY)
                            {
                                BlockLocationUpdateY = true;
                                SetLocation = new Point(SetLocation.X, GridMiddleY - (movedElement.Height / 2));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
            }
            
            movedElement.BlockLocationUpdateX = BlockLocationUpdateX;
            movedElement.BlockLocationUpdateY = BlockLocationUpdateY;

            e.Result = new List<object>() { movedElement, SetLocation };
        }

        private void Background_Align_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<object> args = (List<object>)e.Result;
            FlowBlockUIElement movedElement = (FlowBlockUIElement)args[0];
            Point location = (Point)args[1];
            movedElement.Location = location;
        }

        private void DoSelection(Keys Key)
        {
            if (Key == Keys.A)
            {
                foreach (var uiElement in FlowBloxUIRegistry.UIElements)
                {
                    uiElement.MarkElement(ElementState.Marked);
                }
            }
            else
            {
                if (_recentFlowBlock != null)
                {
                    if (Key == Keys.Up)
                    {
                        foreach (var uiElement in FlowBloxUIRegistry.UIElements)
                        {
                            if (uiElement.Location.Y < (_recentFlowBlock.Location.Y + 20))
                            {
                                uiElement.MarkElement(ElementState.Marked);
                            }
                        }
                    }

                    if (Key == Keys.Right)
                    {
                        foreach (var uiElement in FlowBloxUIRegistry.UIElements)
                        {
                            if (uiElement.Location.X > (_recentFlowBlock.Location.X - 20))
                            {
                                uiElement.MarkElement(ElementState.Marked);
                            }
                        }
                    }

                    if (Key == Keys.Left)
                    {
                        foreach (var uiElement in FlowBloxUIRegistry.UIElements)
                        {
                            if (uiElement.Location.X < (_recentFlowBlock.Location.X + 20))
                            {
                                uiElement.MarkElement(ElementState.Marked);
                            }
                        }
                    }

                    if (Key == Keys.Down)
                    {
                        foreach (FlowBlockUIElement uiElement in FlowBloxUIRegistry.UIElements)
                        {
                            if (uiElement.Location.Y > (_recentFlowBlock.Location.Y - 20))
                            {
                                uiElement.MarkElement(ElementState.Marked);
                            }
                        }
                    }
                }
            }

            UpdateUI();
        }

        private void UpdateRecentFlowBlockFromSender(object sender)
        {
            if (sender is Label)
            {
                _recentFlowBlock = (FlowBlockUIElement)((Panel)((Label)sender).Parent).Parent;
            }
            else if (_recentFlowBlock is FlowBlockUIElement)
            {
                _recentFlowBlock = (FlowBlockUIElement)sender;
            }
        }

        private Point GetAbsolutePositionFromRecentFlowBlock(MouseEventArgs e)
        {
            return new Point(e.X + _recentFlowBlock.Location.X, e.Y + _recentFlowBlock.Location.Y);
        }

        private void baseGridElement_MouseDown(object sender, MouseEventArgs e)
        {
            UpdateRecentFlowBlockFromSender(sender);
            if (e.Button == MouseButtons.Left)
            {
                if (_modeHandler is ConnectionModeHandler)
                    _modeHandler.DoMouseDown(e.Button, GetAbsolutePositionFromRecentFlowBlock(e));
            }
            this._blockGridUpdate = false;
        }

        private void baseGridElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_modeHandler is ConnectionModeHandler)
                    _modeHandler.DoMouseMove(e.Button, GetAbsolutePositionFromRecentFlowBlock(e));
            }
        }

        private void baseGridElement_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_modeHandler is ConnectionModeHandler)
                    _modeHandler.DoMouseUp(e.Button);
            }
        }

        private void baseGridElement_MouseClick(object sender, MouseEventArgs e)
        {
            UpdateRecentFlowBlockFromSender(sender);
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                if (!_recentFlowBlock.IsMultiSelect() || !_recentFlowBlock.ElementSelected)
                {
                    foreach (var uiElement in FlowBloxUIRegistry.UIElements)
                    {
                        uiElement.MarkElement(ElementState.Unmarked);
                    }
                }

                _recentFlowBlock.MarkElement(ElementState.Marked);
            }
            else
            {
                if (_recentFlowBlock.ElementSelected)
                {
                    _recentFlowBlock.MarkElement(ElementState.Unmarked);
                }
                else
                {
                    _recentFlowBlock.MarkElement(ElementState.Marked);
                }
            }

            MarkReferences();
            this._blockGridUpdate = false;
            UpdateUI();
        }

        private void MarkReferences()
        {
            var references = FlowBloxUIRegistry.UIElements
                .Where(x => x.ElementSelected)
                .Select(x => x.InternalFlowBlock.IterationContext)
                .Where(x => x != null)
                .Select(x => FlowBloxUIRegistry.GetUIElementToGridElement(x));

            foreach ( var reference in references)
            {
                if (reference == null)
                    continue;

                reference.MarkElement(ElementState.Reference);
            }
        }

        private void baseGridElement_MouseEnter(object sender, EventArgs e) 
        { 

        }

        private HashSet<FlowBloxArrow> _drawnArrows = new HashSet<FlowBloxArrow>();

        private void PrintReferenceLines(Graphics graphics)
        {
            if (FlowBloxProjectManager.Instance.ActiveProject == null)
                return;

            _drawnArrows.Clear();

            var visibleUIElements = FlowBloxUIRegistry.UIElements
                .Where(x => IsGridElementVisible(x))
                .ToHashSet();

            foreach (var uiElement in FlowBloxUIRegistry.UIElements)
            {
                foreach (var nextUIElement in uiElement.GetNextElementList())
                {
                    if (visibleUIElements.Contains(uiElement) ||
                        visibleUIElements.Contains(nextUIElement))
                    {
                        var arrow = new FlowBloxArrow(uiElement, nextUIElement);
                        _drawnArrows.Add(arrow);

                        bool isSelected = _selectedArrows.Contains(arrow);

                        FlowBloxLineUtil.PrintLine(
                            graphics,
                            visibleUIElements,
                            arrow,
                            lineColor: isSelected ? FlowBloxArrowColors.SelectedArrow : FlowBloxArrowColors.InvokeArrow);
                    }
                }

                if (uiElement.InternalFlowBlock.HasInputReference)
                {
                    var inputReference = uiElement.InternalFlowBlock.IterationContext;
                    var uiElementInputReference = FlowBloxUIRegistry.GetUIElementToGridElement(inputReference);

                    if (visibleUIElements.Contains(uiElement) ||
                        visibleUIElements.Contains(uiElementInputReference))
                    {
                        FlowBloxLineUtil.PrintLine(
                            graphics,
                            visibleUIElements,
                            new FlowBloxArrow(uiElement, uiElementInputReference, 10),
                            dashed: true,
                            lineColor: FlowBloxArrowColors.IterationContextArrow);
                    }
                }

                if (uiElement.InternalFlowBlock is RecursiveCallFlowBlock)
                {
                    var recursiveCallFlowBlock = (RecursiveCallFlowBlock)uiElement.InternalFlowBlock;
                    var invocationTargetFlowBlock = recursiveCallFlowBlock.TargetFlowBlock;

                    if (invocationTargetFlowBlock != null)
                    {
                        var invocationTargetUiElement = FlowBloxUIRegistry.GetUIElementToGridElement(invocationTargetFlowBlock);

                        if (visibleUIElements.Contains(uiElement) ||
                            visibleUIElements.Contains(invocationTargetUiElement))
                        {
                            FlowBloxLineUtil.PrintLine(
                                graphics,
                                visibleUIElements,
                                new FlowBloxArrow(uiElement, invocationTargetUiElement),
                                text: $"recursive call",
                                dashed: true,
                                lineColor: FlowBloxArrowColors.RecursiveCallArrow);
                        }
                    }
                }
            }
        }
    
        private void baseGridElement_Move(object sender, EventArgs e)
        {
            if (_latestGridInteraction != GridInteractions.Scroll)
            {
                MoveElement();
            }
        }

        void Element_GridElementMovedByUser(FlowBlockUIElement movedElement)
        {
            AlignElement(movedElement);
        }

        private void baseGridElement_MoveFinished()
        {
            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                if (_latestGridInteraction != GridInteractions.Scroll)
                {
                    TryScheduleMoveFinished();
                }
            }
        }

        private void MoveElement()
        {
            if (!_blockGridUpdate || _latestGridInteraction == GridInteractions.PrintReferenceLines)
            {
                Graphics Graphics = Graphics.FromImage(BufferedPPC);
                PrintBackground(Graphics);
                FlushDoubleBuffer(Graphics);
                _blockGridUpdate = true;
            }

            this._latestGridInteraction = GridInteractions.MoveElement;
        }

        private void ScrollGrid()
        {
            if (!_blockGridUpdate)
            {
                Graphics Graphics = Graphics.FromImage(BufferedPPC);
                PrintBackground(Graphics);
                FlushDoubleBuffer(Graphics);
                _blockGridUpdate = true;
            }

            this._latestGridInteraction = GridInteractions.Scroll;
        }

        private bool IsGridElementVisible(FlowBlockUIElement uiElement)
        {
            if (uiElement == null)
                return false;

            int rightMax = uiElement.Location.X + uiElement.Width;
            int downMax = uiElement.Location.Y + uiElement.Height;

            if ((rightMax >= 0) && (downMax >= 0))
            {
                if ((uiElement.Location.X <= mainPanel.ClientSize.Width) &&
                    (uiElement.Location.Y <= mainPanel.ClientSize.Height))
                {
                    return true;
                }
            }

            return false;
        }

        private void GetExecutionMapF(BaseFlowBlock f_FlowBlock, ref Dictionary<BaseFlowBlock, bool> executionMap)
        {
            foreach (BaseFlowBlock GridElement in f_FlowBlock.GetNextFlowBlocks())
            {
                if (!executionMap.ContainsKey(GridElement))
                {
                    executionMap[GridElement] = true;
                    GetExecutionMapF(GridElement, ref executionMap);
                }
            }
        }

        private void GetExecutionMapR(BaseFlowBlock r_FlowBlock, ref Dictionary<BaseFlowBlock, bool> executionMap)
        {
            foreach (BaseFlowBlock flowBlock in FlowBloxRegistryProvider.GetRegistry().GetPreviousElements(r_FlowBlock))
            {
                if (!executionMap.ContainsKey(flowBlock))
                {
                    executionMap[flowBlock] = true;
                    if (flowBlock.CanGoBack) 
                        GetExecutionMapR(flowBlock, ref executionMap);
                }
            }
        }

        private void UpdateAllElementBorders()
        {
            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                foreach (var uiElement in FlowBloxUIRegistry.UIElements)
                {
                    uiElement.UpdateBackColor();
                }
            }
        }

        private void UpdateExecutionStatus()
        {
            if (FlowBloxProjectManager.Instance.ActiveProject == null)
                return;

            this._notExecutedElements.ForEach(x => x.IsNotExecuted = false);
            this._notExecutedElements.Clear();

            foreach(var notExecutedElement in FlowBloxRegistryProvider.GetRegistry().GetFlowBlocks<BaseFlowBlock>()
                .Where(x => !typeof(StartFlowBlock).IsAssignableFrom(x.GetType()))
                .Where(x => !typeof(NoteFlowBlock).IsAssignableFrom(x.GetType()))
                .Where(x => !x.ReferencedFlowBlocks.Any()))
            {
                notExecutedElement.IsNotExecuted = true;
                _notExecutedElements.Add(notExecutedElement);
            }
        }

        private void PrintBackground(Graphics g)
        {
            using (TextureBrush brush = new TextureBrush(FlowBloxMainUIImages.grid_pattern, WrapMode.Tile))
            {
                g.FillRectangle(brush, mainPanel.ClientRectangle);
            }
        }

        private void PrintGrid(bool inclusiveReferences)
        {
            if ((FlowBloxProjectManager.Instance.ActiveProject != null) && !_blockGridUpdate)
            {
                Graphics graphics = Graphics.FromImage(BufferedPPC);
                PrintBackground(graphics);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                if (_modeHandler?.Print(graphics) == true)
                {
                    this._latestGridInteraction = GridInteractions.HandlerPrintInteraction;
                }
                else if (inclusiveReferences)
                {
                    if (FlowBloxRegistryProvider.GetRegistry().GetFlowBlocks().Count() > 0)
                    {
                        PrintReferenceLines(graphics);
                    }

                    this._latestGridInteraction = GridInteractions.PrintReferenceLines;
                }
                else
                {
                    this._latestGridInteraction = GridInteractions.Print;
                }
                FlushDoubleBuffer(graphics);
            }
        }

        private void FlushDoubleBuffer(Graphics Graphics)
        {
            try
            {
                if (Graphics != null) 
                    Graphics.Dispose();
            }
            catch (Exception e) 
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);
            }

            Graphics = mainPanel.CreateGraphics();
            Graphics.DrawImage(BufferedPPC, 0, 0, mainPanel.ClientRectangle.Width, mainPanel.ClientRectangle.Height);
            Graphics.Dispose();
        }
    }
}


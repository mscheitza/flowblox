using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core;
using FlowBlox.Grid.Elements.UserControls.Renderer;
using System.Drawing.Drawing2D;
using FlowBlox.AppWindow.Contents;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Enums;
using FlowBlox.Grid.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Extensions;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components.Modifier;
using FlowBlox.Actions;
using FlowBlox.UICore.Utilities;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;

namespace FlowBlox.Grid.Elements.UserControls
{
    public partial class FlowBlockUIElement : UserControl
    {
        private readonly Color backColorDefault = Color.DimGray;
        private readonly Color backColorNotExecuted = Color.Red;

        public delegate void PropertyDoubleClickEventHandler(FlowBlockUIElement sender, string propertyName);
        public delegate void ResultFieldDoubleClickEventHandler(FlowBlockUIElement sender, FieldElement fieldElement);
        public delegate void RequiredFieldDoubleClickEventHandler(FlowBlockUIElement sender, FlowBloxComponent component, FieldElement fieldElement);
        public delegate void ConditionDoubleClickEventHandler(FlowBlockUIElement sender, FlowBloxReactiveObject condition);
        public delegate void ModifierDoubleClickEventHandler(FlowBlockUIElement sender, ModifierBase modifier);

        public delegate void GridElementMovedEventHandler(FlowBlockUIElement flowBlockUIElement);
        public delegate void MoveFinishedEventHandler();
        public delegate void WarnEventHandler(BaseRuntime runtime, string message);
        public delegate void ErrorEventHandler(BaseRuntime runtime, string message);
        public delegate void FlagsChangedEventHandler(BaseRuntime runtime, FlowBlockFlags flowBlockFlags);
        public delegate void DoubleClickEventHandler(object sender);
        public delegate void ElementSelectedChangedEventHandler(FlowBlockUIElement sender, bool selected);

        private BaseFlowBlock _flowBlock;

        private Point _moveFromPosition;

        public BaseFlowBlock InternalFlowBlock => _flowBlock;

        public event GridElementMovedEventHandler GridElementMovedByUser;
        public event MoveFinishedEventHandler MoveFinished;
        public new event DoubleClickEventHandler DoubleClick;

        public event PropertyDoubleClickEventHandler PropertyDoubleClick;
        public event RequiredFieldDoubleClickEventHandler RequiredFieldDoubleClick;
        public event ResultFieldDoubleClickEventHandler ResultFieldDoubleClick;
        public event ConditionDoubleClickEventHandler ConditionDoubleClick;
        public event ModifierDoubleClickEventHandler ModifierDoubleClick;
        public event ElementSelectedChangedEventHandler ElementSelectedChangedByUser;

        public void RaisePropertyDoubleClick(string propertyName) => PropertyDoubleClick?.Invoke(this, propertyName);
        public void RaiseResultFieldDoubleClick(FieldElement fieldElement) => ResultFieldDoubleClick?.Invoke(this, fieldElement);
        public void RaiseRequiredFieldDoubleClick(FlowBloxComponent target, FieldElement fieldElement) => RequiredFieldDoubleClick?.Invoke(this, target, fieldElement);
        public void RaiseConditionDoubleClick(FlowBloxReactiveObject condition) => ConditionDoubleClick?.Invoke(this, condition);
        public void RaiseModifierDoubleClick(ModifierBase modifier) => ModifierDoubleClick?.Invoke(this, modifier);


        private BorderStyle baseBorderStyle;
        private bool isMouseCaptured;

        private Point recentMouseLocation;
        private Point recentLocation;

        private string errorMessage;
        private string warningMessage;
        private FlowBlockFlags currentFlags;

        private bool _elementSelected;
        public bool ElementSelected
        {
            get => _elementSelected;
            set
            {
                if (_elementSelected == value) 
                    return;
                
                _elementSelected = value;

                ElementSelectedChangedByUser?.Invoke(
                    this,
                    value
                );
            }
        }

        public DateTime ElementSelectedAt { get; set; }

        public bool BreakPoint
        {
            get => InternalFlowBlock.BreakPoint;
            set => InternalFlowBlock.BreakPoint = value;
        }

        public bool HasOverriddenNotifications
        {
            get
            {
                return InternalFlowBlock.OverriddenNotificationEntries
                    .SelectMany(x => x.Overrides)
                    .Any();
            }
        }

        public bool CanSelect { get; set; }
        public bool BlockMoveEvent { get; set; }
        public bool BlockLocationUpdateX { get; set; }
        public bool BlockLocationUpdateY { get; set; }

        public AppWindow.AppWindow ApplicationWindowRef { get; set; }
        public Point BufferedLocation { get; set; }
        public Point LatestDifference { get; set; }
        public string ActionId { get; set; }

        private FlowBloxProjectComponentProvider _componentProvider;

        /// <summary>
        /// Initialisiert ein neues Grid-Element.
        /// </summary>
        public FlowBlockUIElement() : base()
        {
            EnableDoubleBuffer();
            InitializeComponent();

            MarkElement(ElementState.Unmarked);

            base.BorderStyle = BorderStyle.None;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.ElementSelectedAt = DateTime.MinValue;

            _componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();
        }

        /// <summary>
        /// Initialisiert ein neues Grid-Element mit einem initialisierten <see cref="BaseFlowBlock"/>.
        /// </summary>
        public FlowBlockUIElement(BaseFlowBlock baseGridElement) : this()
        {
            this._flowBlock = baseGridElement;
            this._flowBlock.OnUndoWarn += _baseGridElement_OnUndoWarn;
            this._flowBlock.OnWarn += _baseGridElement_OnWarn;
            this._flowBlock.OnUndoError += _baseGridElement_OnUndoError;
            this._flowBlock.OnError += _baseGridElement_OnError;
            this._flowBlock.OnFlagsChanged += _flowBlock_OnFlagsChanged;
            this._flowBlock.OnPropertyValuesChanged += _baseGridElement_OnPropertyValuesChanged;

            this.Name = _flowBlock.Name;
            this.Location = _flowBlock.Location;
            this.pbHeaderLeft1.Image = SkiaToSystemDrawingHelper.ToSystemDrawingImage(_flowBlock.Icon16);
            this.labelHeader.Text = FlowBloxComponentHelper.GetDisplayName(_flowBlock);

            this.Move += GridUIElement_Move;

            this.UpdateContent();
        }

        private void GridUIElement_Move(object sender, EventArgs e)
        {
            if (this.Parent == null)
                return;

            Point location = new Point(
                this.Location.X - ((Panel)this.Parent).AutoScrollPosition.X,
                this.Location.Y - ((Panel)this.Parent).AutoScrollPosition.Y
            );

            this._flowBlock.Location = location;
        }

        private void _baseGridElement_OnPropertyValuesChanged()
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                this.Invoke(new Action(_baseGridElement_OnPropertyValuesChanged));
                return;
            }

            this.UpdateContent(true);
        }

        private void EnableDoubleBuffer()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ContainerControl, false);
            this.SetStyle(ControlStyles.Opaque, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        /// <summary>
        /// Setzt den Titel des aktuellen Grid-Elements. Der Titel ist sowohl in der Grid-Element Ansicht zu sehen, als auch in der Kontrollelement-Ansicht.
        /// </summary>
        /// <param name="title"></param>
        protected void SetTitle(string title)
        {
            this.labelHeader.Text = title;
        }

        public Label GetHeader() => labelHeader;

        private void UpdateCoreFlags()
        {
            if (_flowBlock is StartFlowBlock)
                labelHeader.Image = imageList_BaseFlowBlock.Images["start"];
            else
                labelHeader.Image = null;

            if (_flowBlock.ElementIndex >= 0)
                labelIndexInfo.Text = "Execute at position " + _flowBlock.ElementIndex.ToString();

            flpIndexInfo.Visible = (_flowBlock.ElementIndex >= 0);

            var warningOccured = !string.IsNullOrEmpty(warningMessage);
            var errorOccured = !string.IsNullOrEmpty(errorMessage);
            if (warningOccured)
            {
                labelNotification.Text = warningMessage;
                flpNotification.BackColor = Color.Khaki;
                flpNotification.Visible = true;
                pbNotification.Image = imageList_BaseFlowBlock.Images["warning"];

            }
            else if (errorOccured)
            {
                labelNotification.Text = errorMessage;
                flpNotification.BackColor = Color.FromKnownColor(KnownColor.DarkSalmon);
                flpNotification.Visible = true;
                pbNotification.Image = imageList_BaseFlowBlock.Images["error"];
            }
            else
            {
                labelNotification.Text = string.Empty;
                flpNotification.BackColor = Color.Transparent;
                flpNotification.Visible = false;
            }

            if (HasOverriddenNotifications)
            {
                pbHeaderRight1.Image = imageList_BaseFlowBlock.Images["suppressNotifications"];
                pbHeaderRight1.Visible = true;
            }
            else
            {
                pbHeaderRight1.Image = null;
                pbHeaderRight1.Visible = false;
            }

            pbHeaderRight2.Visible = BreakPoint;
            pbHeaderRight2.Image = BreakPoint ? imageList_BaseFlowBlock.Images["breakpoint"] : null;
        }

        private void UpdateFlowBlockFlags()
        {
            pbHeaderLeft2.Image = null;
            pbHeaderLeft3.Image = null;

            PictureBox[] headerPictureBoxes = { pbHeaderLeft2, pbHeaderLeft3 };
            int index = 0;
            foreach (FlowBlockFlags flag in Enum.GetValues(typeof(FlowBlockFlags)))
            {
                if (flag != FlowBlockFlags.None && currentFlags.HasFlag(flag))
                {
                    if (index < headerPictureBoxes.Length)
                    {
                        string resourceName = $"{Enum.GetName(typeof(FlowBlockFlags), flag)}_16";
                        headerPictureBoxes[index].Image = (Bitmap)FlowBloxMainUIImages.ResourceManager.GetObject(resourceName);
                        index++;
                    }
                }
            }
        }

        public void UpdateFlags()
        {
            UpdateCoreFlags();
            UpdateFlowBlockFlags();
        }

        static readonly Dictionary<ElementState, Color> StateToColorMapping = new Dictionary<ElementState, Color>
        {
            { ElementState.Marked, Color.DarkBlue },
            { ElementState.Unmarked, Color.DarkGray },
            { ElementState.Reference, Color.RebeccaPurple }
        };

        public void MarkElement(ElementState state)
        {
            Color colorToUse = StateToColorMapping[state];

            pbHeaderLeft1.BackColor = colorToUse;
            pbHeaderLeft2.BackColor = colorToUse;
            pbHeaderLeft3.BackColor = colorToUse;
            pbHeaderRight1.BackColor = colorToUse;
            pbHeaderRight2.BackColor = colorToUse;

            if ((ApplicationWindowRef != null) && !ApplicationWindowRef.IsRuntimeActive)
            {
                labelHeader.BackColor = colorToUse;
                ElementSelected = (state == ElementState.Marked);
                ElementSelectedAt = DateTime.Now;
            }
        }

        public void MarkElementRuntime(bool mark)
        {
            if (!mark && !(labelHeader.BackColor == Color.DarkGray))
            {
                labelHeader.BackColor = Color.DarkGray;
            }
            else if (mark && !(labelHeader.BackColor == Color.DarkGoldenrod))
            {
                labelHeader.BackColor = Color.DarkGoldenrod;
            }
        }

        private const int MaxHeight = 300;
        private const int NotificationHeight = 25;
        private const int RenderedEntryHeight = 25;

        public virtual void RefreshSize()
        {
            this.Height = 30;

            if (_renderer != null)
            {
                for (int i = 0; i < _renderer.RenderedEntries; i++)
                {
                    this.Height += RenderedEntryHeight;
                }
            }

            if (_flowBlock.ElementIndex >= 0)
                this.Height += NotificationHeight;

            if (!string.IsNullOrEmpty(warningMessage))
                this.Height += NotificationHeight;

            if (this.Height > MaxHeight)
                this.Height = MaxHeight;

            UpdateBackColor();
        }

        private FlowBlockUIElementRenderer _renderer;
        public virtual void UpdateContent(bool keepAnchor = false)
        {
            _renderer = new FlowBlockUIElementRenderer(this);
            var innerPanel = _renderer.Render();
            if (_renderer.RenderedEntries > 0)
            {
                panelCenter.Clear();
                panelCenter.Controls.Add(innerPanel);
            }
            else
            {
                panelCenter.Clear();
                panelCenter.Controls.Add(new PictureBox
                {
                    Image = FlowBloxMainUIImages.no_content_72,
                    Size = new Size(72, 72),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Dock = DockStyle.Fill
                });
            }
            FlowBloxStyle.ApplyStyle(this);

            innerPanel.Height = _renderer.RenderedEntries * RenderedEntryHeight;

            UpdateFlags();
            RefreshSize(keepAnchor);
        }

        public void RefreshSize(bool keepAnchor = false)
        {
            var currentHeight = this.Size.Height;
            RefreshSize();
            var newHeight = this.Size.Height;

            if (keepAnchor && currentHeight != newHeight)
            {
                var heightDifference = newHeight - currentHeight;
                var adjustment = heightDifference / 2;
                this.Location = new Point(this.Location.X, this.Location.Y - adjustment);
            }
        }

        public new BorderStyle BorderStyle
        {
            get { return baseBorderStyle; }

            set
            {
                baseBorderStyle = value;

                Invalidate();
            }
        }

        public void UpdateBackColor() => UpdateBackColor(_flowBlock.IsNotExecuted ? backColorNotExecuted : backColorDefault);

        public void UpdateBackColor(Color color)
        {
            if (color.ToArgb() != BackColor.ToArgb())
                this.BackColor = color;
        }

        private GraphicsPath path;
        private Region region;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            int cornerRadius = 10;
            int rectWidth = this.Width - 1;
            int rectHeight = this.Height - 1;

            path?.Dispose(); // Dispose the old path if it exists
            path = new GraphicsPath();
            path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(rectWidth - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(rectWidth - cornerRadius, rectHeight - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(0, rectHeight - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            region?.Dispose(); // Dispose the old region if it exists
            region = new Region(path);

            this.Region = region;

            this.Invalidate(); // Causes a repaint
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (path != null)
                e.Graphics.DrawPath(System.Drawing.Pens.Black, path);

            base.OnPaint(e);
        }

        private Point UpdateLocation(Point mouseLocation, Point? latestDifference = null)
        {
            Point difference;

            if (!latestDifference.HasValue)
            {
                difference = mouseLocation;

                difference.X -= recentMouseLocation.X;
                difference.Y -= recentMouseLocation.Y;
            }
            else
            {
                difference = latestDifference.Value;
            }

            Point newLocation = recentLocation;

            newLocation.X += difference.X;
            newLocation.Y += difference.Y;

            this.BufferedLocation = newLocation;
            this.LatestDifference = difference;

            return newLocation;
        }

        public void FlushLocationX() { this.Location = new Point(BufferedLocation.X, Location.Y); }
        public void FlushLocationY() { this.Location = new Point(Location.X, BufferedLocation.Y); }

        public bool IsMultiSelect()
        {
            int count = 0;
            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                foreach (var gridUIElement in _componentProvider.GetCurrentUIRegistry().UIElements)
                {
                    if (gridUIElement.ElementSelected)
                        count++;
                }
            }
            return count > 1;
        }

        private void BaseFlowBlock_MouseUp(object sender, MouseEventArgs e) => BaseFlowBlock_MouseUp(sender, e, null);

        private void BaseFlowBlock_MouseUp(object sender, MouseEventArgs e, FlowBloxMoveAction parentMoveAction)
        {
            if (!CanSelect)
                return;

            FlowBloxMoveAction moveAction = null;

            if (e.Button == MouseButtons.Left)
            {
                if (!BlockMoveEvent)
                {
                    if (!ApplicationWindowRef.IsRuntimeActive)
                    {
                        isMouseCaptured = false;
                        Point MouseLocation = Cursor.Position;
                        UpdateLocation(MouseLocation);

                        if (!BlockLocationUpdateX)
                            FlushLocationX();

                        if (!BlockLocationUpdateY)
                            FlushLocationY();

                        if (ApplicationWindowRef != null && _moveFromPosition != this.Location)
                        {
                            moveAction = new FlowBloxMoveAction()
                            {
                                UIElement = this,
                                CapturedAutoScrollPosition = ((Panel)this.Parent).AutoScrollPosition,
                                From = _moveFromPosition,
                                To = this.Location
                            };

                            if (parentMoveAction == null)
                                _componentProvider.GetCurrentChangelist().AddChange(moveAction);
                            else
                                parentMoveAction.AssociatedActions.Add(moveAction);
                        }
                        MoveFinished?.Invoke();
                    }
                }

                this.BlockMoveEvent = false;
            }

            if (sender is not FlowBlockUIElement)
            {
                if (IsMultiSelect())
                {
                    foreach (var uiElement in _componentProvider.GetCurrentUIRegistry().UIElements.Where(x => x != sender))
                    {
                        if (uiElement.ElementSelected && (uiElement != this))
                            uiElement.BaseFlowBlock_MouseUp(this, e, moveAction);
                    }
                }
            }
        }

        public static string GenerateActionId() => "A" + DateTime.Now.ToFileTimeUtc().ToString();

        private void BaseFlowBlock_MouseDown(object sender, MouseEventArgs e)
        {
            if (!CanSelect)
                return;

            if (!(sender is FlowBlockUIElement))
            {
                this.ActionId = GenerateActionId();

                if (((ModifierKeys & Keys.Control) == Keys.Control) || IsMultiSelect())
                {
                    foreach (var uiElement in _componentProvider.GetCurrentUIRegistry().UIElements.Where(x => x != sender))
                    {
                        if (uiElement.ElementSelected && (uiElement != this))
                        {
                            uiElement.BaseFlowBlock_MouseDown(this, e);
                        }
                    }
                }
            }
            else
            {
                this.ActionId = ((FlowBlockUIElement)sender).ActionId;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (!ApplicationWindowRef.IsRuntimeActive)
                {
                    var projectPanel = AppWindow.AppWindow.Instance.GetAccessibleComponent<ProjectPanel>();
                    projectPanel.DisableGridUpdate();

                    if (Enabled && Visible)
                    {
                        this.recentMouseLocation = Cursor.Position;
                        this.recentLocation = this.BufferedLocation = this.Location;
                        this.isMouseCaptured = true;
                        this.BlockLocationUpdateX = false;
                        this.BlockLocationUpdateY = false;
                        this._moveFromPosition = this.Location;
                    }
                }
            }
        }

        private void BaseFlowBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (!CanSelect)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (!ApplicationWindowRef.IsRuntimeActive)
                {
                    if (isMouseCaptured)
                    {
                        Point MouseLocation = Cursor.Position;
                        this.recentLocation = UpdateLocation(MouseLocation, (sender as FlowBlockUIElement)?.LatestDifference);
                        this.recentMouseLocation = MouseLocation;
                        if (!BlockLocationUpdateX) this.FlushLocationX();
                        if (!BlockLocationUpdateY) this.FlushLocationY();
                        GridElementMovedByUser?.Invoke(this);
                    }
                }
            }

            if (sender is not FlowBlockUIElement)
            {
                if (IsMultiSelect())
                {
                    foreach (var uiElement in _componentProvider.GetCurrentUIRegistry().UIElements.Where(x => x != sender))
                    {
                        if (uiElement.ElementSelected && (uiElement != this))
                            uiElement.BaseFlowBlock_MouseMove(this, e);
                    }
                }
            }
        }

        private void BaseFlowBlock_MouseHover(object sender, EventArgs e) { UpdateBackColor(Color.CornflowerBlue); }
        private void BaseFlowBlock_MouseLeave(object sender, EventArgs e)
        {
            UpdateBackColor(this.InternalFlowBlock.IsNotExecuted ? backColorNotExecuted : backColorDefault);
            var projectPanel = AppWindow.AppWindow.Instance.GetAccessibleComponent<ProjectPanel>();
            projectPanel.Controls[0].Focus();
        }
        private void BaseFlowBlock_MouseDoubleClick(object sender, MouseEventArgs e) { DoubleClick?.Invoke(this); }

        private void Warn(BaseRuntime runtime, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new WarnEventHandler(Warn), new object[] { runtime, message });
                return;
            }

            var recent = this.warningMessage;
            this.warningMessage = message;
            if (recent?.Equals(message) == false)
            {
                UpdateCoreFlags();
                RefreshSize();
            }
        }

        private void Error(BaseRuntime runtime, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ErrorEventHandler(Error), new object[] { runtime, message });
                return;
            }

            var recent = this.errorMessage;
            this.errorMessage = message;
            if (recent?.Equals(message) == false)
            {
                UpdateCoreFlags();
                RefreshSize();
            }
        }

        private void FlagsChanged(BaseRuntime runtime, FlowBlockFlags flowBlockFlags)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FlagsChangedEventHandler(FlagsChanged), new object[] { runtime, flowBlockFlags });
                return;
            }

            if (this.currentFlags != flowBlockFlags)
            {
                this.currentFlags = flowBlockFlags;
                this.UpdateFlowBlockFlags();
            }
        }

        private void UndoWarn()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(UndoWarn));
                return;
            }

            string recentWarning = this.warningMessage;
            this.warningMessage = "";
            if (!string.IsNullOrEmpty(recentWarning))
            {
                UpdateCoreFlags();
                RefreshSize();
            }
        }

        private void _baseGridElement_OnUndoWarn(BaseRuntime runtime) => this.Warn(runtime, "");

        private void _baseGridElement_OnWarn(BaseRuntime runtime, string message) => this.Warn(runtime, message);

        private void _baseGridElement_OnError(BaseRuntime runtime, string message) => this.Error(runtime, message);

        private void _baseGridElement_OnUndoError(BaseRuntime runtime) => this.Error(runtime, "");

        private void _flowBlock_OnFlagsChanged(BaseRuntime runtime, FlowBlockFlags flowBlockFlags) => this.FlagsChanged(runtime, flowBlockFlags);

        public IEnumerable<FlowBlockUIElement> GetNextElementList()
        {
            return _flowBlock.GetNextFlowBlocks()
                .Select(x => _componentProvider.GetCurrentUIRegistry().GetUIElementToGridElement(x))
                .ExceptNull()
                .ToList();
        }

        public Size? AnchorSize { get; set; }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.Core.Components
{
    public class FlowBloxScrollablePanel : Panel
    {
        private readonly Color scrollBarBase = Color.FromArgb(44, 44, 44);
        private readonly Color scrollBarHover = Color.FromArgb(70, 70, 70);

        private readonly int scrollbarStrength = 9;

        private PictureBox scrollBarRight;
        private PictureBox scrollBarLeft;
        private PictureBox scrollBarTop;
        private PictureBox scrollBarBottom;

        private int scrollStep = 20;
        private int contentWidth = 0;
        private int contentHeight = 0;

        private int currentScrollOffsetX = 0;
        private int currentScrollOffsetY = 0;

        private Timer scrollTimer;
        private int scrollDirectionX = 0; // -1 left, 1 right
        private int scrollDirectionY = 0; // -1 above, 1 below

        public int ScrollStep
        {
            get => scrollStep;
            set => scrollStep = Math.Max(1, value);
        }

        public FlowBloxScrollablePanel()
        {
            this.AutoScroll = false;
            this.DoubleBuffered = true;

            scrollBarRight = new PictureBox
            {
                Width = scrollbarStrength,
                Dock = DockStyle.Right,
                BackColor = scrollBarBase,
                Visible = false,
                Image = FlowBloxMainUIImages.arrow_right_16,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand
            };
            this.Controls.Add(scrollBarRight);

            scrollBarLeft = new PictureBox
            {
                Width = scrollbarStrength,
                Dock = DockStyle.Left,
                BackColor = scrollBarBase,
                Visible = false,
                Image = FlowBloxMainUIImages.arrow_left_16,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand
            };
            this.Controls.Add(scrollBarLeft);

            scrollBarTop = new PictureBox
            {
                Height = scrollbarStrength,
                Dock = DockStyle.Top,
                BackColor = scrollBarBase,
                Visible = false,
                Image = FlowBloxMainUIImages.arrow_up_16,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand
            };
            this.Controls.Add(scrollBarTop);

            scrollBarBottom = new PictureBox
            {
                Height = scrollbarStrength,
                Dock = DockStyle.Bottom,
                BackColor = scrollBarBase,
                Visible = false,
                Image = FlowBloxMainUIImages.arrow_down_16,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand
            };
            this.Controls.Add(scrollBarBottom);

            // Timer
            scrollTimer = new Timer { Interval = 50 };
            scrollTimer.Tick += ScrollTimer_Tick;

            // Events – Horizontal
            scrollBarRight.MouseDown += (s, e) => { scrollDirectionX = 1; ScrollContent(scrollStep, 0); scrollTimer.Start(); };
            scrollBarLeft.MouseDown += (s, e) => { scrollDirectionX = -1; ScrollContent(-scrollStep, 0); scrollTimer.Start(); };

            // Events – Vertical
            scrollBarBottom.MouseDown += (s, e) => { scrollDirectionY = 1; ScrollContent(0, scrollStep); scrollTimer.Start(); };
            scrollBarTop.MouseDown += (s, e) => { scrollDirectionY = -1; ScrollContent(0, -scrollStep); scrollTimer.Start(); };

            // Shared Hover/Up
            scrollBarRight.MouseUp += ScrollBar_MouseUp;
            scrollBarLeft.MouseUp += ScrollBar_MouseUp;
            scrollBarTop.MouseUp += ScrollBar_MouseUp;
            scrollBarBottom.MouseUp += ScrollBar_MouseUp;

            scrollBarRight.MouseEnter += ScrollBar_MouseEnter;
            scrollBarLeft.MouseEnter += ScrollBar_MouseEnter;
            scrollBarTop.MouseEnter += ScrollBar_MouseEnter;
            scrollBarBottom.MouseEnter += ScrollBar_MouseEnter;

            scrollBarRight.MouseLeave += ScrollBar_MouseLeave;
            scrollBarLeft.MouseLeave += ScrollBar_MouseLeave;
            scrollBarTop.MouseLeave += ScrollBar_MouseLeave;
            scrollBarBottom.MouseLeave += ScrollBar_MouseLeave;

            this.Resize += FlowBloxScrollablePanel_Resize;
            this.MouseWheel += FlowBloxScrollablePanel_MouseWheel;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (scrollTimer != null)
                {
                    scrollTimer.Stop();
                    scrollTimer.Tick -= ScrollTimer_Tick;
                    scrollTimer.Dispose();
                    scrollTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            int dx = scrollStep * scrollDirectionX;
            int dy = scrollStep * scrollDirectionY;
            if (dx != 0 || dy != 0)
                ScrollContent(dx, dy);
        }

        private void ScrollBar_MouseUp(object sender, MouseEventArgs e)
        {
            scrollDirectionX = 0;
            scrollDirectionY = 0;
            scrollTimer.Stop();
        }

        private void ScrollBar_MouseEnter(object sender, EventArgs e)
        {
            if (sender is PictureBox pb)
                pb.BackColor = scrollBarHover;
        }

        private void ScrollBar_MouseLeave(object sender, EventArgs e)
        {
            if (sender is PictureBox pb)
                pb.BackColor = scrollBarBase;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (!IsScrollButton(e.Control))
                UpdateContentSize();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            if (!IsScrollButton(e.Control))
                UpdateContentSize();
        }

        public void Clear()
        {
            for (int i = this.Controls.Count - 1; i >= 0; i--)
            {
                var control = this.Controls[i];
                if (!IsScrollButton(control))
                {
                    this.Controls.Remove(control);
                    control.Dispose();
                }
            }
            currentScrollOffsetX = 0;
            currentScrollOffsetY = 0;
            UpdateContentSize();
        }

        private void FlowBloxScrollablePanel_MouseWheel(object sender, MouseEventArgs e)
        {
            bool horizontal = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
            {
                if (horizontal)
                    ScrollContent(e.Delta > 0 ? -scrollStep : scrollStep, 0);
                else
                    ScrollContent(0, e.Delta > 0 ? -scrollStep : scrollStep);

                if (e is HandledMouseEventArgs h)
                    h.Handled = true;
            }
            else
            {
                if (e is HandledMouseEventArgs h)
                    h.Handled = false;
            }
        }

        private void FlowBloxScrollablePanel_Resize(object sender, EventArgs e)
        {
            UpdateScrollBarsVisibility();
        }

        private void ScrollContent(int dx, int dy)
        {
            int maxX = Math.Max(0, contentWidth - this.ClientSize.Width);
            int maxY = Math.Max(0, contentHeight - this.ClientSize.Height);

            int newX = Math.Max(0, Math.Min(currentScrollOffsetX + dx, maxX));
            int newY = Math.Max(0, Math.Min(currentScrollOffsetY + dy, maxY));

            int deltaX = newX - currentScrollOffsetX;
            int deltaY = newY - currentScrollOffsetY;

            if (deltaX == 0 && deltaY == 0)
            {
                UpdateScrollBarsVisibility();
                return;
            }

            currentScrollOffsetX = newX;
            currentScrollOffsetY = newY;

            foreach (Control ctrl in this.Controls)
            {
                if (!IsScrollButton(ctrl))
                {
                    ctrl.Left -= deltaX;
                    ctrl.Top -= deltaY;
                }
            }

            UpdateScrollBarsVisibility();
        }

        private void UpdateScrollBarsVisibility()
        {
            int maxX = Math.Max(0, contentWidth - this.ClientSize.Width);
            int maxY = Math.Max(0, contentHeight - this.ClientSize.Height);

            // Horizontal
            scrollBarRight.Visible = contentWidth > this.ClientSize.Width && currentScrollOffsetX < maxX;
            scrollBarLeft.Visible = currentScrollOffsetX > 0;

            // Vertical
            scrollBarBottom.Visible = contentHeight > this.ClientSize.Height && currentScrollOffsetY < maxY;
            scrollBarTop.Visible = currentScrollOffsetY > 0;
        }

        private void UpdateContentSize()
        {
            int maxRight = 0;
            int maxBottom = 0;

            foreach (Control ctrl in this.Controls)
            {
                if (!IsScrollButton(ctrl))
                {
                    if (ctrl.Right > maxRight) maxRight = ctrl.Right;
                    if (ctrl.Bottom > maxBottom) maxBottom = ctrl.Bottom;
                }
            }

            contentWidth = maxRight;
            contentHeight = maxBottom;

            // If content has become smaller, correct offsets
            int maxX = Math.Max(0, contentWidth - this.ClientSize.Width);
            int maxY = Math.Max(0, contentHeight - this.ClientSize.Height);

            if (currentScrollOffsetX > maxX || currentScrollOffsetY > maxY)
            {
                // "Jump back" to valid area
                ScrollContent(Math.Min(0, maxX - currentScrollOffsetX), Math.Min(0, maxY - currentScrollOffsetY));
            }
            else
            {
                UpdateScrollBarsVisibility();
            }
        }

        private bool IsScrollButton(Control c)
            => c == scrollBarRight || c == scrollBarLeft || c == scrollBarTop || c == scrollBarBottom;
    }
}

using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.Core.Components
{
    public class FlowBloxTextBox : Panel
    {
        private TextBox textBox;
        private Label stripeBorder;
        private bool resizing = false;
        private Point lastPoint;
        private Panel textBoxContainer;
        private int _textBoxClickIndex;
        private Timer resizeTimer;
        private bool needsUpdate = false;

        public FlowBloxTextBox()
        {
            InitializeComponents();
            InitializeResizeTimer();
        }

        public TextBox InnerTextBox => textBox;

        public override string Text
        {
            get => textBox.Text;
            set => textBox.Text = value;
        }

        public bool ReadOnly
        {
            get => textBox.ReadOnly;
            set => textBox.ReadOnly = value;
        }

        public bool Multiline
        {
            get => textBox.Multiline;
            set => textBox.Multiline = value;
        }

        public bool ShowSizingGrip
        {
            get => stripeBorder.Visible;
            set => stripeBorder.Visible = value;
        }

        private void UpdateTextBoxContainerBackground()
        {
            textBoxContainer.BackColor = textBox.BackColor;
        }

        private void InitializeComponents()
        {
            textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };

            ControlHelper.EnableDoubleBuffer(textBox);
            ControlHelper.EnableOptimizedDoubleBuffer(textBox);

            textBox.MouseDoubleClick += TextBox_DoubleClick;
            textBox.Click += TextBox_Click;

            // Container Panel for TextBox with Padding
            textBoxContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(3, 3, 3, 3),
                BackColor = Color.White,
                Tag = FlowBloxStyleTags.StyleIgnoreSelf
            };
            ControlHelper.EnableDoubleBuffer(textBoxContainer);
            ControlHelper.EnableOptimizedDoubleBuffer(textBoxContainer);

            textBoxContainer.Controls.Add(textBox);

            stripeBorder = new Label
            {
                Height = textBox.Height,
                Width = 3,
                Dock = DockStyle.Right,
                Cursor = Cursors.SizeNWSE
            };
            stripeBorder.Paint += StripeBorder_Paint;

            this.Controls.Add(textBoxContainer);
            this.Controls.Add(stripeBorder);

            stripeBorder.MouseDown += StripeBorder_MouseDown;
            stripeBorder.MouseMove += StripeBorder_MouseMove;
            stripeBorder.MouseUp += StripeBorder_MouseUp;

            textBox.BackColorChanged += TextBox_BackColorChanged;

            this.Height = 24;
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        private void TextBox_BackColorChanged(object sender, EventArgs e)
        {
            UpdateTextBoxContainerBackground();
        }

        private void InitializeResizeTimer()
        {
            resizeTimer = new Timer
            {
                Interval = 250
            };
            resizeTimer.Tick += ResizeTimer_Tick;
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            if (needsUpdate)
            {
                this.textBoxContainer.ResumeLayout();
                this.textBoxContainer.PerformLayout();
                this.textBoxContainer.SuspendLayout();
                needsUpdate = false;
            }
        }

        private void ScalableTextboxWithStripedBorder_Shown(object sender, EventArgs e)
        {
            stripeBorder.Height = textBox.Height;
        }

        private void StripeBorder_Paint(object sender, PaintEventArgs e)
        {
            var label = sender as Label;
            using (var brush = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.Gray, Color.Transparent))
            {
                e.Graphics.FillRectangle(brush, label.ClientRectangle);
            }
        }

        private void StripeBorder_MouseDown(object sender, MouseEventArgs e)
        {
            resizing = true;
            lastPoint = this.PointToClient(Cursor.Position);
            this.textBoxContainer.SuspendLayout();
            resizeTimer.Start();
        }

        private void StripeBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (resizing)
            {
                var currentPoint = this.PointToClient(Cursor.Position);
                int widthChange = currentPoint.X - lastPoint.X;
                int heightChange = currentPoint.Y - lastPoint.Y;

                this.Width += widthChange;

                if (textBox.Multiline)
                {
                    this.Height += heightChange;
                }

                lastPoint = currentPoint;
                needsUpdate = true;
            }
        }

        private void StripeBorder_MouseUp(object sender, MouseEventArgs e)
        {
            resizing = false;
            this.textBoxContainer.ResumeLayout();
            resizeTimer.Stop();
        }

        public void EnableDeveloperMode()
        {
            this.textBox.Font = new Font("JetBrains Mono", 8f, FontStyle.Regular);
            this.textBox.Tag = FlowBloxStyleTags.StyleKeepFont;
        }

        private void TextBox_Click(object sender, EventArgs e)
        {
            _textBoxClickIndex = textBox.SelectionStart;
        }

        private void TextBox_DoubleClick(object sender, MouseEventArgs e)
        {
            int start = _textBoxClickIndex;
            while (start > 0 && char.IsLetterOrDigit(textBox.Text[start - 1]))
            {
                start--;
            }

            int end = _textBoxClickIndex;
            while (end < textBox.Text.Length && char.IsLetterOrDigit(textBox.Text[end]))
            {
                end++;
            }
            textBox.Select(start, end - start);
        }
    }
}

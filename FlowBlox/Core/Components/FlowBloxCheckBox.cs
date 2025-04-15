using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.Core.Components
{
    public class FlowBloxCheckBox : Control
    {
        private bool _checked;
        private bool _enabled = true;
        private Label _label;

        private static readonly Image _checkedImage = FlowBloxMainUIImages.checkbox_checked_24;
        private static readonly Image _uncheckedImage = FlowBloxMainUIImages.checkbox_unchecked_24;
        private static readonly Image _inactiveImage = FlowBloxMainUIImages.checkbox_disabled_24;

        public event EventHandler CheckedChanged;

        public override string Text
        {
            get { return _label.Text; }
            set { _label.Text = value; AdjustSize(); }
        }

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    Invalidate();
                    OnCheckedChanged(EventArgs.Empty);
                }
            }
        }

        public new bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                _label.Enabled = value;
                Invalidate();
            }
        }

        public FlowBloxCheckBox()
        {
            this.Size = new Size(24, 24);
            _label = new Label
            {
                Location = new Point(28, 0),
                AutoSize = true
            };

            this.Controls.Add(_label);

            this.MouseDown += (s, e) =>
            {
                if (Enabled)
                {
                    Checked = !Checked; 
                }
            };

            this.MouseEnter += (s, e) => Cursor = Cursors.Hand; 
            this.MouseLeave += (s, e) => Cursor = Cursors.Default; 
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e); 
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Image drawImage = null;
            if (!Enabled)
                drawImage = _inactiveImage;
            else
                drawImage = Checked ? _checkedImage : _uncheckedImage;

            if (drawImage != null)
                e.Graphics.DrawImage(drawImage, new Point(0, 0));
        }

        private void AdjustSize()
        {
            Size textSize = TextRenderer.MeasureText(_label.Text, _label.Font);
            _label.Size = textSize;
            this.Size = new Size(24 + textSize.Width + 4, Math.Max(24, textSize.Height));
            _label.Location = new Point(28, (this.Height - _label.Height) / 2);
        }
    }
}

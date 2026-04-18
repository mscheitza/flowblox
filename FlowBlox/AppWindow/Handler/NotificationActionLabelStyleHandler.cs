using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.AppWindow.Handler
{
    internal sealed class NotificationActionLabelStyleHandler : IDisposable
    {
        private readonly ToolStripStatusLabel _label;
        private readonly StatusStrip _statusStrip;
        private readonly Color _defaultColor;
        private readonly Color _hoverColor;
        private readonly FontStyle _baseFontStyle;
        private readonly FontStyle _hoverFontStyle;

        private Font _baseFont;
        private Font _hoverFont;
        private bool _isRegistered;
        private bool _isDisposed;

        public NotificationActionLabelStyleHandler(
            ToolStripStatusLabel label,
            StatusStrip statusStrip,
            Color? defaultColor = null,
            Color? hoverColor = null,
            FontStyle baseFontStyle = FontStyle.Bold,
            FontStyle hoverFontStyle = FontStyle.Bold | FontStyle.Underline)
        {
            _label = label ?? throw new ArgumentNullException(nameof(label));
            _statusStrip = statusStrip ?? throw new ArgumentNullException(nameof(statusStrip));
            _defaultColor = defaultColor ?? Color.LightSkyBlue;
            _hoverColor = hoverColor ?? Color.DeepSkyBlue;
            _baseFontStyle = baseFontStyle;
            _hoverFontStyle = hoverFontStyle;
        }

        public void Register()
        {
            if (_isRegistered || _isDisposed)
            {
                return;
            }

            _baseFont = new Font(_label.Font, _baseFontStyle);
            _hoverFont = new Font(_label.Font, _hoverFontStyle);

            _label.IsLink = false;
            _label.ForeColor = _defaultColor;
            _label.Font = _baseFont;

            _label.MouseEnter += Label_MouseEnter;
            _label.MouseLeave += Label_MouseLeave;

            _isRegistered = true;
        }

        private void Label_MouseEnter(object sender, EventArgs e)
        {
            _label.ForeColor = _hoverColor;
            _label.Font = _hoverFont;
            _statusStrip.Cursor = Cursors.Hand;
        }

        private void Label_MouseLeave(object sender, EventArgs e)
        {
            _label.ForeColor = _defaultColor;
            _label.Font = _baseFont;
            _statusStrip.Cursor = Cursors.Default;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _label.MouseEnter -= Label_MouseEnter;
            _label.MouseLeave -= Label_MouseLeave;

            _baseFont?.Dispose();
            _hoverFont?.Dispose();

            _isDisposed = true;
        }
    }
}


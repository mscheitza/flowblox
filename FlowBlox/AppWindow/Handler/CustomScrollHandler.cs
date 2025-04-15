using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.AppWindow.Handler
{
    public class CustomScrollHandler
    {
        private readonly Panel _panel;
        private Point _scrollStartPoint;
        private Point _initialScrollPosition;
        private bool _isScrolling;

        public CustomScrollHandler(Panel panel)
        {
            _panel = panel;
            _panel.AutoScroll = true;
        }

        public void Register()
        {
            _panel.MouseDown += Panel_MouseDown;
            _panel.MouseMove += Panel_MouseMove;
            _panel.MouseUp += Panel_MouseUp;
            _panel.MouseCaptureChanged += Panel_MouseCaptureChanged;
        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _scrollStartPoint = e.Location;
                _initialScrollPosition = new Point(-_panel.AutoScrollPosition.X, -_panel.AutoScrollPosition.Y);
                _isScrolling = true;
                _panel.Cursor = Cursors.SizeAll;
            }
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isScrolling)
            {
                int deltaX = e.X - _scrollStartPoint.X;
                int deltaY = e.Y - _scrollStartPoint.Y;
                _panel.AutoScrollPosition = new Point(
                    _initialScrollPosition.X - deltaX,
                    _initialScrollPosition.Y - deltaY
                );
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _isScrolling = false;
                _panel.Cursor = Cursors.Default; // Zurück zum Standard-Cursor
            }
        }

        private void Panel_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (_isScrolling)
            {
                _isScrolling = false;
                _panel.Cursor = Cursors.Default;
            }
        }
    }
}

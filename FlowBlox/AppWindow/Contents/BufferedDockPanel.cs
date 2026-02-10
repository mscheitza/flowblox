using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class BufferedDockPanel : DockPanel
    {
        public BufferedDockPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            UpdateStyles();
        }
    }
}

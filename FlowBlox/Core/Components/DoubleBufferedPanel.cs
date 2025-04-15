using System.Windows.Forms;

namespace FlowBlox.Core
{
    internal class DoubleBufferedPanel : Panel
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000;
                return handleParam;
            }
        }

        private void Initialize()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ContainerControl, true);
            this.SetStyle(ControlStyles.Opaque, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        public DoubleBufferedPanel() : base()
        {
            Initialize();
        }
    }
}

using FlowBlox.Grid;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Grid.Provider;

namespace FlowBlox.AppWindow.Handler
{
    public abstract class ModeHandlerBase
    {
        private FlowBloxProjectComponentProvider _componentProvider;
        protected BackgroundWorker _backgroundWorker_PrintGrid;
        protected int _mouseMoveTimeunit;
        protected FlowBloxUIRegistry _gridUIRegistry;

        protected ModeHandlerBase(BackgroundWorker backgroundWorker_PrintGrid, int mouseMoveTimeunit)
        {
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();
            _backgroundWorker_PrintGrid = backgroundWorker_PrintGrid;
            _mouseMoveTimeunit = mouseMoveTimeunit;
            _gridUIRegistry = _componentProvider.GetCurrentUIRegistry();
        }

        public abstract bool Print(Graphics Graphics);

        public abstract void DoMouseDown(MouseButtons buttons, Point location);

        public abstract void DoMouseMove(MouseButtons Button, Point Location);

        public abstract void DoMouseUp(MouseButtons button);
    }
}
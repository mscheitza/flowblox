using FlowBlox.Grid;
using System.Drawing;
using System.Windows.Forms;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Grid.Provider;
using FlowBlox.AppWindow.Contents;

namespace FlowBlox.AppWindow.Handler
{
    public abstract class ModeHandlerBase
    {
        private FlowBloxProjectComponentProvider _componentProvider;
        protected readonly ProjectPanel _projectPanel;
        protected FlowBloxUIRegistry _gridUIRegistry;

        protected ModeHandlerBase(ProjectPanel projectPanel)
        {
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();
            _projectPanel = projectPanel;
            _gridUIRegistry = _componentProvider.GetCurrentUIRegistry();
        }

        public abstract bool Print(Graphics Graphics);

        public abstract void DoMouseDown(MouseButtons buttons, Point location);

        public abstract void DoMouseMove(MouseButtons Button, Point Location);

        public abstract void DoMouseUp(MouseButtons button);
    }
}

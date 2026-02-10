using FlowBlox.AppWindow.ContentFactories;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow
{
    internal class DockContentEntry
    {
        public ObjectManagerDockContentFactory Factory { get; set; }

        public DockContent Content { get; set; }

        private DockState _originalDockState;

        private int _originalPaneIndex;

        public void CloseAndRemoveFromDock()
        {
            if (!Content.IsDisposed)
            {
                Content.DockPanel?.Controls.Remove(Content);
                Content.Close();
            }
        }

        public void CapturePreviousPosition()
        {
            if (Content.DockPanel != null)
            {
                _originalDockState = Content.DockState;

                var pane = Content.DockPanel.Panes
                    .FirstOrDefault(p => p.DisplayingContents.Contains(Content));

                if (pane != null)
                    _originalPaneIndex = pane.DisplayingContents.IndexOf(Content);
            }
        }

        internal void Recreate()
        {
            CapturePreviousPosition();
            CloseAndRemoveFromDock();
            Content = Factory.Create();

            if (Content.DockPanel == null)
                return;

            var targetPane = Content.DockPanel.Panes
                .FirstOrDefault(p => p.DockState == _originalDockState);

            if (targetPane != null && 
                _originalPaneIndex >= 0 && 
                _originalPaneIndex <= targetPane.DisplayingContents.Count)
            {
                Content.DockTo(targetPane, DockStyle.Fill, _originalPaneIndex);
            }
        }
    }
}

using System.Windows.Controls;

namespace FlowBlox.UICore.Factory.PropertyView
{
    internal sealed class ScrollViewerResolutionState
    {
        public ScrollViewer? ScrollViewer { get; private set; }

        public bool IsResolved { get; private set; }

        public ScrollViewer? ParentScrollViewer { get; private set; }

        public bool IsParentResolved { get; private set; }

        public void Resolve(ScrollViewer? scrollViewer)
        {
            ScrollViewer = scrollViewer;
            IsResolved = true;
        }

        public void ResolveParent(ScrollViewer? scrollViewer)
        {
            ParentScrollViewer = scrollViewer;
            IsParentResolved = true;
        }

        public void Reset()
        {
            ScrollViewer = null;
            IsResolved = false;
            ParentScrollViewer = null;
            IsParentResolved = false;
        }
    }
}

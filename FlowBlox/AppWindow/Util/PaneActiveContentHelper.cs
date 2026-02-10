using System.Collections.Generic;
using System.Linq;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Util
{
    public class PaneActiveContentHelper
    {
        public static Dictionary<DockPane, string> CapturePaneActiveContents(DockPanel dockPanel)
        {
            var result = new Dictionary<DockPane, string>();

            foreach (var pane in dockPanel.Panes)
            {
                if (pane is DockPane dockPane && dockPane.ActiveContent is DockContent activeContent)
                {
                    result[dockPane] = activeContent.Name;
                }
            }

            return result;
        }

        public static void RestorePaneActiveContents(DockPanel dockPanel, Dictionary<DockPane, string> paneActiveMap)
        {
            foreach (var kvp in paneActiveMap)
            {
                var pane = kvp.Key;
                var previousName = kvp.Value;

                var newContent = pane.Contents
                    .OfType<DockContent>()
                    .FirstOrDefault(dc => dc.Name == previousName);

                if (newContent != null && pane.ActiveContent != newContent)
                {
                    newContent.Activate();
                }
            }
        }
    }
}

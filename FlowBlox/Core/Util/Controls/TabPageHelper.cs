using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.Controls
{
    public static class TabPageHelper
    {
        public static void ShowTabPage(TabControl tabControl, TabPage tabPage)
        {
            if (!tabControl.TabPages.Contains(tabPage))
                tabControl.TabPages.Add(tabPage);
            tabPage.Show();
        }

        public static void HideTabPage(TabControl tabControl, TabPage tabPage)
        {
            if (tabControl.TabPages.Contains(tabPage))
                tabControl.TabPages.Remove(tabPage);
        }
    }
}

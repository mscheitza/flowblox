using System;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Provider;

namespace FlowBlox.Grid.Elements.UI
{
    public class UIActionsToolstripMenuItemsProvider : UIActionsProviderBase<ToolStripMenuItem>
    {
        protected override ToolStripMenuItem CreateItem(string displayName, EventHandler clickHandler, bool enabled)
        {
            var menuItem = new ToolStripMenuItem(displayName, FlowBloxMainUIImages.execute_16, clickHandler)
            {
                Enabled = enabled
            };
            return menuItem;
        }
    }
}

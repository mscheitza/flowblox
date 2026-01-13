using System;
using System.Windows.Forms;
using FlowBlox.UICore.Provider;
using FlowBlox.UICore.Utilities;
using SkiaSharp;

namespace FlowBlox.Grid.Elements.UI
{
    public class UIActionsToolstripMenuItemsProvider : UIActionsProviderBase<ToolStripMenuItem>
    {
        protected override ToolStripMenuItem CreateItem(string displayName, EventHandler clickHandler, bool enabled, SKImage icon16)
        {
            var menuItem = new ToolStripMenuItem(displayName, FlowBloxMainUIImages.execute_16, clickHandler)
            {
                Enabled = enabled
            };
            menuItem.Image = icon16 != null ?
                SkiaToSystemDrawingHelper.ToSystemDrawingImage(icon16) :
                null;
            return menuItem;
        }
    }
}
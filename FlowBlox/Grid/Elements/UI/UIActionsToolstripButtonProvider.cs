using System;
using System.Windows.Forms;
using FlowBlox.UICore.Provider;
using FlowBlox.UICore.Utilities;
using SkiaSharp;

namespace FlowBlox.Grid.Elements.UI
{
    public class UIActionsToolstripButtonProvider : UIActionsProviderBase<ToolStripButton>
    {
        protected override ToolStripButton CreateItem(string displayName, EventHandler clickHandler, bool enabled, SKImage icon16)
        {
            var button = new ToolStripButton(displayName, FlowBloxMainUIImages.execute_16, clickHandler)
            {
                Enabled = enabled
            };
            button.Image = icon16 != null ?
                SkiaToSystemDrawingHelper.ToSystemDrawingImage(icon16) :
                null;
            return button;
        }
    }
}
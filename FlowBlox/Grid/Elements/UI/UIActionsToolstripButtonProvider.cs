using System;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Provider;

namespace FlowBlox.Grid.Elements.UI
{
    public class UIActionsToolstripButtonProvider : UIActionsProviderBase<ToolStripButton>
    {
        protected override ToolStripButton CreateItem(string displayName, EventHandler clickHandler, bool enabled)
        {
            var button = new ToolStripButton(displayName, FlowBloxMainUIImages.execute_16, clickHandler)
            {
                Enabled = enabled
            };
            return button;
        }
    }
}

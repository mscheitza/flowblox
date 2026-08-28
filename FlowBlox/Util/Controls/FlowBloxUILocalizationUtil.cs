using System.Windows.Forms;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Util.Controls
{
    public class FlowBloxUILocalizationUtil
    {
        public static void Localize(Control control)
        {
            string key = $"{control.Name}_Text";
            string localizedText = FlowBloxResourceUtil.GetLocalizedString(key, typeof(FlowBloxMainUITexts));
            if (!string.IsNullOrEmpty(localizedText))
                control.Text = localizedText;

            LocalizeControls(control, control.Name);
        }

        private static void LocalizeControls(Control parent, string context)
        {
            foreach (Control control in parent.Controls)
            {
                string key = $"{context}_{control.Name}_Text";
                string localizedText = FlowBloxResourceUtil.GetLocalizedString(key, typeof(FlowBloxMainUITexts));
                if (!string.IsNullOrEmpty(localizedText))
                    control.Text = localizedText;

                if (control.HasChildren)
                    LocalizeControls(control, context);

                if (control is ToolStrip toolStrip)
                    LocalizeToolStripItems(toolStrip, context);

                if (control.ContextMenuStrip != null)
                    LocalizeContextMenuStrip(control.ContextMenuStrip, context);
            }
        }

        private static void LocalizeContextMenuStrip(ContextMenuStrip contextMenuStrip, string context)
        {
            LocalizeToolStripItems(contextMenuStrip, context);
        }

        private static void LocalizeToolStripItems(ToolStrip toolStrip, string context)
        {
            foreach (ToolStripItem item in toolStrip.Items)
                LocalizeToolStripItem(item, context);
        }

        private static void LocalizeToolStripItem(ToolStripItem item, string context)
        {
            string key = $"{context}_{item.Name}_Text";
            string localizedText = FlowBloxResourceUtil.GetLocalizedString(key, typeof(FlowBloxMainUITexts));
            if (!string.IsNullOrEmpty(localizedText))
                item.Text = localizedText;

            if (item is not ToolStripDropDownItem dropDownItem)
                return;

            foreach (ToolStripItem subItem in dropDownItem.DropDownItems)
                LocalizeToolStripItem(subItem, context);
        }
    }
}
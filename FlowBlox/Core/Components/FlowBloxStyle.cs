using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Elements.UserControls;

namespace FlowBlox.Core
{
    /// <summary>
    /// Die Stilgebende Klasse der Applikation. Der aktuelle Style kann in der [General.Style] Option definiert werden.
    /// In dieser Version können Stile noch nicht eigens definiert werden.
    /// </summary>
    public class FlowBloxStyle
    {
        public const string StyleIgnore = "style_ignore";
        public const string StyleIgnoreSelf = "style_ignore_self";
        public const string StyleHighlight = "style_highlight";
        public const string StyleHighlightHint = "style_highlight_hint";
        public const string StyleHeader = "style_header";
        public const string StyleKeepFont = "style_keep_font";

        /// <summary>
        /// Wendet einen Stil auf ein <see cref="FlowBlockUIElement"/> an. 
        /// </summary>
        /// <param name="uiElement"></param>
        internal static void ApplyStyle(FlowBlockUIElement uiElement)
        {
            FlowBloxSyleBase baseStyle = GetStyleFromOptions();

            if (baseStyle != null)
            {
                uiElement.ForeColor = baseStyle.FlowBlockUIElementForeColor;

                foreach (Control childControl in uiElement.Controls)
                {
                    if (childControl is TableLayoutPanel)
                    {
                        ((TableLayoutPanel)childControl).BackColor = baseStyle.FlowBlockUIElementBackColor;
                    }
                }
            }
        }

        /// <summary>
        /// Wendet einen Stil auf ein <c>System.Windows.Forms.Form</c> Objekt an. Diese Methode sollte nach der <c>InitializeComponent</c> Methode gerufen werden.
        /// </summary>
        /// <param name="form"></param>
        public static void ApplyStyle(Form form)
        {
            FlowBloxSyleBase style = GetStyleFromOptions();
            if (style != null)
            {
                form.BackColor = style.ControlBackColor;
                form.ForeColor = style.ControlForeColor;
            }
            if (form.MainMenuStrip != null)
            {
                ApplyStyle(form.MainMenuStrip);
            }

            foreach (Control control in form.Controls)
            {
                ApplyStyle(control);
            }
        }

        /// <summary>
        /// Wendet einen Stil auf ein <c>System.Windows.Forms.Control</c> Objekt an.
        /// </summary>
        /// <param name="control"></param>
        public static void ApplyStyle(Control control)
        {
            if (control.Tag == StyleIgnore)
                return;

            if (control.Tag != StyleIgnoreSelf)
            {
                FlowBloxSyleBase style = GetStyleFromOptions();
                if (style != null)
                {
                    if (control.Tag == StyleHighlight)
                        control.BackColor = style.ControlHighlightBackColor;
                    else if (control.Tag == StyleHighlightHint)
                        control.BackColor = style.ControlHighlightHintBackColor;
                    else if (control.Tag == StyleHeader)
                    {
                        control.BackColor = style.ControlHeaderBackColor;
                        control.ForeColor = style.ControlHeaderForeColor;
                    }
                    else
                    {
                        control.BackColor = style.ControlBackColor;
                    }

                    control.ForeColor = style.ControlForeColor;

                    if (control.Tag != StyleKeepFont)
                        control.Font = style.DefaultFont;
                }

                if (control is TextBox) ApplyStyle((TextBox)control);
                if (control is CheckBox) ApplyStyle((CheckBox)control);
                if (control is Button) ApplyStyle((Button)control);
                if (control is MenuStrip) ApplyStyle((MenuStrip)control);
                if (control is ToolStrip) ApplyStyle((ToolStrip)control);
                if (control is ListView) ApplyStyle((ListView)control);
                if (control is DataGridView) ApplyStyle((DataGridView)control);
                if (control.ContextMenuStrip != null) ApplyStyle(control.ContextMenuStrip);
            }

            foreach (Control childControl in control.Controls)
            {
                ApplyStyle(childControl);
            }
        }

        private static FlowBloxSyleBase GetStyleFromOptions()
        {
            string selectedStyle = FlowBloxOptions.GetOptionInstance().OptionCollection["General.Style"].Value;
            if (selectedStyle.Equals("Professional"))
                return new FlowBloxProfessionalStyle();

            return null;
        }

        private static void ApplyStyle(TextBox textBox)
        {
            var style = GetStyleFromOptions();
            if (style != null)
            {
                if (!textBox.ReadOnly)
                    textBox.BackColor = style.TextBoxBackColor;
            }
        }

        private static void ApplyStyle(DataGridView dataGridView)
        {
            FlowBloxSyleBase style = GetStyleFromOptions();
            if (style != null)
            {

                // Verwendung eines flachen Stils für Zellen und Header
                dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                dataGridView.BorderStyle = BorderStyle.None;

                // Anwendung eines flachen Styles auf ScrollBars
                dataGridView.ScrollBars = ScrollBars.Both;
                dataGridView.ColumnHeadersDefaultCellStyle.Font = style.HeaderFont;
                dataGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);

                // Einstellen der Höhe der Kopfzeilen
                dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
                dataGridView.ColumnHeadersHeight = 40;
            }
        }

        private static void ApplyStyle(Button button)
        {
            var flowBloxStyle = GetStyleFromOptions();
            if (flowBloxStyle != null)
            {
                button.FlatStyle = flowBloxStyle.ButtonFlatStyle;
            }
        }

        private static void ApplyStyle(CheckBox checkBox)
        {
            checkBox.Padding = new Padding(5);
            checkBox.Margin = new Padding(5);
        }

        private static void ApplyStyle(ListView listView)
        {
            FlowBloxSyleBase style = GetStyleFromOptions();
            if (style != null)
            {
                listView.BackColor = style.ListViewBackColor;
                listView.OwnerDraw = true;
                listView.FullRowSelect = true;
                listView.GridLines = false;

                typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(listView, true, null);

                listView.DrawColumnHeader += (s, e) =>
                {
                    e.DrawBackground();
                    using (var headerFont = style.HeaderFont)
                    {
                        TextRenderer.DrawText(e.Graphics, e.Header.Text, headerFont, e.Bounds, SystemColors.WindowText, 
                            TextFormatFlags.VerticalCenter | 
                            TextFormatFlags.LeftAndRightPadding | 
                            TextFormatFlags.Left);
                    }
                };
            }
        }

        private static void ApplyStyle(ToolStrip toolStrip)
        {
            FlowBloxSyleBase style = GetStyleFromOptions();
            if (style != null)
            {
                ApplyStyle(toolStrip, style);
            }
        }

        private static void ApplyStyle(MenuStrip menuStrip)
        {
            FlowBloxSyleBase style = GetStyleFromOptions();
            if (style != null)
            {
                ApplyStyle(menuStrip, style);
            }
        }

        private static void ApplyStyle(ToolStrip toolStrip, FlowBloxSyleBase colorTable)
        {
            toolStrip.Renderer = new ToolStripProfessionalRenderer(colorTable);
            toolStrip.BackColor = colorTable.ToolStripBackColor;
            foreach (ToolStripItem toolStripItem in toolStrip.Items)
            {
                toolStripItem.ForeColor = colorTable.MenuForeColor;
                if (toolStripItem is ToolStripDropDownButton)
                {
                    foreach (ToolStripItem DropDownItem in ((ToolStripDropDownButton)toolStripItem).DropDownItems)
                    {
                        DropDownItem.ForeColor = colorTable.MenuForeColor;
                    }
                }
            }
        }

        private static void ApplyStyle(MenuStrip menuStrip, FlowBloxSyleBase style)
        {
            menuStrip.Renderer = new ToolStripProfessionalRenderer(style);

            menuStrip.Font = style.MenuStripFont;
            menuStrip.BackColor = style.MenuBackColor;
            menuStrip.ForeColor = style.MenuForeColor;

            foreach (ToolStripMenuItem menuItem in menuStrip.Items)
            {
                ApplyStyle(menuItem, style);
            }
        }

        private static void ApplyStyle(ToolStripItem toolStripItem, FlowBloxSyleBase style)
        {
            toolStripItem.ForeColor = style.MenuForeColor;
            toolStripItem.ForeColor = style.MenuForeColor;

            if (toolStripItem is ToolStripDropDownItem)
            {
                foreach (ToolStripItem dropDownItem in ((ToolStripDropDownItem)toolStripItem).DropDownItems)
                {
                    ApplyStyle(dropDownItem, style);
                }
            }
        }

        private static void ApplyStyle(ContextMenuStrip contextMenu)
        {
            FlowBloxSyleBase style = GetStyleFromOptions();
            contextMenu.Renderer = new ToolStripProfessionalRenderer(style);
            contextMenu.ForeColor = style.MenuForeColor;
            contextMenu.Font = style.MenuStripFont;
            foreach (ToolStripMenuItem item in contextMenu.Items.OfType<ToolStripMenuItem>())
            {
                ApplyStyle(item, style);
            }
        }
    }
}

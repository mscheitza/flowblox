using System.Drawing;

namespace FlowBlox.Core
{
    internal class FlowBloxProfessionalStyle : FlowBloxSyleBase
    {
        public FlowBloxProfessionalStyle() : base()
        {
            this.ItemSelectedBeginColor = Color.Orange;
            this.ItemSelectedEndColor = Color.OrangeRed;
            this.ItemPressedMiddleColor = Color.WhiteSmoke;
            this.ItemBorderColor = Color.FromArgb(210, 210, 210);
            this.MenuBorderColor = Color.White;
            this.MenuBackColor = Color.FromArgb(36, 36, 36);
            this.MenuForeColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
            this.SeparatorColor = Color.FromKnownColor(KnownColor.GhostWhite);
            this.FlowBlockUIElementBackColor = Color.FromArgb(50, 50, 50);
            this.FlowBlockUIElementForeColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
            this.ListViewBackColor = Color.FromKnownColor(KnownColor.White);
            this.TextBoxBackColor = Color.FromKnownColor(KnownColor.White);
            this.ToolStripBackColor = Color.FromArgb(70, 70, 70);
            this.ControlBackColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
            this.ControlForeColor = Color.FromArgb(51, 51, 51);
            this.DefaultFont = new Font("Segoe UI", 9, FontStyle.Regular);
            this.HeaderFont = new Font("Segoe UI", 9, FontStyle.Bold);
            this.MenuStripFont = new Font("Segoe UI", 9, FontStyle.Regular);
            this.ButtonFlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ControlHighlightBackColor = Color.FromArgb(220, 232, 244);
            this.ControlHighlightHintBackColor = Color.FromArgb(255, 255, 153);
            this.ControlHeaderBackColor = Color.FromArgb(210, 223, 234);
            this.ControlHeaderForeColor = Color.FromArgb(43, 0, 53);
        }
    }
}

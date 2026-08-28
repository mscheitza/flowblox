using System.Drawing;

namespace FlowBlox.Components
{
    internal class FlowBloxProfessionalStyle : FlowBloxSyleBase
    {
        public FlowBloxProfessionalStyle() : base()
        {
            ItemSelectedBeginColor = Color.SteelBlue;
            ItemSelectedEndColor = Color.DodgerBlue;
            ItemPressedMiddleColor = Color.WhiteSmoke;
            ItemBorderColor = Color.FromArgb(210, 210, 210);
            MenuBorderColor = Color.White;
            MenuBackColor = Color.FromArgb(36, 36, 36);
            MenuForeColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
            SeparatorColor = Color.FromKnownColor(KnownColor.GhostWhite);
            FlowBlockUIElementBackColor = Color.FromArgb(50, 50, 50);
            FlowBlockUIElementForeColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
            ListViewBackColor = Color.FromKnownColor(KnownColor.White);
            TextBoxBackColor = Color.FromKnownColor(KnownColor.White);
            ToolStripBackColor = Color.FromArgb(70, 70, 70);
            ControlBackColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
            ControlForeColor = Color.FromArgb(51, 51, 51);
            DefaultFont = new Font("Segoe UI", 9, FontStyle.Regular);
            HeaderFont = new Font("Segoe UI", 9, FontStyle.Bold);
            MenuStripFont = new Font("Segoe UI", 9, FontStyle.Regular);
            ButtonFlatStyle = System.Windows.Forms.FlatStyle.Popup;
            ControlHighlightBackColor = Color.FromArgb(220, 232, 244);
            ControlHighlightHintBackColor = Color.FromArgb(255, 255, 153);
            ControlHeaderBackColor = Color.FromArgb(210, 223, 234);
            ControlHeaderForeColor = Color.FromArgb(43, 0, 53);
        }
    }
}
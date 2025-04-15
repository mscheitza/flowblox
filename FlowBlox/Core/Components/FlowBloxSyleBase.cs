using System.Windows.Forms;
using System.Drawing;

namespace FlowBlox.Core
{
    internal abstract class FlowBloxSyleBase : ProfessionalColorTable
    {
        public Color ItemSelectedBeginColor { get; set; }
        public Color ItemSelectedEndColor { get; set; }
        public Color ItemPressedMiddleColor { get; set; }
        public Color ItemBorderColor { get; set; }
        public Color MenuBorderColor { get; set; }
        public Color MenuBackColor { get; set; }
        public Color MenuForeColor { get; set; }
        public Color SeparatorColor { get; set; }
        public Color FlowBlockUIElementBackColor { get; set; }
        public Color FlowBlockUIElementForeColor { get; set; }
        public Color ToolStripBackColor { get; set; }
        public Color ControlBackColor { get; set; }
        public Color ControlForeColor { get; set; }
        public Font DefaultFont { get; set; }
        public Font HeaderFont { get; set; }
        public Font MenuStripFont { get; set; }
        public FlatStyle ButtonFlatStyle { get; set; }
        public Color TextBoxBackColor { get; set; }
        public Color ListViewBackColor { get; set; }
        public Color ControlHighlightBackColor { get; set; }
        public Color ControlHighlightHintBackColor { get; set; }
        public Color ControlHeaderBackColor { get; set; }
        public Color ControlHeaderForeColor { get; set; }

        #region Overridden properties from ProfessionalColorTable

        public override Color ButtonPressedGradientBegin => ItemSelectedBeginColor;

        public override Color ButtonPressedGradientMiddle => ItemPressedMiddleColor;

        public override Color ButtonPressedGradientEnd => ItemSelectedBeginColor;

        public override Color MenuItemSelected => ItemSelectedBeginColor;

        public override Color MenuItemBorder => ItemBorderColor;

        public override Color SeparatorDark => SeparatorColor;

        public override Color ImageMarginGradientBegin => MenuBackColor;

        public override Color ImageMarginGradientEnd => MenuBackColor;

        public override Color ImageMarginGradientMiddle => Color.FromKnownColor(KnownColor.DarkSlateGray);

        public override Color ToolStripDropDownBackground => MenuBackColor;

        public override Color MenuItemSelectedGradientBegin => ItemSelectedBeginColor;

        public override Color MenuItemSelectedGradientEnd => ItemSelectedEndColor;

        public override Color MenuItemPressedGradientBegin => ItemSelectedBeginColor;

        public override Color MenuItemPressedGradientEnd => ItemSelectedEndColor;

        public override Color MenuBorder => MenuBorderColor;

        public override Color ButtonSelectedGradientBegin => ItemSelectedBeginColor;

        public override Color ButtonSelectedGradientEnd => ItemSelectedEndColor;

        #endregion
    }
}

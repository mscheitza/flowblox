using System;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Util.Controls;

namespace FlowBlox.Views
{
    /// <summary>
    /// Fenster zur Einstellung der Höhe und Breite des WebFlowIDE.Grids.
    /// </summary>
    internal partial class GridView : Form
    {
        public new int Width { get; private set; } = 0;
        public new int Height { get; private set; } = 0;

        public GridView(int Width, int Height)
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);

            this.tbWidth.Text = Width.ToString();
            this.tbHeight.Text = Height.ToString();
            this.Width = Width;
            this.Height = Height;
        }

        private void btApply_Click(object sender, EventArgs e)
        {
            int checkWidth;
            try
            {
                checkWidth = int.Parse(tbWidth.Text);

                if (checkWidth < 2000) throw new Exception();
            }
            catch (Exception)
            {
                FlowBloxMessageBox.Show
                    (
                        this,
                        "Bitte geben Sie eine gültige Breite für Ihr Raster an. Mindestwert: 2000px",
                        "Ungültige Breite angegeben",
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Info
                    );

                return;
            }

            int checkHeight;
            try
            {
                checkHeight = int.Parse(tbHeight.Text);

                if (checkHeight < 1000) throw new Exception();
            }
            catch (Exception)
            {
                FlowBloxMessageBox.Show
                    (
                        this,
                        "Bitte geben Sie eine gültige Höhe für Ihr Raster an. Mindestwert: 1000px",
                        "Ungültige Breite angegeben",
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Info
                    );

                return;
            }

            this.Width = checkWidth;
            this.Height = checkHeight;

            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.Controls
{
    internal class TextBoxCustomizer
    {
        public static void CustomizeTextBox(Control parent, TextBox textBox)
        {
            // Erstellen eines Panels als Container für die TextBox
            Panel panel = new Panel();
            panel.Size = new Size(textBox.Width + 10, textBox.Height + 10);  // Größere Größe als die TextBox
            panel.BackColor = Color.White;
            panel.Padding = new Padding(5);  // Inneres Padding, um Abstand zum Text zu schaffen
            panel.Location = new Point(textBox.Location.X - 5, textBox.Location.Y - 5);
            panel.Paint += (sender, e) => {
                // Zeichnen der abgerundeten Ecken
                Graphics graphics = e.Graphics;
                int cornerRadius = 10;
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(new Rectangle(0, 0, cornerRadius, cornerRadius), 180, 90);
                path.AddArc(new Rectangle(panel.Width - cornerRadius - 1, 0, cornerRadius, cornerRadius), -90, 90);
                path.AddArc(new Rectangle(panel.Width - cornerRadius - 1, panel.Height - cornerRadius - 1, cornerRadius, cornerRadius), 0, 90);
                path.AddArc(new Rectangle(0, panel.Height - cornerRadius - 1, cornerRadius, cornerRadius), 90, 90);
                path.CloseAllFigures();
                graphics.DrawPath(new Pen(Color.Gray, 2), path);
                graphics.FillPath(new SolidBrush(panel.BackColor), path);
            };

            // Hinzufügen der TextBox zum Panel
            textBox.Location = new Point(5, 5);  // Zentrieren der TextBox im Panel
            textBox.BorderStyle = BorderStyle.None;  // Entfernen des Standard-Borders der TextBox
            panel.Controls.Add(textBox);

            // Hinzufügen des Panels zum Hauptformular oder zum übergeordneten Container
            parent.Controls.Add(panel);
        }
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.Views.PropertyView.Handler
{
    public class ScalableTextboxHandler
    {
        private bool resizing = false;
        private Point lastPoint;
        private TextBox textBox;

        public ScalableTextboxHandler(TextBox textBox)
        {
            this.textBox = textBox;
        }

        public void Register()
        {
            textBox.MouseDown += new MouseEventHandler(myTextBox_MouseDown);
            textBox.MouseMove += new MouseEventHandler(myTextBox_MouseMove);
            textBox.MouseUp += new MouseEventHandler(myTextBox_MouseUp);
        }

        private void myTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && e.X >= textBox.Width - 15 && e.Y >= textBox.Height - 15)
            {
                resizing = true;
                lastPoint = e.Location;
                textBox.Cursor = Cursors.SizeNWSE;
            }
        }

        private void myTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (e.X >= textBox.Width - 15 && e.Y >= textBox.Height - 15)
                {
                    textBox.Cursor = Cursors.SizeNWSE;
                }
                else
                {
                    textBox.Cursor = Cursors.IBeam;
                }

                if (resizing)
                {
                    textBox.Width += e.X - lastPoint.X;
                    textBox.Height += e.Y - lastPoint.Y;
                    lastPoint = e.Location;
                }
            }
        }

        private void myTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            resizing = false;
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Cursor = Cursors.Default;
            }
        }
    }
}

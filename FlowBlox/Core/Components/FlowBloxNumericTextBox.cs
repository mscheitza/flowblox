using FlowBlox.Core.Util.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.Core.Components
{
    public class FlowBloxNumericTextBox : Panel
    {
        private NumericTextBox numericTextBox;
        private Panel textBoxContainer;

        public FlowBloxNumericTextBox()
        {
            InitializeComponents();
        }

        public NumericTextBox InnerNumericTextBox => numericTextBox;

        private void InitializeComponents()
        {
            numericTextBox = new NumericTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };

            ControlHelper.EnableDoubleBuffer(numericTextBox);
            ControlHelper.EnableOptimizedDoubleBuffer(numericTextBox);

            textBoxContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(3, 3, 3, 3),
                BackColor = Color.White,
                Tag = FlowBloxStyleTags.StyleIgnoreSelf
            };
            textBoxContainer.Controls.Add(numericTextBox);

            this.Controls.Add(textBoxContainer);

            this.Height = 24;
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }
    }
}

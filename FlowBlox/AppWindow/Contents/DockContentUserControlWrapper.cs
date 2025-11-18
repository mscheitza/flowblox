using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class DockContentUserControlWrapper<T> : DockContent where T : UserControl, new()
    {
        private T userControl;

        public DockContentUserControlWrapper()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            userControl = new T();
            userControl.AutoScroll = true;
            userControl.Dock = DockStyle.Fill;
            userControl.Name = typeof(T).Name.ToLower() + "Control";
            userControl.TabIndex = 0;
            userControl.Padding = new Padding(0, 25, 0, 0);
            userControl.Dock = DockStyle.Fill;
            this.Controls.Add(userControl);
            this.Name = typeof(T).Name.ToLower() + "DockPanel";
            this.Text = userControl.Text;
            this.Name = userControl.Text;
            this.ResumeLayout(false);
        }

        public T UserControl
        {
            get 
            { 
                return userControl; 
            }
        }
    }
}

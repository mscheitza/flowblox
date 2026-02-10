using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.Controls
{
    public class WindowsFormsHelper
    {
        public static void ShowNonModal(Form parentForm, Form childForm)
        {
            CenterParent(parentForm, childForm);
            childForm.Show(parentForm);
        }

        public static void CenterParent(Form parentForm, Form childForm)
        {
            childForm.StartPosition = FormStartPosition.Manual;
            childForm.Location = new Point(parentForm.Location.X + (parentForm.Width - childForm.Width) / 2, parentForm.Location.Y + (parentForm.Height - childForm.Height) / 2);
        }
    }
}

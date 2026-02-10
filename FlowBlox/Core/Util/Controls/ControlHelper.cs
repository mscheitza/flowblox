using FlowBlox.Core.Components;
using System.Reflection;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.Controls
{
    public static class ControlHelper
    {
        public static void EnableDoubleBuffer(Control control)
        {
            PropertyInfo propertyInfo = typeof(Control).GetProperty("DoubleBuffered", 
                BindingFlags.SetProperty | 
                BindingFlags.NonPublic | 
                BindingFlags.Instance);

            if (propertyInfo != null)
                propertyInfo.SetValue(control, true, null);
        }

        public static void EnableOptimizedDoubleBuffer(Control control)
        {
            MethodInfo setStyleMethod = control.GetType().GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
            if (setStyleMethod != null)
                setStyleMethod.Invoke(control, [ControlStyles.OptimizedDoubleBuffer, true]);

            MethodInfo updateStylesMethod = control.GetType().GetMethod("UpdateStyles", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateStylesMethod != null)
                updateStylesMethod.Invoke(control, null);
        }

        public static bool IsChildControl(Control parent, Control child)
        {
            Control currentParent = child.Parent;
            while (currentParent != null)
            {
                if (currentParent == parent)
                    return true;
                currentParent = currentParent.Parent;
            }
            return false;
        }

        public static T FindParentOfType<T>(Control control, bool topMost = false) where T : Control
        {
            T foundParent = null;
            Control current = control;

            while ((current = current.Parent) != null)
            {
                if (current is T)
                {
                    foundParent = (T)current;
                    if (!topMost)
                        return foundParent;
                }
            }

            return foundParent;
        }


        public static void FocusControlByName(Form form, string controlName, bool selectAll = false)
        {
            Control targetControl = FindControlRecursive(form, controlName);
            if (targetControl != null)
            {
                TabPage tabPage = FindParentTabPage(targetControl);
                if (tabPage != null)
                    ((TabControl)tabPage.Parent).SelectedTab = tabPage;

                targetControl.Focus();

                if (selectAll)
                {
                    if (targetControl is TextBox textBox)
                        textBox.Select(0, textBox.Text.Length);
                    else if (targetControl is NumericUpDown numericUpDown)
                        numericUpDown.Select(0, numericUpDown.Text.Length);
                    else if (targetControl is MaskedTextBox maskedTextBox)
                        maskedTextBox.Select(0, maskedTextBox.Text.Length);
                    else if (targetControl is NumericTextBox numericTextBox)
                        numericTextBox.Select(0, numericTextBox.Text.Length);
                }

            }
        }

        private static Control FindControlRecursive(Control root, string controlName)
        {
            if (root.Name == controlName)
            {
                return root;
            }

            foreach (Control control in root.Controls)
            {
                Control foundControl = FindControlRecursive(control, controlName);
                if (foundControl != null)
                {
                    return foundControl;
                }
            }

            return null;
        }

        private static TabPage FindParentTabPage(Control control)
        {
            while (control != null && !(control is TabPage))
            {
                control = control.Parent;
            }
            return control as TabPage;
        }
    }
}

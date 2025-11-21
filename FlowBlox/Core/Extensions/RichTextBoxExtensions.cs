using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace FlowBlox.Core.Extensions
{
    public static class RichTextBoxExtensions
    {
        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Disables redrawing of the control to avoid flicker during bulk updates.
        /// </summary>
        public static void BeginUpdate(this RichTextBox richTextBox)
        {
            SendMessage(richTextBox.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Enables redrawing of the control after bulk updates and forces a repaint.
        /// </summary>
        public static void EndUpdate(this RichTextBox richTextBox)
        {
            SendMessage(richTextBox.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
            richTextBox.Invalidate();
        }
    }
}

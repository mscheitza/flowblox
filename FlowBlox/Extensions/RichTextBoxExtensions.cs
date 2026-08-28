using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FlowBlox.Extensions
{
    public static class RichTextBoxExtensions
    {
        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

        /// <summary>
        /// Disables redrawing of the control to avoid flicker during bulk updates.
        /// </summary>
        public static void BeginUpdate(this RichTextBox richTextBox)
        {
            SendMessage(richTextBox.Handle, WM_SETREDRAW, nint.Zero, nint.Zero);
        }

        /// <summary>
        /// Enables redrawing of the control after bulk updates and forces a repaint.
        /// </summary>
        public static void EndUpdate(this RichTextBox richTextBox)
        {
            SendMessage(richTextBox.Handle, WM_SETREDRAW, new nint(1), nint.Zero);
            richTextBox.Invalidate();
        }
    }
}
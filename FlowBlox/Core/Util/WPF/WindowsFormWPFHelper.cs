using FlowBlox.Core.Logging;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Views;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace FlowBlox.Core.Util.WPF
{
    public static class WindowsFormWPFHelper
    {
        public static bool? ShowDialog(Window wpfDialog, IWin32Window owner)
        {
            var helper = new WindowInteropHelper(wpfDialog);
            helper.Owner = owner.Handle;

            wpfDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ElementHost.EnableModelessKeyboardInterop(wpfDialog);
            return InvokeShowDialog(() => wpfDialog.ShowDialog(), owner);
        }

        public static bool? ShowDialog(Window wpfDialog, Control control)
        {
            var owner = (IWin32Window)control.FindForm();
            return InvokeShowDialog(() => ShowDialog(wpfDialog, owner), owner);
        }

        public static DialogResult? ShowWinFormsDialog(Form winFormsDialog, Window wpfOwner)
        {
            var wpfHandle = new WindowInteropHelper(wpfOwner).Handle;
            var owner = new Win32WindowWrapper(wpfHandle);
            return InvokeShowDialog<DialogResult ?>(() => winFormsDialog.ShowDialog(owner), owner);
        }

        private static TResult InvokeShowDialog<TResult>(Func<TResult> func, IWin32Window owner)
        {
            try
            {
                return func.Invoke();
            }
            catch(Exception ex)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();
                logger.Exception(ex);

                FlowBloxMessageBox.Show
                (
                    null,
                    FlowBloxResourceUtil.GetLocalizedString("WindowsFormWPFHelper_ShowDialog_Failure_Message", typeof(FlowBloxMainUITexts)),
                    FlowBloxResourceUtil.GetLocalizedString("WindowsFormWPFHelper_ShowDialog_Failure_Title", typeof(FlowBloxMainUITexts)),
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Error
                );
            }

            return default;
        }
    }
}

using FlowBlox.Core.Constants;
using FlowBlox.Core.Util.Resources;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlowBlox.UICore.Utilities
{
    public enum MessageBoxType
    {
        Question,
        Notification,
        Error
    }

    public static class MessageBoxHelper
    {
        public static async Task<bool?> ShowQuestionAsync(MetroWindow window, string question)
        {
            if (window == null)
                window = Application.Current.MainWindow as MetroWindow;

            if (window == null)
                return null;

            var result = await window.ShowMessageAsync(
                FlowBloxResourceUtil.GetLocalizedString("Global_MessageBox_Question_Title"),
                question,
                MessageDialogStyle.AffirmativeAndNegative);

            return result == MessageDialogResult.Affirmative;
        }

        public static async Task ShowMessageBoxAsync(MetroWindow window, MessageBoxType messageBoxType, string description)
        {
            if (window == null)
                window = Application.Current.MainWindow as MetroWindow;

            if (window == null)
                return;

            if (messageBoxType == MessageBoxType.Notification)
                await window.ShowMessageAsync(FlowBloxResourceUtil.GetLocalizedString("Global_MessageBox_Notification_Title"), description);

            if (messageBoxType == MessageBoxType.Error)
                await window.ShowMessageAsync(FlowBloxResourceUtil.GetLocalizedString("Global_MessageBox_Error_Title"), description);
        }

        public static async Task ShowMessageBoxAsync(MetroWindow window, string title, string description)
        {
            if (window == null)
                window = Application.Current.MainWindow as MetroWindow;

            if (window == null)
                return;

            await window.ShowMessageAsync(title, description);
        }
    }
}

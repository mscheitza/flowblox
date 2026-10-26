using FlowBlox.Core.Models.Deletion;
using FlowBlox.UICore.Views;
using FlowBlox.UICore.Interfaces;
using System.Windows;

namespace FlowBlox.UICore.Util
{
    public static class DeletionConfirmationService
    {
        public static bool Confirm(Window owner, DeletionReport report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            if (!report.HasDependencies)
                return true;

            var dialog = new DeletionConfirmationWindow(report) { Owner = owner };
            return dialog.ShowDialog() == true;
        }

        public static bool Confirm(IDialogService dialogService, DeletionReport report)
        {
            if (report == null)
                return false;
            if (!report.HasDependencies)
                return true;
            return dialogService?.ShowWPFDialog(new DeletionConfirmationWindow(report), true) == true;
        }
    }
}

using FlowBlox.Core.DependencyInjection;
using FlowBlox.Interfaces;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Interfaces;
using FlowBlox.Views;
using System;
using System.Windows.Forms;

namespace FlowBlox.Services
{
    public class FlowBloxMessageBoxService : IFlowBloxMessageBoxService
    {
        public FlowBloxMessageBoxDialogResult ShowMessageBox(string message, string title, FlowBloxMessageBoxTypes flowBloxMessageBoxType)
        {
            // Determine current owner form
            var ownerService = FlowBloxServiceLocator.Instance.GetService<IOwnerService>();
            var ownerForm = ownerService.GetCurrentOwner();

            // Set the icon based on the message box type
            FlowBloxMessageBox.Icons icon = flowBloxMessageBoxType switch
            {
                FlowBloxMessageBoxTypes.Information => FlowBloxMessageBox.Icons.Info,
                FlowBloxMessageBoxTypes.Question => FlowBloxMessageBox.Icons.Question,
                FlowBloxMessageBoxTypes.Error => FlowBloxMessageBox.Icons.Error,
                FlowBloxMessageBoxTypes.Warning => FlowBloxMessageBox.Icons.Warning,
                _ => FlowBloxMessageBox.Icons.Info // Default to 'Info' if the type is not recognized
            };

            // Set the buttons based on the message box type
            // For 'Question' type, show 'Yes' and 'No' buttons; for other types, show 'OK' button
            FlowBloxMessageBox.Buttons buttons = flowBloxMessageBoxType == FlowBloxMessageBoxTypes.Question
                ? FlowBloxMessageBox.Buttons.YesNo
                : FlowBloxMessageBox.Buttons.OK;

            // Show the message box and capture the result
            var result = FlowBloxMessageBox.Show(
                ownerForm,
                message,
                title,
                buttons,
                icon
            );

            // Return the corresponding enum value based on the result
            return result switch
            {
                DialogResult.OK => FlowBloxMessageBoxDialogResult.OK,
                DialogResult.Yes => FlowBloxMessageBoxDialogResult.Yes,
                DialogResult.No => FlowBloxMessageBoxDialogResult.No,
                _ => FlowBloxMessageBoxDialogResult.OK
            };
        }
    }
}

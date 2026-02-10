using FlowBlox.Core.Attributes;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.WPF;
using FlowBlox.Interfaces;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Models.DialogService;
using FlowBlox.Views;
using System.Collections;
using System.Windows;
using System.Windows.Forms;

namespace FlowBlox.Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowWPFDialog(Window window, bool isModal = true)
        {
            var ownerService = FlowBloxServiceLocator.Instance.GetService<IOwnerService>();
            var owner = ownerService.GetCurrentOwner();
            return WindowsFormWPFHelper.ShowDialog(window, owner);
        }

        public FieldSelectionResult InvokeFieldSelection(object target, FlowBlockUIAttribute flowBlockUI, Window window)
        {
            return InvokeFieldSelection(target, flowBlockUI, null, window);
        }

        public FieldSelectionResult InvokeFieldSelection(object target, FlowBlockUIAttribute flowBlockUI, IList items, Window window)
        {
            var flowBlock = target as BaseFlowBlock;

            var fieldSelectionWindow = new FieldSelectionWindow(flowBlock, items)
            {
                IsRequired = !flowBlockUI.UiOptions.HasFlag(UIOptions.FieldSelectionDefaultNotRequired),
                HideRequired = flowBlockUI.UiOptions.HasFlag(UIOptions.FieldSelectionHideRequired)
            };

            var dialogResult = WindowsFormWPFHelper.ShowWinFormsDialog(fieldSelectionWindow, window);

            if (dialogResult == DialogResult.OK)
            {
                var selectedFields = fieldSelectionWindow.SelectedFields;
                var isRequired = fieldSelectionWindow.IsRequired;

                var result = new FieldSelectionResult
                {
                    Success = true,
                    Target = flowBlock,
                    SelectedFields = selectedFields,
                    IsRequired = isRequired
                };
 
                return result;
            }

            return new FieldSelectionResult();
        }

        public InsertTextOrFieldResult InvokeInsertTextOrField(BaseFlowBlock flowBlock, string parameterName, Window window)
        {
            var insertTextOrFieldDialog = new InsertTextOrField(flowBlock, parameterName, false);

            var dialogResult = WindowsFormWPFHelper.ShowWinFormsDialog(insertTextOrFieldDialog, window);

            if (dialogResult == DialogResult.OK)
            {
                var insertedValue = insertTextOrFieldDialog.SelectedValue;

                return new InsertTextOrFieldResult
                {
                    Success = true,
                    InsertedValue = insertedValue,
                    IsSelectedFieldRequired = insertTextOrFieldDialog.IsSelectedFieldRequired(),
                    SelectedField = insertTextOrFieldDialog.GetSelectedField()
                };
            }

            return new InsertTextOrFieldResult();
        }

        public EditValueResult InvokeEditValue(EditValueRequest editValueRequest, Window window)
        {
            var editValueWindow = new EditValueWindow(editValueRequest.Value, editValueRequest.IsRegex, editValueRequest.IsMultiline);
            editValueWindow.SetMode(editValueRequest.EditMode);
            editValueWindow.SetParameterName(editValueRequest.ParameterName.Trim('%'));
            var dialogResult = WindowsFormWPFHelper.ShowWinFormsDialog(editValueWindow, window);

            if (dialogResult == DialogResult.OK)
            {
                var modifiedValue = editValueWindow.GetValue();

                var result = new EditValueResult
                {
                    Success = true,
                    Value = modifiedValue,
                    IsMaskedRegexString = editValueWindow.IsMaskedRegexString()
                };

                return result;
            }

            return new EditValueResult();
        }
    }
}
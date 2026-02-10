using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Models.DialogService;
using System.Collections;
using System.Windows;

namespace FlowBlox.UICore.Interfaces
{
    public interface IDialogService
    {
        bool? ShowWPFDialog(Window window, bool isModal = true);

        FieldSelectionResult InvokeFieldSelection(object target, FlowBlockUIAttribute flowBlockUI, Window window);

        FieldSelectionResult InvokeFieldSelection(object target, FlowBlockUIAttribute flowBlockUI, IList items, Window window);

        InsertTextOrFieldResult InvokeInsertTextOrField(BaseFlowBlock flowBlock, string parameterName, Window window);

        EditValueResult InvokeEditValue(EditValueRequest editValueRequest, Window window);
    }
}
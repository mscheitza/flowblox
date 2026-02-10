using FlowBlox.UICore.Enums;

namespace FlowBlox.UICore.Interfaces
{
    public interface IFlowBloxMessageBoxService
    {
        FlowBloxMessageBoxDialogResult ShowMessageBox(string message, string title, FlowBloxMessageBoxTypes flowBloxMessageBoxType);
    }
}

using FlowBlox.UICore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Interfaces
{
    public interface IFlowBloxMessageBoxService
    {
        FlowBloxMessageBoxDialogResult ShowMessageBox(string message, string title, FlowBloxMessageBoxTypes flowBloxMessageBoxType);
    }
}

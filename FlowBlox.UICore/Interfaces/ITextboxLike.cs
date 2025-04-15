using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlowBlox.UICore.Interfaces
{
    public interface ITextBoxLike
    {
        string Text { get; set; }
        int SelectionStart { get; set; }
        int SelectionLength { get; set; }
        void Focus();
        void AddClickHandler(MouseButtonEventHandler handler);
    }
}

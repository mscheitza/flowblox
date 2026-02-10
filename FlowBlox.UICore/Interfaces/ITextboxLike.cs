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

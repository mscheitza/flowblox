using FlowBlox.UICore.Interfaces;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Factory.Adapter
{
    public class ComboBoxAdapter : ITextBoxLike
    {
        private readonly ComboBox _comboBox;
        private int _selectionStart;
        private int _selectionLength;

        public ComboBoxAdapter(ComboBox comboBox)
        {
            _comboBox = comboBox;
        }

        public string Text
        {
            get => _comboBox.Text;
            set => _comboBox.Text = value;
        }

        public int SelectionStart
        {
            get => _selectionStart;
            set => _selectionStart = value;
        }

        public int SelectionLength
        {
            get => _selectionLength;
            set => _selectionLength = value;
        }

        public void Focus() => _comboBox.Focus();

        public void AddClickHandler(MouseButtonEventHandler handler)
        {
            _comboBox.PreviewMouseLeftButtonUp += handler;
        }
    }
}

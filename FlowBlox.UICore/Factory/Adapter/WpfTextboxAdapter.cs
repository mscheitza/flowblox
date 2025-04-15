using FlowBlox.UICore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Factory.Adapter
{
    public class WpfTextBoxAdapter : ITextBoxLike
    {
        private readonly TextBox _textBox;

        public WpfTextBoxAdapter(TextBox textBox)
        {
            _textBox = textBox;
        }

        public string Text
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        public int SelectionStart
        {
            get => _textBox.SelectionStart;
            set => _textBox.SelectionStart = value;
        }

        public int SelectionLength
        {
            get => _textBox.SelectionLength;
            set => _textBox.SelectionLength = value;
        }

        public void Focus() => _textBox.Focus();

        public void AddClickHandler(MouseButtonEventHandler handler)
        {
            _textBox.PreviewMouseLeftButtonUp += handler;
        }

        public TextBox InnerTextBox => _textBox;
    }
}

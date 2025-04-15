using FlowBlox.UICore.Interfaces;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlowBlox.UICore.Factory.Adapter
{
    public class AvalonEditAdapter : ITextBoxLike
    {
        private readonly TextEditor _editor;

        public AvalonEditAdapter(TextEditor editor)
        {
            _editor = editor;
        }

        public string Text
        {
            get => _editor.Text;
            set => _editor.Text = value;
        }

        public int SelectionStart
        {
            get => _editor.SelectionStart;
            set => _editor.SelectionStart = value;
        }

        public int SelectionLength
        {
            get => _editor.SelectionLength;
            set => _editor.SelectionLength = value;
        }

        public void Focus() => _editor.Focus();

        public void AddClickHandler(MouseButtonEventHandler handler)
        {
            _editor.PreviewMouseLeftButtonUp += handler;
        }

        public TextEditor InnerEditor => _editor;
    }
}

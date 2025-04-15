using FlowBlox.Core.Attributes;
using FlowBlox.Grid.Elements.Util;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using static ICSharpCode.AvalonEdit.Document.TextDocumentWeakEventManager;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public class AvalonEditTextBoxFactory
    {
        private readonly PropertyInfo _property;
        private readonly object _target;
        private readonly bool _readOnly;

        public AvalonEditTextBoxFactory(PropertyInfo property, object target, bool readOnly)
        {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _readOnly = readOnly;
        }

        public FrameworkElement Create(FlowBlockTextBoxAttribute textAttr)
        {
            var editor = new TextEditor
            {
                ShowLineNumbers = textAttr.MultiLine,
                IsReadOnly = _readOnly,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = textAttr.MultiLine ? double.NaN : 24,
                Padding = new Thickness(2),
                BorderThickness = new Thickness(1),
                FontFamily = new FontFamily("JetBrains Mono"), 
                FontSize = 12
            };

            editor.TextChanged += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);

            SetHighlighting(editor, textAttr.SyntaxHighlighting);

            if (_property.GetValue(_target) is string value)
                editor.Text = value;

            editor.TextChanged += (s, e) =>
            {
                if (_property.CanWrite)
                {
                    var current = _property.GetValue(_target) as string;
                    var newText = editor.Text;
                    if (current != newText)
                    {
                        _property.SetValue(_target, newText);
                    }
                }
            };

            if (!textAttr.MultiLine)
            {
                editor.PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                        e.Handled = true;
                };
            }

            return ResizableControlContainer.Create(editor);
        }

        private void SetHighlighting(TextEditor editor, string resourceOrName)
        {
            if (string.IsNullOrWhiteSpace(resourceOrName))
                return;

            var builtin = HighlightingManager.Instance.GetDefinition(resourceOrName);
            if (builtin != null)
            {
                editor.SyntaxHighlighting = builtin;
                return;
            }

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceOrName);
            if (stream == null)
                return;

            using var reader = new XmlTextReader(stream);
            var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            editor.SyntaxHighlighting = highlighting;
        }
    }
}

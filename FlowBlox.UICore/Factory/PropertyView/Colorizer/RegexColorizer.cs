using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView.Colorizer
{
    public sealed class RegexColorizer : DocumentColorizingTransformer
    {
        private readonly Regex _regex;
        private readonly Brush _foreground;
        private readonly FontWeight? _fontWeight;
        private readonly Brush _background;

        public RegexColorizer(Regex regex, Brush foreground, Brush background = null, FontWeight? fontWeight = null)
        {
            if (regex == null)
                throw new ArgumentNullException(nameof(regex));

            _regex = regex;
            _foreground = foreground;
            _background = background;
            _fontWeight = fontWeight ?? FontWeights.Normal;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (CurrentContext?.Document == null) 
                return;

            var lineText = CurrentContext.Document.GetText(line);
            foreach (Match m in _regex.Matches(lineText))
            {
                if (!m.Success) 
                    continue;
                
                int startOffset = line.Offset + m.Index;
                int endOffset = startOffset + m.Length;

                ChangeLinePart(startOffset, endOffset, element =>
                {
                    if (_foreground != null) 
                        element.TextRunProperties.SetForegroundBrush(_foreground);

                    if (_background != null) 
                        element.TextRunProperties.SetBackgroundBrush(_background);

                    if (_fontWeight.HasValue) 
                        element.TextRunProperties.SetTypeface(
                            new Typeface(element.TextRunProperties.Typeface.FontFamily,
                                         element.TextRunProperties.Typeface.Style,
                                         _fontWeight.Value,
                                         element.TextRunProperties.Typeface.Stretch));
                });
            }
        }
    }

}

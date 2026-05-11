using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace FlowBlox.UICore.Utilities
{
    public static class TextBlockLinkHelper
    {
        private static readonly Regex UrlRegex = new(@"(https?://[^\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(string),
                typeof(TextBlockLinkHelper),
                new PropertyMetadata(string.Empty, OnFormattedTextChanged));

        public static void SetFormattedText(DependencyObject element, string value) => element.SetValue(FormattedTextProperty, value);

        public static string GetFormattedText(DependencyObject element) => (string)element.GetValue(FormattedTextProperty);

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock)
                return;

            textBlock.Inlines.Clear();
            var text = e.NewValue as string ?? string.Empty;
            if (text.Length == 0)
                return;

            var lastIndex = 0;
            foreach (Match match in UrlRegex.Matches(text))
            {
                if (match.Index > lastIndex)
                {
                    textBlock.Inlines.Add(new Run(text[lastIndex..match.Index]));
                }

                var linkText = match.Value.TrimEnd('.', ',', ';', ':', ')', ']', '}');
                if (Uri.TryCreate(linkText, UriKind.Absolute, out var uri))
                {
                    var hyperlink = new Hyperlink(new Run(linkText))
                    {
                        NavigateUri = uri
                    };
                    hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                    textBlock.Inlines.Add(hyperlink);
                }
                else
                {
                    textBlock.Inlines.Add(new Run(match.Value));
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
                textBlock.Inlines.Add(new Run(text[lastIndex..]));
        }

        private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e?.Uri == null)
                return;

            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
                {
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch
            {
                // Ignore failures and keep UI stable.
            }
        }
    }
}

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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

        public static readonly DependencyProperty AppendLinkTextProperty =
            DependencyProperty.RegisterAttached(
                "AppendLinkText",
                typeof(string),
                typeof(TextBlockLinkHelper),
                new PropertyMetadata(string.Empty, OnInlineContentChanged));

        public static void SetAppendLinkText(DependencyObject element, string value) => element.SetValue(AppendLinkTextProperty, value);

        public static string GetAppendLinkText(DependencyObject element) => (string)element.GetValue(AppendLinkTextProperty);

        public static readonly DependencyProperty AppendLinkCommandProperty =
            DependencyProperty.RegisterAttached(
                "AppendLinkCommand",
                typeof(ICommand),
                typeof(TextBlockLinkHelper),
                new PropertyMetadata(null, OnInlineContentChanged));

        public static void SetAppendLinkCommand(DependencyObject element, ICommand value) => element.SetValue(AppendLinkCommandProperty, value);

        public static ICommand GetAppendLinkCommand(DependencyObject element) => (ICommand)element.GetValue(AppendLinkCommandProperty);

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => RefreshInlines(d);

        private static void OnInlineContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => RefreshInlines(d);

        private static void RefreshInlines(DependencyObject d)
        {
            if (d is not TextBlock textBlock)
                return;

            textBlock.Inlines.Clear();
            var text = GetFormattedText(textBlock) ?? string.Empty;

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

            var appendLinkText = GetAppendLinkText(textBlock);
            var appendLinkCommand = GetAppendLinkCommand(textBlock);
            if (!string.IsNullOrWhiteSpace(appendLinkText) && appendLinkCommand != null)
            {
                var normalBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x3A, 0x6E, 0xA5));
                var hoverBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x5B, 0x8F, 0xC7));
                var actionText = new Span(new Run(appendLinkText))
                {
                    Foreground = normalBrush,
                    Cursor = Cursors.Hand,
                    FontSize = 11
                };
                actionText.MouseEnter += (_, _) => actionText.Foreground = hoverBrush;
                actionText.MouseLeave += (_, _) => actionText.Foreground = normalBrush;
                actionText.MouseLeftButtonUp += (_, e) =>
                {
                    if (appendLinkCommand.CanExecute(null))
                        appendLinkCommand.Execute(null);

                    e.Handled = true;
                };

                textBlock.Inlines.Add(new Run(" "));
                textBlock.Inlines.Add(actionText);
            }
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

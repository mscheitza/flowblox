using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowBlox.Core.Util.OpenXml
{
    public enum OpenXmlTextSectionMode
    {
        [Display(Name = "OpenXmlTextSectionMode_IncludeWithTags", ResourceType = typeof(FlowBloxTexts))]
        IncludeWithTags,
        [Display(Name = "OpenXmlTextSectionMode_Include", ResourceType = typeof(FlowBloxTexts))]
        Include,
        [Display(Name = "OpenXmlTextSectionMode_Exclude", ResourceType = typeof(FlowBloxTexts))]
        Exclude
    }

    public sealed class OpenXmlTextExtractionOptions
    {
        public OpenXmlTextSectionMode HeaderMode { get; init; } = OpenXmlTextSectionMode.IncludeWithTags;
        public OpenXmlTextSectionMode BodyMode { get; init; } = OpenXmlTextSectionMode.IncludeWithTags;
        public OpenXmlTextSectionMode FooterMode { get; init; } = OpenXmlTextSectionMode.IncludeWithTags;
        public bool SplitAtPageBreaks { get; init; } = true;
    }

    public static class OpenXmlTextExtractor
    {
        public static IReadOnlyList<string> Extract(byte[] documentContent, OpenXmlTextExtractionOptions options)
        {
            ArgumentNullException.ThrowIfNull(documentContent);
            ArgumentNullException.ThrowIfNull(options);
            if (documentContent.Length == 0)
                throw new ArgumentException("The Open XML document is empty.", nameof(documentContent));

            using var stream = new MemoryStream(documentContent, writable: false);
            using var document = WordprocessingDocument.Open(stream, false);
            var mainPart = document.MainDocumentPart
                ?? throw new InvalidOperationException("The Open XML document has no main document part.");
            var body = mainPart.Document?.Body
                ?? throw new InvalidOperationException("The Open XML document has no body.");

            var headerText = ExtractPartText(mainPart.HeaderParts.Select(part => part.Header));
            var footerText = ExtractPartText(mainPart.FooterParts.Select(part => part.Footer));
            var bodyPages = ExtractBodyPages(body, options.SplitAtPageBreaks);

            return bodyPages
                .Select(pageText => ComposeResult(headerText, pageText, footerText, options))
                .ToList();
        }

        private static IReadOnlyList<string> ExtractBodyPages(Body body, bool splitAtPageBreaks)
        {
            var pages = new List<StringBuilder> { new() };
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                var paragraphText = new StringBuilder();
                foreach (var element in paragraph.Descendants())
                {
                    switch (element)
                    {
                        case Text text:
                            paragraphText.Append(text.Text);
                            break;
                        case TabChar:
                            paragraphText.Append('\t');
                            break;
                        case CarriageReturn:
                            paragraphText.AppendLine();
                            break;
                        case Break lineBreak when splitAtPageBreaks && lineBreak.Type?.Value == BreakValues.Page:
                        case LastRenderedPageBreak when splitAtPageBreaks:
                            AppendParagraph(pages[^1], paragraphText);
                            paragraphText.Clear();
                            pages.Add(new StringBuilder());
                            break;
                        case Break:
                            paragraphText.AppendLine();
                            break;
                    }
                }

                AppendParagraph(pages[^1], paragraphText);
            }

            return pages.Select(TrimTrailingLineBreaks).ToList();
        }

        private static string ExtractPartText(IEnumerable<OpenXmlPartRootElement> roots)
        {
            var text = new StringBuilder();
            foreach (var paragraph in roots
                         .Where(root => root != null)
                         .SelectMany(root => root.Descendants<Paragraph>()))
            {
                AppendParagraph(text, ExtractParagraphText(paragraph));
            }

            return TrimTrailingLineBreaks(text);
        }

        private static StringBuilder ExtractParagraphText(Paragraph paragraph)
        {
            var text = new StringBuilder();
            foreach (var element in paragraph.Descendants())
            {
                switch (element)
                {
                    case Text textElement:
                        text.Append(textElement.Text);
                        break;
                    case TabChar:
                        text.Append('\t');
                        break;
                    case Break:
                    case CarriageReturn:
                        text.AppendLine();
                        break;
                }
            }

            return text;
        }

        private static string ComposeResult(
            string headerText,
            string bodyText,
            string footerText,
            OpenXmlTextExtractionOptions options)
        {
            var sections = new List<string>();
            AddSection(sections, "header", headerText, options.HeaderMode, includeWhenEmpty: false);
            AddSection(sections, "body", bodyText, options.BodyMode, includeWhenEmpty: true);
            AddSection(sections, "footer", footerText, options.FooterMode, includeWhenEmpty: false);
            return string.Join(Environment.NewLine, sections);
        }

        private static void AddSection(
            ICollection<string> sections,
            string tagName,
            string content,
            OpenXmlTextSectionMode mode,
            bool includeWhenEmpty)
        {
            if (mode == OpenXmlTextSectionMode.Exclude || (!includeWhenEmpty && string.IsNullOrEmpty(content)))
                return;

            sections.Add(mode == OpenXmlTextSectionMode.IncludeWithTags
                ? $"<{tagName}>{Environment.NewLine}{content}{Environment.NewLine}</{tagName}>"
                : content);
        }

        private static void AppendParagraph(StringBuilder target, StringBuilder paragraph)
        {
            if (target.Length > 0 && target[^1] != '\n')
                target.AppendLine();
            target.Append(paragraph);
            target.AppendLine();
        }

        private static string TrimTrailingLineBreaks(StringBuilder text) =>
            text.ToString().TrimEnd('\r', '\n');
    }
}

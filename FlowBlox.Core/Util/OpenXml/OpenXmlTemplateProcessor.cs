using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Util.OpenXml
{
    public static class OpenXmlTemplateProcessor
    {
        private static readonly Regex PlaceholderRegex = new(
            BaseFlowBlock.Regex_FullyQualifiedFieldNames,
            RegexOptions.Compiled);

        public static byte[] ReplacePlaceholders(byte[] templateContent, Func<string, string> placeholderResolver)
        {
            ArgumentNullException.ThrowIfNull(templateContent);
            ArgumentNullException.ThrowIfNull(placeholderResolver);

            if (templateContent.Length == 0)
                throw new ArgumentException("The Open XML template is empty.", nameof(templateContent));

            using var stream = new MemoryStream();
            stream.Write(templateContent);
            stream.Position = 0;

            using (var document = WordprocessingDocument.Open(stream, true))
            {
                var mainPart = document.MainDocumentPart
                    ?? throw new InvalidOperationException("The Open XML template has no main document part.");

                ReplaceInRoot(mainPart.Document, placeholderResolver);
                mainPart.Document.Save();

                foreach (var headerPart in mainPart.HeaderParts)
                {
                    if (headerPart.Header == null)
                        continue;

                    ReplaceInRoot(headerPart.Header, placeholderResolver);
                    headerPart.Header.Save();
                }

                foreach (var footerPart in mainPart.FooterParts)
                {
                    if (footerPart.Footer == null)
                        continue;

                    ReplaceInRoot(footerPart.Footer, placeholderResolver);
                    footerPart.Footer.Save();
                }
            }

            return stream.ToArray();
        }

        private static void ReplaceInRoot(OpenXmlElement root, Func<string, string> placeholderResolver)
        {
            foreach (var paragraph in root.Descendants<Paragraph>())
                ReplaceInParagraph(paragraph, placeholderResolver);
        }

        private static void ReplaceInParagraph(Paragraph paragraph, Func<string, string> placeholderResolver)
        {
            var textNodes = paragraph.Descendants<Text>().ToList();
            if (textNodes.Count == 0)
                return;

            var combinedText = string.Concat(textNodes.Select(x => x.Text));
            var matches = PlaceholderRegex.Matches(combinedText);

            for (var matchIndex = matches.Count - 1; matchIndex >= 0; matchIndex--)
            {
                var match = matches[matchIndex];
                var replacement = placeholderResolver(match.Value) ?? string.Empty;
                if (string.Equals(match.Value, replacement, StringComparison.Ordinal))
                    continue;

                ReplaceRange(textNodes, match.Index, match.Length, replacement);
            }
        }

        private static void ReplaceRange(IReadOnlyList<Text> textNodes, int startIndex, int length, string replacement)
        {
            var endIndex = startIndex + length;
            var currentIndex = 0;
            var startNodeIndex = -1;
            var endNodeIndex = -1;
            var startOffset = 0;
            var endOffset = 0;

            for (var index = 0; index < textNodes.Count; index++)
            {
                var nodeLength = textNodes[index].Text?.Length ?? 0;
                var nodeEndIndex = currentIndex + nodeLength;

                if (startNodeIndex < 0 && startIndex < nodeEndIndex)
                {
                    startNodeIndex = index;
                    startOffset = startIndex - currentIndex;
                }

                if (endIndex <= nodeEndIndex)
                {
                    endNodeIndex = index;
                    endOffset = endIndex - currentIndex;
                    break;
                }

                currentIndex = nodeEndIndex;
            }

            if (startNodeIndex < 0 || endNodeIndex < 0)
                return;

            var startText = textNodes[startNodeIndex].Text ?? string.Empty;
            var endText = textNodes[endNodeIndex].Text ?? string.Empty;
            var prefix = startText[..startOffset];
            var suffix = endText[endOffset..];

            if (startNodeIndex == endNodeIndex)
            {
                SetText(textNodes[startNodeIndex], prefix + replacement + suffix);
                return;
            }

            SetText(textNodes[startNodeIndex], prefix + replacement);
            for (var index = startNodeIndex + 1; index < endNodeIndex; index++)
                SetText(textNodes[index], string.Empty);
            SetText(textNodes[endNodeIndex], suffix);
        }

        private static void SetText(Text textNode, string value)
        {
            textNode.Text = value;
            textNode.Space = value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
                ? SpaceProcessingModeValues.Preserve
                : null;
        }
    }
}

using FlowBlox.Core.Util.OpenXml;

namespace FlowBlox.AIAssistant.Tools.Converter
{
    internal static class Docx2TextConverter
    {
        public const string Name = "Docx2TextConverter";

        public static string Convert(byte[] docxBytes)
        {
            var pages = OpenXmlTextExtractor.Extract(
                docxBytes ?? Array.Empty<byte>(),
                new OpenXmlTextExtractionOptions
                {
                    HeaderMode = OpenXmlTextSectionMode.IncludeWithTags,
                    BodyMode = OpenXmlTextSectionMode.IncludeWithTags,
                    FooterMode = OpenXmlTextSectionMode.IncludeWithTags,
                    SplitAtPageBreaks = true
                });

            return string.Join(
                Environment.NewLine,
                pages.Select((page, index) =>
                    $"<page number=\"{index + 1}\">{Environment.NewLine}{page}{Environment.NewLine}</page>"));
        }
    }
}

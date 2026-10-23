using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FlowBlox.UICore.Formatting;

internal sealed class XmlContentFormattingProvider : IContentFormattingProvider
{
    public string Format(string content)
    {
        try
        {
            var document = XDocument.Parse(content ?? string.Empty);
            using var output = new Utf8StringWriter(new StringBuilder());
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = document.Declaration == null
            };
            using (var writer = XmlWriter.Create(output, settings))
                document.Save(writer);
            return output.ToString();
        }
        catch (XmlException exception)
        {
            throw new FormatException("The content is not valid XML.", exception);
        }
    }

    private sealed class Utf8StringWriter(StringBuilder builder) : StringWriter(builder)
    {
        public override Encoding Encoding => System.Text.Encoding.UTF8;
    }
}

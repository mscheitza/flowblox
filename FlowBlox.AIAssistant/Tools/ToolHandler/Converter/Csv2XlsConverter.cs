using System.Text;
using System.Xml;

namespace FlowBlox.AIAssistant.Tools.ToolHandler.Converter
{
    internal sealed class Csv2XlsConverter : CsvToExcelConverterBase
    {
        public const string Name = "Csv2XlsConverter";

        public static byte[] Convert(string csvContent)
        {
            var rows = ParseCsvRows(csvContent ?? string.Empty);

            using var memoryStream = new MemoryStream();
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = true,
                OmitXmlDeclaration = false
            };

            using (var writer = XmlWriter.Create(memoryStream, settings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("Workbook", "urn:schemas-microsoft-com:office:spreadsheet");
                writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
                writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
                writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");
                writer.WriteAttributeString("xmlns", "html", null, "http://www.w3.org/TR/REC-html40");

                writer.WriteStartElement("Worksheet");
                writer.WriteAttributeString("ss", "Name", "urn:schemas-microsoft-com:office:spreadsheet", "Sheet1");

                writer.WriteStartElement("Table");
                foreach (var row in rows)
                {
                    writer.WriteStartElement("Row");
                    foreach (var cell in row)
                    {
                        writer.WriteStartElement("Cell");
                        writer.WriteStartElement("Data");
                        writer.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet", "String");
                        writer.WriteString(cell ?? string.Empty);
                        writer.WriteEndElement(); // Data
                        writer.WriteEndElement(); // Cell
                    }

                    writer.WriteEndElement(); // Row
                }

                writer.WriteEndElement(); // Table
                writer.WriteEndElement(); // Worksheet
                writer.WriteEndElement(); // Workbook
                writer.WriteEndDocument();
            }

            return memoryStream.ToArray();
        }
    }
}

using OfficeOpenXml;

namespace FlowBlox.AIAssistant.Tools.ToolHandler.Converter
{
    internal sealed class Csv2XlsxConverter : CsvToExcelConverterBase
    {
        public const string Name = "Csv2XlsxConverter";

        public static byte[] Convert(string csvContent)
        {
            ConfigureEpplusLicense();

            var rows = ParseCsvRows(csvContent ?? string.Empty);

            using var memoryStream = new MemoryStream();
            using (var package = new ExcelPackage(memoryStream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    var row = rows[rowIndex];
                    for (var colIndex = 0; colIndex < row.Count; colIndex++)
                    {
                        worksheet.Cells[rowIndex + 1, colIndex + 1].Value = row[colIndex];
                    }
                }

                package.Save();
            }

            return memoryStream.ToArray();
        }
    }
}

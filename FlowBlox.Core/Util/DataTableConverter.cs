using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ude;
using ZstdSharp.Unsafe;

namespace FlowBlox.Core.Util
{
    public class DataTableConverter
    {
        private string _cellSeparator;
        private Encoding _encoding;

        static DataTableConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private string ReplaceSpecialCharacters(string cellSeparator)
        {
            return cellSeparator?
                .Replace("\\t", "\t")
                .Replace("\\r", "\r")
                .Replace("\\n", "\n");
        }

        public DataTableConverter(string cellSeparator, Encoding encoding)
        {
            _cellSeparator = ReplaceSpecialCharacters(cellSeparator);
            _encoding = encoding;
        }

        public DataTable GetDataTableFromCsv(Stream memoryStream, bool hasHeader)
        {
            DataTable dt = new DataTable();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _cellSeparator,
                HasHeaderRecord = hasHeader,
                BadDataFound = null,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(memoryStream))
            using (var csv = new CsvReader(reader, config))
            {
                if (hasHeader)
                {
                    LoadDataWithHeader(csv, dt);
                }
                else
                {
                    LoadDataWithoutHeader(csv, dt);
                }
            }

            return dt;
        }

        private static void LoadDataWithHeader(CsvReader csv, DataTable dt)
        {
            bool isHeaderLoaded = false;
            int columnCount = 0;

            while (csv.Read())
            {
                if (!isHeaderLoaded)
                {
                    csv.ReadHeader();
                    var header = csv.HeaderRecord;
                    columnCount = header.Length;
                    foreach (var headerName in header)
                    {
                        dt.Columns.Add(headerName);
                    }
                    isHeaderLoaded = true;
                }
                else
                {
                    if (csv.Parser.Count == columnCount)
                    {
                        var row = dt.NewRow();
                        for (int i = 0; i < columnCount; i++)
                        {
                            row[i] = csv.GetField(i);
                        }
                        dt.Rows.Add(row);
                    }
                }
            }
        }

        private static void LoadDataWithoutHeader(CsvReader csv, DataTable dt)
        {
            if (csv.Read())
            {
                // Create columns from the first row
                for (int i = 0; i < csv.Parser.Count; i++)
                {
                    dt.Columns.Add("Column" + i);
                }

                // Add first row to data table
                var row = dt.NewRow();
                for (int i = 0; i < csv.Parser.Count; i++)
                {
                    row[i] = csv.GetField(i);
                }
                dt.Rows.Add(row);
            }

            while (csv.Read())
            {
                var row = dt.NewRow();
                for (int i = 0; i < csv.Parser.Count; i++)
                {
                    row[i] = csv.GetField(i);
                }
                dt.Rows.Add(row);
            }
        }

        public void DataTableToCsv(Stream memoryStream, DataTable dataTable)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _cellSeparator,
                Encoding = _encoding
            };

            using (var writer = new StreamWriter(memoryStream, _encoding))
            {
                WriteDataTableToStream(writer, dataTable, config);
            }
        }

        private static void WriteDataTableToStream(TextWriter writer, DataTable dataTable, CsvConfiguration config)
        {
            using (var csv = new CsvWriter(writer, config))
            {
                // Header
                foreach (DataColumn column in dataTable.Columns)
                {
                    csv.WriteField(column.ColumnName);
                }
                csv.NextRecord();

                // Rows
                foreach (DataRow row in dataTable.Rows)
                {
                    for (var i = 0; i < dataTable.Columns.Count; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }
    }
}

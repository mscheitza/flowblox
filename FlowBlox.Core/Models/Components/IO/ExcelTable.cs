using FlowBlox.Core.Attributes;
using FlowBlox.Core.Attributes.FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using OfficeOpenXml;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace FlowBlox.Core.Models.Components.IO
{
    [Display(Name = "TypeNames_ExcelTable", ResourceType = typeof(FlowBloxTexts))]
    public class ExcelTable : DataTableBase
    {
        public ExcelTable() : base() 
        {

        }

        public override DataTable Read()
        {
            if (DataSource == null || !DataSource.CanRead())
            {
                throw new InvalidOperationException("The data source is not ready for reading.");
            }

            using (var memoryStream = new MemoryStream(DataSource.Content))
            {
                var dataTable = new DataTable();
                using (var package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                    {
                        dataTable.Columns.Add(FirstRowHeader ? firstRowCell.Text : $"Column {firstRowCell.Start.Column}");
                    }

                    var startRow = FirstRowHeader ? 2 : 1;
                    for (int rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = worksheet.Cells[rowNum, 1, rowNum, worksheet.Dimension.End.Column];
                        DataRow row = dataTable.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                }
                return dataTable;
            }
        }

        public override void Write(DataTable dataTable)
        {
            if (DataSource == null)
            {
                throw new InvalidOperationException("The data source is not ready for writing.");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
                    {
                        worksheet.Cells[1, colIndex + 1].Value = dataTable.Columns[colIndex].ColumnName;
                    }

                    for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                    {
                        for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
                        {
                            var value = dataTable.Rows[rowIndex][colIndex];
                            Type type = dataTable.Columns[colIndex].DataType;

                            if (type == typeof(string))
                            {
                                worksheet.Cells[rowIndex + 2, colIndex + 1].Value = value.ToString();
                                worksheet.Cells[rowIndex + 2, colIndex + 1].Style.Numberformat.Format = "@";
                            }
                            else
                            {
                                worksheet.Cells[rowIndex + 2, colIndex + 1].Value = value;
                            }
                        }
                    }

                    package.Save();
                }

                DataSource.Content = memoryStream.ToArray();
            }
        }

        public override void OptionsInit(List<OptionElement> defaults)
        {
            defaults.Add(new OptionElement(
                "EPPlus.LicenseType",
                "NonCommercialPersonal",
                "Specify the license type: 'NonCommercialOrganization', 'NonCommercialPersonal', or 'Commercial'",
                OptionElement.OptionType.Text));

            defaults.Add(new OptionElement(
                "EPPlus.LicenseKeyOrName",
                Environment.UserName,
                "Optional license key or licensee name, depending on the license model.",
                OptionElement.OptionType.Text));

            base.OptionsInit(defaults);
        }

        public override void RuntimeStarted(BaseRuntime runtime)
        {
            var options = FlowBloxOptions.GetOptionInstance().OptionCollection;
            var licenseKeyOrName = options["EPPlus.LicenseKeyOrName"].Value;
            var licenseType = options["EPPlus.LicenseType"].Value?.Trim()?.ToLowerInvariant();
            switch (licenseType)
            {
                case "noncommercialorganization":
                    ExcelPackage.License.SetNonCommercialOrganization(licenseKeyOrName);
                    break;
                case "noncommercialpersonal":
                    ExcelPackage.License.SetNonCommercialPersonal(licenseKeyOrName);
                    break;

                case "commercial":
                    ExcelPackage.License.SetCommercial(licenseKeyOrName);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported EPPlus license type '{licenseType}'. Valid values are 'noncommercialorganization', 'noncommercialpersonal' or 'Commercial'.");
            }

            base.RuntimeStarted(runtime);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FlowBlox.Core.Extensions
{
    public static class DataGridViewExtensions
    {
        public static void CopyCellValue(this DataGridView dataGridView, string fromColumnName, string toColumnName, IDictionary<object, object> valueOverrides = null, string displayMember = null)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                CopyCellValueForRow(row, fromColumnName, toColumnName, valueOverrides, displayMember);
            }
        }

        public static void CopyCellValueForRow(this DataGridViewRow row, string fromColumnName, string toColumnName, IDictionary<object, object> valueOverrides = null, string displayMember = null)
        {
            if (row.DataGridView.Columns[fromColumnName] != null && row.DataGridView.Columns[toColumnName] != null && !row.IsNewRow)
            {
                // Überprüfe, ob ein überschreibender Wert im Dictionary vorhanden ist
                if (valueOverrides != null && row.Cells[fromColumnName].Value != null && valueOverrides.TryGetValue(row.Cells[fromColumnName].Value, out object overrideValue))
                {
                    // Verwende den Wert aus dem Dictionary
                    row.Cells[toColumnName].Value = overrideValue;
                }
                else if (displayMember != null && row.Cells[fromColumnName].Value != null)
                {
                    // Verwende den Anzeigenamen
                    var value = row.Cells[fromColumnName].Value;
                    var property = value.GetType().GetProperty(displayMember);
                    row.Cells[toColumnName].Value = (string)property?.GetValue(value);
                }
                else
                {
                    // Verwende den Standardwert aus der Zelle
                    var value = row.Cells[fromColumnName].Value;
                    if (value == null)
                        row.Cells[toColumnName].Value = DBNull.Value;
                    else
                        row.Cells[toColumnName].Value = value;
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.Controls
{
    public class ColumnWidthCalculatedEventArgs : EventArgs
    {
        public ColumnHeader Column { get; }
        public int ColumnIndex { get; }
        public int CalculatedWidth { get; }

        public ColumnWidthCalculatedEventArgs(ColumnHeader column, int columnIndex, int calculatedWidth)
        {
            Column = column;
            ColumnIndex = columnIndex;
            CalculatedWidth = calculatedWidth;
        }
    }

    public class ListViewColumnAdjustmentHandler
    {
        private const int DefaultColumnWidth = 200;

        private ListView _listView;
        private Form _form;

        public event EventHandler<ColumnWidthCalculatedEventArgs> OnColumnWidthCalculated;

        public static ListViewColumnAdjustmentHandler Register(ListView listView) => new ListViewColumnAdjustmentHandler(listView);

        private ListViewColumnAdjustmentHandler(ListView listView)
        {
            _listView = listView;
            _listView.Layout += _listView_Layout;
        }
        
        private void _listView_Layout(object sender, LayoutEventArgs e)
        {
            if (_form == null)
                _form = _listView.FindForm();

            if (_form != null)
            {
                _form.Shown -= _form_Shown;
                _form.Shown += _form_Shown;

                _form.Layout -= _form_Layout;
                _form.Layout += _form_Layout;
            }
        }

        private void _form_Layout(object sender, LayoutEventArgs e) => AdjustListViewColumns();

        private void _form_Shown(object sender, EventArgs e) => AdjustListViewColumns();

        public void AdjustListViewColumns()
        {
            if (_listView.Columns.Count == 0)
                return;

            if (_listView.Width == 0)
                return;

            _listView.SuspendLayout();
            AdjustListViewColumnWidthByContent(_listView);
            _listView.ResumeLayout();
        }

        private readonly Dictionary<int, Font> _overrideFonts = new Dictionary<int, Font>();
        public void ApplyColumnFont(int index, Font font)
        {
            if (index < 0) 
                return;

            if (font == null)
                _overrideFonts.Remove(index);
            else
                _overrideFonts[index] = font;
        }

        private void AdjustListViewColumnWidthByContent(ListView listView)
        {
            var columnIndexToWidth = new Dictionary<int, int>();
            for (int columnIndex = 0; columnIndex < listView.Columns.Count; columnIndex++)
            {
                columnIndexToWidth[columnIndex] = DefaultColumnWidth;
            }

            if (listView.Items.Count == 0 && listView.Columns.Count == 0)
                return;

            int totalWidth = listView.ClientRectangle.Width;

            // Calculate the new column widths based on the content of the items
            foreach (ListViewItem item in listView.Items)
            {
                for (int i = 0; i < listView.Columns.Count - 1; i++)
                {
                    Font measureFont;
                    if (_overrideFonts.TryGetValue(i, out var overrideFont) && overrideFont != null)
                        measureFont = overrideFont;
                    else
                        measureFont = item.SubItems[i].Font;

                    int width = TextRenderer.MeasureText(item.SubItems[i].Text, measureFont).Width + 10;
                    if (width > columnIndexToWidth[i])
                    {
                        columnIndexToWidth[i] = width;
                    }
                }
            }

            // Trigger event (for all columns except the last fill column)
            for (int i = 0; i < listView.Columns.Count - 1; i++)
            {
                var col = listView.Columns[i];
                int calc = columnIndexToWidth[i];
                OnColumnWidthCalculated?.Invoke(this, new ColumnWidthCalculatedEventArgs(col, i, calc));
            }

            // Also include column header names in the width calculation
            for (int i = 0; i < listView.Columns.Count - 1; i++)
            {
                int headerWidth = TextRenderer.MeasureText(listView.Columns[i].Text, listView.Font).Width + 10;
                if (headerWidth > columnIndexToWidth[i])
                {
                    columnIndexToWidth[i] = headerWidth;
                }
            }

            int leftWidth = totalWidth;

            // Apply the calculated column widths, except for the last column
            for (int i = 0; i < listView.Columns.Count - 1; i++)
            {
                if (listView.Columns[i].Width == 0)
                    continue;

                listView.Columns[i].Width = columnIndexToWidth[i];
                leftWidth -= columnIndexToWidth[i];
            }

            // Set the width of the last column so that it takes up the remaining space
            listView.Columns[listView.Columns.Count - 1].Width = leftWidth > 0 ? leftWidth : DefaultColumnWidth;
        }
    }
}

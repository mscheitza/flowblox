using System.Collections;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.Controls
{
    internal class ListViewItemComparer : IComparer
    {
        private int col;
        private SortOrder sortOrder;
        public ListViewItemComparer(int column, SortOrder sortOrder)
        {
            col = column;
            this.sortOrder = sortOrder;
        }
        public int Compare(object x, object y)
        {
            int returnVal = string.Compare(((ListViewItem)x).SubItems[col].Text,
((ListViewItem)y).SubItems[col].Text);
            if (sortOrder == SortOrder.Descending) returnVal *= -1;
            return returnVal;
        }
    }
}

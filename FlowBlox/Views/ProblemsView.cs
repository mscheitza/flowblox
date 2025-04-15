using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Util.WPF;
using FlowBlox.UICore.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.Views
{
    public partial class ProblemsView : UserControl
    {
        public ProblemsView()
        {
            InitializeComponent();
            FlowBloxUILocalizationUtil.Localize(this);
            ListViewHelper.EnableDoubleBuffer(listView);
        }

        public void Append(ProblemTrace problemTrace)
        {
            ListViewItem lvItemProblemTrace = new ListViewItem();
            lvItemProblemTrace.Name = problemTrace.Timestamp.ToString();
            lvItemProblemTrace.Text = problemTrace.Timestamp.ToString();
            lvItemProblemTrace.Tag = problemTrace;
            lvItemProblemTrace.SubItems.Add(problemTrace.Name);
            lvItemProblemTrace.SubItems.Add(problemTrace.Criticality);
            lvItemProblemTrace.SubItems.Add(problemTrace.Message);
            lvItemProblemTrace.SubItems.Add(problemTrace.Exception?.ToString());
            listView.Items.Add(lvItemProblemTrace);
        }

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                var selectedItem = listView.SelectedItems[0];
                var currentlySelectedProblemTrace = (ProblemTrace)selectedItem.Tag;
                var problemTraceWindow = new ProblemTraceWindow(currentlySelectedProblemTrace);
                WindowsFormWPFHelper.ShowDialog(problemTraceWindow, this);
            }
        }
    }
}

using FlowBlox.Core;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FlowBlox.Views
{
    public partial class FieldSelectionWindow : Form
    {
        public List<FieldElement> SelectedFields { get; private set; } = new List<FieldElement>();

        public bool MultiSelect
        {
            get
            {
                return listViewFields.MultiSelect;
            }
            set
            {
                this.listViewFields.MultiSelect = value;
            }
        }

        public bool IsRequired
        {
            get
            {
                return cbRequired.Checked;
            }
            set
            {
                cbRequired.Checked = value;
            }
        }

        public bool HideRequired
        {
            get
            {
                return !cbRequired.Visible;
            }
            set
            {
                cbRequired.Visible = !value;
            }
        }

        protected List<BaseResultFlowBlock> elements = null;

        private FlowBloxRegistry _registry;

        public FieldSelectionWindow(BaseFlowBlock flowBlock) : this()
        {
            Initialize(flowBlock);
        }

        public FieldSelectionWindow(object target, IList items) : this()
        {
            if (items == null)
                Initialize(target as BaseFlowBlock);
            else
                Initialize(target as BaseFlowBlock, items.OfType<FieldElement>());
        }

        public FieldSelectionWindow()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
            _registry = FlowBloxRegistryProvider.GetRegistry();
        }

        private void Initialize(BaseFlowBlock flowBlock, IEnumerable<FieldElement> fieldElements)
        {
            if (flowBlock != null)
                fieldElements = fieldElements.OrderByDescending(x => flowBlock.ReferencedFlowBlocks.Contains(x.Source));

            // If no field elements were passed, load them from the registry
            if (fieldElements == null || !fieldElements.Any())
                fieldElements = _registry.GetFieldElements();

            // Append user fields
            fieldElements = fieldElements.Concat(_registry.GetUserFields())
                .Distinct();

            foreach (var fieldElement in fieldElements)
            {
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = fieldElement.Source?.Name;
                listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(listViewItem, fieldElement.Name));
                listViewItem.Tag = fieldElement;

                if (fieldElement.UserField)
                    listViewItem.ImageKey = "user";
                else if (flowBlock?.ReferencedFlowBlocks.Contains(fieldElement.Source) == true)
                    listViewItem.ImageKey = "connected";
                else
                    listViewItem.ImageKey = "disconnected";

                listViewFields.Items.Add(listViewItem);
            }
        }

        private void Initialize(BaseFlowBlock flowBlock)
        {
            IEnumerable<BaseResultFlowBlock> resultElements = _registry.GetResultFlowBlocks();
            IEnumerable<FieldElement> fieldElements;
            if (flowBlock != null)
            {
                resultElements = resultElements
                    .Except(new[] { flowBlock })
                    .Cast<BaseResultFlowBlock>();
            }
            fieldElements = resultElements.SelectMany(x => x.Fields);
            Initialize(flowBlock, fieldElements);
        }

        private void btApply_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvField in listViewFields.SelectedItems)
            {
                SelectedFields.Add((FieldElement)lvField.Tag);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void listViewSelectors_SelectedIndexChanged(object sender, EventArgs e)
        {
            listViewFields.Items.Clear();

            foreach (ListViewItem Item in listViewElements.SelectedItems)
            {
                foreach (FieldElement FieldElement in ((BaseResultFlowBlock)Item.Tag).Fields)
                {
                    ListViewItem lvFieldItem = new ListViewItem();
                    lvFieldItem.Text = FieldElement.Name;
                    lvFieldItem.Tag = FieldElement;
                    listViewFields.Items.Add(lvFieldItem);
                }
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listViewFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            btApply.Enabled = (listViewFields.SelectedItems.Count > 0);
        }

        private void listViewFields_DoubleClick(object sender, EventArgs e)
        {
            if (btApply.Enabled)
            {
                btApply_Click(sender, e);
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}

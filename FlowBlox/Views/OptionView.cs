using System;
using System.Drawing;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;

namespace FlowBlox.Views
{
    public partial class OptionView : UserControl
    {
        public OptionElement OptionElement { get; private set; } = null;

        private bool isModified = false;

        public OptionView()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
        }

        public void Initialize(OptionElement optionElement)
        {
            bool isBooleanOption =
                (optionElement.Value.ToLower().Equals("true") || optionElement.Value.ToLower().Equals("false")) ||
                (optionElement.Type == OptionElement.OptionType.Boolean);

            if (isBooleanOption && !(optionElement.Type == OptionElement.OptionType.Boolean))
                optionElement.Type = OptionElement.OptionType.Boolean;

            cbValue.Dock = DockStyle.Top;
            cbValue.Checked = isBooleanOption && optionElement.Value.ToLower().Equals("true");
            cbValue.Text = optionElement.DisplayName;
            cbValue.Visible = isBooleanOption;
            tbValue.Visible = !isBooleanOption;

            this.isModified = false;
            this.tbName.Text = optionElement.Name;
            this.tbName.ReadOnly = optionElement.SystemOption;
            this.tbValue.Text = optionElement.Value;
            this.tbDescription.Text = optionElement.Description;

            if (optionElement.Type == OptionElement.OptionType.Password)
                this.tbValue.PasswordChar = '*';
            else
                this.tbValue.PasswordChar = default;

            cbType.Items.Clear();

            Array Values = Enum.GetValues(typeof(OptionElement.OptionType));

            foreach (OptionElement.OptionType Value in Values)
            {
                cbType.Items.Add(Value.ToString());
            }

            this.cbType.Text = optionElement.Type.ToString();
            this.cbType.Enabled = !optionElement.SystemOption;
            this.tbDescription.ReadOnly = optionElement.SystemOption;
            this.OptionElement = optionElement;
            this.isModified = false;
            this.UpdateUI();
        }

        private void UpdateUI()
        {
            this.btApply.Enabled = isModified;
        }

        private void btApply_Click(object sender, EventArgs e)
        {
            try
            {
                this.OptionElement.Name = tbName.Text;
                this.OptionElement.Description = tbDescription.Text;
                this.OptionElement.Type = (OptionElement.OptionType)Enum.Parse(typeof(OptionElement.OptionType), cbType.Text);
                this.OptionElement.Value = tbValue.Text;
                this.OptionElement.Validate();
                FlowBloxOptions.GetOptionInstance().Save();
                this.isModified = false;
                UpdateUI();
            }
            catch (Exception exception)
            {
                FlowBloxMessageBox.Show(this, exception.Message);
            }
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            isModified = true;
            UpdateUI();
        }

        private void tbValue_TextChanged(object sender, EventArgs e)
        {
            isModified = true;
            UpdateUI();
        }

        private void tbContainsContent_TextChanged(object sender, EventArgs e)
        {
            isModified = true;
            UpdateUI();
        }

        private void cbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            isModified = true;
            UpdateUI();
        }

        private void cbValue_CheckedChanged(object sender, EventArgs e)
        {
            tbValue.Text = cbValue.Checked.ToString().ToLower();
        }

        private void tbDesc_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void lblValue_Click(object sender, EventArgs e)
        {

        }
    }
}

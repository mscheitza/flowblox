using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Resources;

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
            if (optionElement == null)
                return;

            // Boolean combos
            SetupYesNoCombo(cbIsPlaceholderEnabled, optionElement.IsPlaceholderEnabled);
            SetupYesNoCombo(cbBooleanValue, optionElement.GetValueBoolean());

            // Type combo
            cbType.Items.Clear();
            Array values = Enum.GetValues(typeof(OptionElement.OptionType));
            foreach (OptionElement.OptionType value in values)
            {
                cbType.Items.Add(value.ToString());
            }

            // Boolean detection
            bool isBooleanOption =
                (optionElement.Value?.ToLower().Equals("true") == true || optionElement.Value?.ToLower().Equals("false") == true) ||
                (optionElement.Type == OptionElement.OptionType.Boolean);

            if (isBooleanOption && optionElement.Type != OptionElement.OptionType.Boolean)
                optionElement.Type = OptionElement.OptionType.Boolean;

            // Bind base fields
            this.tbName.Text = optionElement.Name;
            this.tbName.ReadOnly = optionElement.SystemOption;

            this.tbValue.Text = optionElement.Value;
            this.tbDescription.Text = optionElement.Description;

            // Password masking
            this.tbValue.PasswordChar = optionElement.Type == OptionElement.OptionType.Password ? '*' : default;

            // Type selection & enable rules
            this.cbType.Text = optionElement.Type.ToString();
            this.cbType.Enabled = !optionElement.SystemOption;

            // Description editable rule (kept)
            this.tbDescription.ReadOnly = optionElement.SystemOption;

            // New flag editable rule (consistent with SystemOption pattern)
            this.cbIsPlaceholderEnabled.Enabled = !optionElement.SystemOption;

            // Value UI: boolean uses combo, otherwise textbox
            cbBooleanValue.Visible = optionElement.Type == OptionElement.OptionType.Boolean;
            tbValue.Visible = !cbBooleanValue.Visible;

            this.OptionElement = optionElement;

            this.isModified = false;
            UpdateUI();
        }

        private static void SetupYesNoCombo(ComboBox combo, bool currentValue)
        {
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.DisplayMember = "Key";
            combo.ValueMember = "Value";
            combo.Items.Clear();

            combo.Items.Add(new KeyValuePair<string, string>(
                FlowBloxResourceUtil.GetLocalizedString("OptionView_Yes", typeof(FlowBloxMainUITexts)), "true"));
            combo.Items.Add(new KeyValuePair<string, string>(
                FlowBloxResourceUtil.GetLocalizedString("OptionView_No", typeof(FlowBloxMainUITexts)), "false"));

            combo.SelectedIndex = currentValue ? 0 : 1;
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

                // Save value depending on type
                if (this.OptionElement.Type == OptionElement.OptionType.Boolean)
                {
                    var selected = cbBooleanValue.SelectedItem;
                    var value = selected is KeyValuePair<string, string> kv ? kv.Value : "false";
                    this.OptionElement.Value = value;
                }
                else
                {
                    this.OptionElement.Value = tbValue.Text;
                }

                // Save "Is Placeholder Enabled" flag
                var isPlaceholderEnabled_Selected = cbIsPlaceholderEnabled.SelectedItem;
                var isPlaceholderEnabled_Value = isPlaceholderEnabled_Selected is KeyValuePair<string, string> kvShow ? kvShow.Value : "false";
                this.OptionElement.IsPlaceholderEnabled = isPlaceholderEnabled_Value == "true";

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

            // Re-evaluate boolean UI on type change
            var type = (OptionElement.OptionType)Enum.Parse(typeof(OptionElement.OptionType), cbType.Text);
            cbBooleanValue.Visible = type == OptionElement.OptionType.Boolean;
            tbValue.Visible = !cbBooleanValue.Visible;

            UpdateUI();
        }

        private void cbBooleanValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            isModified = true;
            UpdateUI();
        }

        private void cbShowInFieldSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            isModified = true;
            UpdateUI();
        }

        private void tbDesc_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
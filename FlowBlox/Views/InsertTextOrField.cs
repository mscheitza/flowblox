using System;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Models.Components;

namespace FlowBlox.Views
{
    /// <summary>
    /// Dialog allowing the user to provide either a constant value or a runtime/user field reference
    /// for a parameter. Only one input is allowed at a time.
    /// </summary>
    public partial class InsertTextOrField : Form
    {
        private readonly BaseFlowBlock _flowBlock;

        /// <summary>
        /// The final value selected by the user:
        /// either a constant text or the fully qualified name of the selected field.
        /// </summary>
        public string SelectedValue { get; private set; } = string.Empty;

        private static readonly string _noFieldSelectedText = FlowBloxResourceUtil.GetLocalizedString("InsertTextOrField_NoFieldSelected_Text", typeof(FlowBloxMainUITexts));

        private FieldElement _selectedField;
        public FieldElement GetSelectedField()
        {
            if (tbValue.TextLength > 0)
                return null;

            if (tbSelectedField.Text == _noFieldSelectedText)
                return null;

            return _selectedField;
        }

        private bool _selectedFieldRequired;
        public bool IsSelectedFieldRequired() => _selectedFieldRequired;

        public InsertTextOrField(BaseFlowBlock flowBlock, string parameterName, bool userFieldsOnly)
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
            this.lbParameter.Text = parameterName;
            this.tbSelectedField.Text = _noFieldSelectedText;
            this._flowBlock = flowBlock;
        }

        /// <summary>
        /// Returns the value selected by the user, prioritizing constant text if provided.
        /// </summary>
        private string GetValueBySelection()
        {
            if (tbValue.TextLength > 0)
                return tbValue.Text;

            if (tbSelectedField.Text != _noFieldSelectedText)
                return tbSelectedField.Text;

            return string.Empty;
        }

        /// <summary>
        /// Applies the selection and closes the dialog.
        /// </summary>
        private void btApply_Click(object sender, EventArgs e)
        {
            this.SelectedValue = GetValueBySelection();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Closes the dialog without applying changes.
        /// </summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Updates control states based on current input (mutually exclusive).
        /// </summary>
        private void UpdateUI()
        {
            tbSelectedField.Enabled = tbValue.TextLength == 0;
            btSelectField.Enabled = tbValue.TextLength == 0;
        }

        private void tbValue_TextChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void tbSelectedField_TextChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Opens the field selection dialog and inserts the selected field (if one was selected).
        /// </summary>
        private void btSelectField_Click(object sender, EventArgs e)
        {
            var selectionDialog = new FieldSelectionWindow(_flowBlock);
            selectionDialog.ShowDialog(this);
            if (selectionDialog.SelectedFields.Count == 1)
            {
                tbSelectedField.Text = selectionDialog.SelectedFields[0].FullyQualifiedName;

                _selectedField = selectionDialog.SelectedFields[0];
                _selectedFieldRequired = selectionDialog.IsRequired;
            }
        }
    }
}
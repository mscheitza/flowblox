using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;
using FlowBlox.UICore.Enums;

namespace FlowBlox.Views
{
    public partial class EditValueWindow : Form
    {
        private string _value;

        public EditValueWindow(string value, bool isRegex, bool isMultiline)
            : this(isRegex, isMultiline)
        {
            this.Text = FlowBloxResourceUtil.GetLocalizedString("EditValueWindow_TextEditValue", typeof(FlowBloxMainUITexts));
            textBox_Value.Text = value;
            comboBox_Value.Text = value;
        }

        public EditValueWindow(bool isRegex, bool isMultiline)
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);

            this.Text = FlowBloxResourceUtil.GetLocalizedString("EditValueWindow_TextCreateValue", typeof(FlowBloxMainUITexts));

            cbMaskRegex.Visible = isRegex;
            cbMaskRegex.Checked = isRegex;
            textBox_Value.Multiline = isMultiline;

            comboBox_Value.Visible = false;
        }

        /// <summary>
        /// Gets the result text entered by the user.
        /// </summary>
        /// <returns></returns>
        public string GetValue() => _value;

        /// <summary>
        /// Whether "mask regex characters" checkbox is checked.
        /// </summary>
        /// <returns></returns>
        public bool IsMaskedRegexString() => cbMaskRegex.Checked;

        public string Title
        {
            get => Text;
            set => Text = value;
        }

        public string Description
        {
            get => lbDescription.Text;
            set => lbDescription.Text = value;
        }

        public int SelectionStart
        {
            get => textBox_Value.SelectionStart;
            set => textBox_Value.SelectionStart = value;
        }

        public int SelectionLength
        {
            get => textBox_Value.SelectionLength;
            set => textBox_Value.SelectionLength = value;
        }

        /// <summary>
        /// Enables "Continue" instead of "Cancel"
        /// </summary>
        public void ShowContinueButton()
        {
            btCancel.Text = FlowBloxResourceUtil.GetLocalizedString("EditValueWindow_TextContinue", typeof(FlowBloxMainUITexts));
            btCancel.ImageKey = "next.png";
        }

        /// <summary>
        /// Displays a list of suggestions in a ComboBox.
        /// </summary>
        /// <param name="suggestions"></param>
        /// <param name="allowUserEdit"></param>
        public void SetSuggestions(List<string> suggestions, bool allowUserEdit)
        {
            textBox_Value.Visible = false;
            comboBox_Value.Visible = true;

            string previouslySet = comboBox_Value.Text;

            comboBox_Value.DropDownStyle = allowUserEdit ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
            comboBox_Value.Items.Clear();
            comboBox_Value.Items.AddRange(suggestions.ToArray());

            if (!string.IsNullOrWhiteSpace(previouslySet) && suggestions.Contains(previouslySet))
            {
                comboBox_Value.SelectedIndex = suggestions.IndexOf(previouslySet);
            }
        }

        /// <summary>
        /// Sets a header description text.
        /// </summary>
        /// <param name="header"></param>
        public void SetHeader(string header)
        {
            lbDescription.Text = header;
        }

        /// <summary>
        /// Shows the parameter name section.
        /// </summary>
        /// <param name="parameterName"></param>
        public void SetParameterName(string parameterName)
        {
            textBox_Parameter.Text = parameterName;
            textBox_Parameter.Visible = true;
            lbParameter.Visible = true;
        }

        /// <summary>
        /// Sets a font style based on the usage scenario
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(EditMode mode)
        {
            var font = mode == EditMode.Developer
                ? new Font("JetBrains Mono", 8.25F)
                : SystemFonts.DefaultFont;

            textBox_Value.Font = font;
            comboBox_Value.Font = font;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        private void btApply_Click(object sender, EventArgs e)
        {
            string value = textBox_Value.Visible ? textBox_Value.Text : comboBox_Value.Text;

            if (string.IsNullOrWhiteSpace(value))
            {
                FlowBloxMessageBox.Show(
                    this,
                    FlowBloxResourceUtil.GetLocalizedString("EditValueWindow_TextInvalid_Message", typeof(FlowBloxMainUITexts)),
                    FlowBloxResourceUtil.GetLocalizedString("EditValueWindow_TextInvalid_Title", typeof(FlowBloxMainUITexts)),
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Info
                );
                return;
            }

            _value = value;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btAbort_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
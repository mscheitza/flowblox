using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Views;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FlowBlox.UICore.Enums;
using FlowBlox.Core.Utilities;

namespace FlowBlox.Grid.Elements.Util
{
    public static class FieldUIUtil
    {
        public static void ApplyFieldToTextBox(FieldElement fieldElement, TextBox textBox)
        {
            ApplyFieldToTextBox([fieldElement], textBox);
        }

        public static void ApplyFieldToTextBox(IEnumerable<FieldElement> fieldElements, TextBox textBox)
        {
            foreach (var fieldElement in fieldElements)
            {
                string fieldDefinition = fieldElement.FullyQualifiedName;

                if (textBox.SelectionLength > 0)
                {
                    int Index = textBox.SelectionStart;

                    textBox.Text = textBox.Text.Remove(Index, textBox.SelectionLength);
                    textBox.Text = textBox.Text.Insert(Index, fieldDefinition);
                }
                else if (textBox.SelectionStart >= 0)
                {
                    textBox.Text = textBox.Text.Insert(textBox.SelectionStart, fieldDefinition);
                }
                else
                {
                    textBox.Text += fieldDefinition;
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.ScrollToCaret();
                }
            }
        }

        private static void EditRegularExpression(TextBox textBoxControl, string fieldName)
        {
            EditValueWindow editValue = new EditValueWindow(true, true);
            editValue.SetParameterName(fieldName.Replace("%%", string.Empty));
            editValue.SetMode(EditMode.Developer);
            editValue.ShowDialog(textBoxControl.FindForm());
            if (!string.IsNullOrEmpty(editValue.GetValue()))
            {
                string text = textBoxControl.Text;
                string regexValue = editValue.IsMaskedRegexString() ? RegexUtil.EscapeRegexValue(editValue.GetValue()) : editValue.GetValue();
                text = text.Insert(textBoxControl.SelectionStart, regexValue);
                textBoxControl.Text = text;
            }
        }

        public static string GetSelectedParameter(object sender)
        {
            TextBox textBoxControl = (TextBox)sender;
            Regex regex = new Regex("(?>%%).*?(?>%%)");
            MatchCollection matchCollection = regex.Matches(textBoxControl.Text);
            foreach (Match match in matchCollection)
            {
                if ((textBoxControl.SelectionStart >= match.Index) &&
                    (textBoxControl.SelectionStart < (match.Index + match.Length)))
                {
                    textBoxControl.Text = textBoxControl.Text.Remove(match.Index, match.Length);
                    textBoxControl.SelectionStart = match.Index;
                    return match.Value;
                }
            }
            return string.Empty;
        }

        public static void RegisterRegexOnParameterSelectedAction(TextBox textBox)
        {
            textBox.Click += (sender, e) =>
            {
                string parameterName = GetSelectedParameter(textBox);
                if (!string.IsNullOrEmpty(parameterName))
                    EditRegularExpression(textBox, parameterName);
            };
        }

        public static void RegisterOnParameterSelectedAction(BaseFlowBlock flowBlock, TextBox textBox)
        {
            textBox.Click += (sender, e) =>
            {
                string parameterName = GetSelectedParameter(textBox);
                if (!string.IsNullOrEmpty(parameterName))
                {
                    InsertTextOrField insertTextOrField = new InsertTextOrField(flowBlock, parameterName, false);
                    insertTextOrField.ShowDialog(textBox.FindForm());
                    if (!insertTextOrField.SelectedValue.Equals(string.Empty))
                    {
                        string text = textBox.Text;
                        text = text.Insert(textBox.SelectionStart, insertTextOrField.SelectedValue);
                        text = text.Replace(parameterName, insertTextOrField.SelectedValue);
                        textBox.Text = text;
                    }
                }
            };
        }
    }
}

using System.Text.RegularExpressions;
using System.Windows;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.UICore.Views;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Models.DialogService;
using FlowBlox.UICore.Enums;
using FlowBlox.Core.Utilities;

namespace FlowBlox.UICore.Utilities
{
    public static class TextBoxHelper
    {
        public static void ShowFieldSelectionDialog(object target, FlowBlockUIAttribute flowBlockUI, ITextBoxLike textBox, Window window)
        {
            var dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            var fieldSelectionResult = dialogService.InvokeFieldSelection(target, flowBlockUI, window);
            if (fieldSelectionResult.Success)
            {
                // Apply field selection required option to target
                FlowBlockHelper.ApplyFieldSelectionRequiredOption(fieldSelectionResult.Target, fieldSelectionResult.SelectedFields, fieldSelectionResult.IsRequired);

                // Apply field selection to textbox
                ApplyFieldToTextBox(fieldSelectionResult.SelectedFields, textBox);
            }
        }

        public static void ApplyFieldToTextBox(FieldElement fieldElement, ITextBoxLike textBox) => ApplyFieldToTextBox(new[] { fieldElement }, textBox);

        public static void ApplyFieldToTextBox(IEnumerable<FieldElement> fieldElements, ITextBoxLike textBox)
        {
            foreach (var fieldElement in fieldElements)
            {
                string fieldDefinition = fieldElement.FullyQualifiedName;
                var selectionStart = textBox.SelectionStart;
                var selectionLength = textBox.SelectionLength;

                if (selectionLength > 0)
                {
                    textBox.Text = textBox.Text.Remove(selectionStart, selectionLength);
                    textBox.Text = textBox.Text.Insert(selectionStart, fieldDefinition);
                }
                else
                {
                    textBox.Text = textBox.Text.Insert(selectionStart, fieldDefinition);
                }

                // Set caret to the end of inserted text
                textBox.SelectionStart = selectionStart + fieldDefinition.Length;
                textBox.SelectionLength = 0;
            }
        }

        /// <summary>
        /// Opens the toolbox dialog to select a snippet and inserts it into the TextBox.
        /// </summary>
        public static void ShowToolboxDialog(ITextBoxLike textBox, string? toolboxCategory, Window owner)
        {
            var dialog = new ToolboxWindow(true, toolboxCategory)
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (dialog.ShowDialog() == true)
            {
                textBox.Text = dialog.SelectedToolboxElement.Content;
            }
        }

        /// <summary>
        /// Opens the toolbox dialog for editing a regular expression inside a selected parameter (%%param%%).
        /// </summary>
        public static void ShowEditValueDialogForRegex(ITextBoxLike textBox, string parameterLabel, Window window)
        {
            string parameterName = GetSelectedParameter(textBox);
            if (!string.IsNullOrEmpty(parameterName))
            {
                var dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
                var editValueResult = dialogService.InvokeEditValue(new EditValueRequest()
                {
                    IsRegex = true,
                    IsMultiline = true,
                    ParameterName = parameterName,
                    EditMode = EditMode.Developer
                }, window);

                if (editValueResult.Success)
                {
                    string regexValue = editValueResult.IsMaskedRegexString
                        ? RegexUtil.EscapeRegexValue(editValueResult.Value)
                        : editValueResult.Value;

                    string updatedText = textBox.Text.Remove(textBox.SelectionStart, 0);
                    updatedText = updatedText.Insert(textBox.SelectionStart, regexValue);
                    textBox.Text = updatedText;
                    textBox.SelectionStart += regexValue.Length;
                }
            }
        }

        /// <summary>
        /// Registers a click handler on the TextBox to detect and edit regex placeholders (%%parameter%%).
        /// </summary>
        public static void RegisterRegexOnParameterSelectedAction(ITextBoxLike textBox, Window window)
        {
            textBox.AddClickHandler((sender, args) =>
            {
                string parameterName = GetSelectedParameter(textBox);
                if (!string.IsNullOrEmpty(parameterName))
                {
                    ShowEditValueDialogForRegex(textBox, parameterName, window);
                    args.Handled = true;
                }
            });
        }

        /// <summary>
        /// Registers a click handler for field selection insertion when a parameter is selected.
        /// </summary>
        public static void RegisterOnParameterSelectedAction(object target, ITextBoxLike textBox, Window window)
        {
            textBox.AddClickHandler((sender, args) =>
            {
                string parameterName = GetSelectedParameter(textBox);
                if (!string.IsNullOrEmpty(parameterName))
                {
                    var dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
                    var insertTextOrFieldResult = dialogService.InvokeInsertTextOrField(target as BaseFlowBlock, parameterName, window);

                    if (insertTextOrFieldResult.Success)
                    {
                        string insertedValue = insertTextOrFieldResult.InsertedValue;
                        var parameterNameIndex = textBox.Text.IndexOf(parameterName);
                        textBox.Text = textBox.Text.Replace(parameterName, insertedValue);
                        if (!string.IsNullOrEmpty(insertedValue))
                            textBox.SelectionStart = parameterNameIndex + insertedValue.Length;
                        else
                            textBox.SelectionStart = parameterNameIndex;

                        if (insertTextOrFieldResult.SelectedField != null)
                        {
                            FlowBlockHelper.ApplyFieldSelectionRequiredOption(target, [insertTextOrFieldResult.SelectedField], insertTextOrFieldResult.IsSelectedFieldRequired);
                        }
                    }
                    args.Handled = true;
                }
            });
        }

        /// <summary>
        /// Extracts the selected %%parameter%% placeholder in the TextBox, if any.
        /// </summary>
        public static string GetSelectedParameter(ITextBoxLike textBox)
        {
            var regex = new Regex("%%.*?%%");
            var matches = regex.Matches(textBox.Text);

            foreach (Match match in matches)
            {
                if (textBox.SelectionStart >= match.Index &&
                    textBox.SelectionStart <= match.Index + match.Length)
                {
                    return match.Value;
                }
            }

            return string.Empty;
        }
    }
}

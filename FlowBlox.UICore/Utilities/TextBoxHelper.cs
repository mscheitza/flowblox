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
using FlowBlox.Core.Models.Project;
using FlowBlox.UICore.Models;


namespace FlowBlox.UICore.Utilities
{
    public static class TextBoxHelper
    {
        public static void ShowFieldSelectionDialog(
            object target,
            FlowBlockUIAttribute flowBlockUI,
            ITextBoxLike textBox,
            Window ownerWindow)
        {
            var args = new FieldSelectionWindowArgs
            {
                FlowBlock = target as BaseFlowBlock,
                SelectionMode = FieldSelectionMode.Fields,
                IsRequired = !flowBlockUI.UiOptions.HasFlag(UIOptions.FieldSelectionDefaultNotRequired),
                HideRequired = flowBlockUI.UiOptions.HasFlag(UIOptions.FieldSelectionHideRequired)
            };

            var win = new FieldSelectionWindow(args)
            {
                Owner = ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (win.ShowDialog() != true || win.Result == null)
                return;

            var result = win.Result;

            if (result.SelectionMode == FieldSelectionMode.Fields)
            {
                FlowBlockHelper.ApplyFieldSelectionRequiredOption(target, result.SelectedFields, result.IsRequired);
                ApplyFieldElementsToTextBox(result.SelectedFields, textBox);
            }
            else if (result.SelectionMode == FieldSelectionMode.ProjectProperties)
            {
                ApplyProjectPropertyElementsToTextBox(result.SelectedProjectProperties, textBox);
            }
            else
            {
                if (result.SelectionMode == FieldSelectionMode.Options)
                    ApplyOptionElementsToTextBox(result.SelectedOptions, textBox);
                else
                    ApplyInputFileElementsToTextBox(result.SelectedInputFiles, textBox);
            }
        }

        public static void ApplyInputFileElementsToTextBox(IEnumerable<FlowBloxInputFilePlaceholderElement> inputFileElements, ITextBoxLike textBox)
        {
            var defs = (inputFileElements ?? Enumerable.Empty<FlowBloxInputFilePlaceholderElement>())
                .Select(x => x?.Placeholder)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!);

            ApplyFieldDefinitionsToTextBox(defs, textBox);
        }

        public static void ApplyOptionElementsToTextBox(IEnumerable<OptionElement> optionElements, ITextBoxLike textBox)
        {
            var defs = (optionElements ?? Enumerable.Empty<OptionElement>())
                .Select(o => o.Name)
                .Select(name => $"$Options::{name}");

            ApplyFieldDefinitionsToTextBox(defs, textBox);
        }

        public static void ApplyProjectPropertyElementsToTextBox(IEnumerable<FlowBloxProjectPropertyElement> projectPropertyElements, ITextBoxLike textBox)
        {
            var defs = (projectPropertyElements ?? Enumerable.Empty<FlowBloxProjectPropertyElement>())
                .Select(p => p?.Key)
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Select(key => $"$Project::{key}");

            ApplyFieldDefinitionsToTextBox(defs, textBox);
        }

        public static void ApplyFieldElementsToTextBox(IEnumerable<FieldElement> fieldElements, ITextBoxLike textBox)
        {
            var defs = fieldElements.Select(f => f.FullyQualifiedName);
            ApplyFieldDefinitionsToTextBox(defs, textBox);
        }

        public static void ApplyFieldDefinitionsToTextBox(IEnumerable<string> fieldDefinitions, ITextBoxLike textBox)
        {
            foreach (var fieldDefinition in fieldDefinitions)
            {
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

using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Attributes.FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Factory.Adapter;
using FlowBlox.UICore.Factory.PropertyView;
using FlowBlox.UICore.Interfaces;
using MahApps.Metro.IconPacks;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FlowBlox.UICore.Resolver
{
    public class TextBoxWithOptionalButtonsCreator
    {
        private Window _window;

        public TextBoxWithOptionalButtonsCreator(Window window)
        {
            this._window = window;
        }

        private const double IconSize = 10;

        public FrameworkElement CreateTextBoxWithOptionalButtons(PropertyInfo property, object target, string displayName, FlowBlockUIAttribute flowBlockUI, Binding binding, bool readOnly)
        {
            var textAttribute = property.GetCustomAttribute<FlowBlockTextBoxAttribute>();
            FrameworkElement baseTextBox;

            if (textAttribute?.IsCodingMode == true)
            {
                var factory = new AvalonEditTextBoxFactory(property, target, readOnly);
                baseTextBox = factory.Create(textAttribute);
            }
            else if (textAttribute?.MultiLine == true)
            {
                var factory = new MultilineTextBoxFactory(property, target, readOnly);
                baseTextBox = factory.Create();
            }
            else if (textAttribute?.Suggestions == true &&
                     !string.IsNullOrWhiteSpace(textAttribute.SuggestionMember) &&
                     property.PropertyType == typeof(string))
            {
                var comboBox = new ComboBox
                {
                    IsEditable = true,
                    IsTextSearchEnabled = true,
                    IsReadOnly = readOnly,
                    IsEnabled = !readOnly
                };

                var suggestions = ResolveSuggestions(target, textAttribute.SuggestionMember);
                comboBox.ItemsSource = suggestions;
                comboBox.SetBinding(ComboBox.TextProperty, binding);
                comboBox.SelectionChanged += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);
                baseTextBox = comboBox;
            }
            else
            {
                var textBox = new TextBox
                {
                    IsReadOnly = readOnly
                };
                textBox.TextChanged += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);
                textBox.SetBinding(TextBox.TextProperty, binding);
                baseTextBox = textBox;
            }

            var grid = new System.Windows.Controls.Grid();
            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });

            var textBoxControl = GetTextboxFromFrameworkElement(baseTextBox);

            System.Windows.Controls.Grid.SetColumn(baseTextBox, 0);
            grid.Children.Add(baseTextBox);

            int columnIndex = 1;

            // File Selection
            if (flowBlockUI?.UiOptions.HasFlag(UIOptions.EnableFileSelection) == true)
            {
                var fileSelectionAttribute = property.GetCustomAttribute<FlowBlockUIFileSelectionAttribute>();
                var filter = fileSelectionAttribute?.Filter ?? "All files (*.*)|*.*";

                var fileButton = new Button
                {
                    Content = new PackIconMaterial
                    {
                        Kind = PackIconMaterialKind.FileDocumentOutline,
                        Width = IconSize,
                        Height = IconSize
                    },
                    Width = 24,
                    Height = 24,
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = FlowBloxResourceUtil.GetLocalizedString("TextBoxWithOptionalButtonsCreator_EnableFileSelection_Tooltip", typeof(FlowBloxTexts)),
                    IsEnabled = !readOnly
                };

                fileButton.Click += (s, e) =>
                {
                    var dialog = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = filter
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        property.SetValue(target, dialog.FileName);

                        if (textBoxControl is WpfTextBoxAdapter wpfAdapter)
                        {
                            var expression = wpfAdapter.InnerTextBox.GetBindingExpression(TextBox.TextProperty);
                            expression?.UpdateTarget();
                        }
                        else if (textBoxControl is AvalonEditAdapter avalonAdapter)
                        {
                            avalonAdapter.Text = property.GetValue(target)?.ToString();
                        }
                    }
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                System.Windows.Controls.Grid.SetColumn(fileButton, columnIndex++);
                grid.Children.Add(fileButton);
            }

            // Folder Selection
            if (flowBlockUI?.UiOptions.HasFlag(UIOptions.EnableFolderSelection) == true)
            {
                var folderButton = new Button
                {
                    Content = new PackIconMaterial
                    {
                        Kind = PackIconMaterialKind.FolderOutline,
                        Width = IconSize,
                        Height = IconSize
                    },
                    Width = 24,
                    Height = 24,
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = FlowBloxResourceUtil.GetLocalizedString("TextBoxWithOptionalButtonsCreator_EnableFolderSelection_Tooltip", typeof(FlowBloxTexts)),
                    IsEnabled = !readOnly
                };

                folderButton.Click += (s, e) =>
                {
                    using var dialog = new System.Windows.Forms.FolderBrowserDialog();

                    var result = dialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        property.SetValue(target, dialog.SelectedPath);

                        if (textBoxControl is WpfTextBoxAdapter wpfAdapter)
                        {
                            var expression = wpfAdapter.InnerTextBox.GetBindingExpression(TextBox.TextProperty);
                            expression?.UpdateTarget();
                        }
                        else if (textBoxControl is AvalonEditAdapter avalonAdapter)
                        {
                            avalonAdapter.Text = property.GetValue(target)?.ToString();
                        }
                    }
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                System.Windows.Controls.Grid.SetColumn(folderButton, columnIndex++);
                grid.Children.Add(folderButton);
            }

            // Field Selection
            if (flowBlockUI?.UiOptions.HasFlag(UIOptions.EnableFieldSelection) == true)
            {
                var fieldButton = new Button
                {
                    Content = new PackIconMaterial
                    {
                        Kind = PackIconMaterialKind.CursorText,
                        Width = IconSize,
                        Height = IconSize
                    },
                    Margin = new Thickness(4, 0, 0, 0),
                    Padding = new Thickness(6, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = FlowBloxResourceUtil.GetLocalizedString("TextBoxWithOptionalButtonsCreator_EnableFieldSelection_Tooltip", typeof(FlowBloxTexts)),
                    IsEnabled = !readOnly
                };

                fieldButton.Click += (s, e) =>
                {
                    Utilities.TextBoxHelper.ShowFieldSelectionDialog(target, flowBlockUI, textBoxControl, _window);
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                System.Windows.Controls.Grid.SetColumn(fieldButton, columnIndex++);
                grid.Children.Add(fieldButton);
            }

            // Toolbox
            if (flowBlockUI?.ToolboxCategory != null && !string.IsNullOrEmpty(flowBlockUI.ToolboxCategory))
            {
                var toolboxButton = new Button
                {
                    Content = new PackIconMaterial
                    {
                        Kind = PackIconMaterialKind.Tools,
                        Width = IconSize,
                        Height = IconSize
                    },
                    Margin = new Thickness(4, 0, 0, 0),
                    Padding = new Thickness(6, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = FlowBloxResourceUtil.GetLocalizedString("TextBoxWithOptionalButtonsCreator_EnableToolbox_Tooltip", typeof(FlowBloxTexts)),
                    IsEnabled = !readOnly
                };

                toolboxButton.Click += (s, e) =>
                {
                    Utilities.TextBoxHelper.ShowToolboxDialog(textBoxControl, flowBlockUI.ToolboxCategory, _window);
                };

                if (!readOnly)
                {
                    if (flowBlockUI.ToolboxCategory == nameof(FlowBloxToolboxCategory.Regex))
                    {
                        Utilities.TextBoxHelper.RegisterRegexOnParameterSelectedAction(textBoxControl, _window);
                    }
                    else if (flowBlockUI.UiOptions.HasFlag(UIOptions.EnableFieldSelection))
                    {
                        Utilities.TextBoxHelper.RegisterOnParameterSelectedAction(target, textBoxControl, _window);
                    }
                }

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                System.Windows.Controls.Grid.SetColumn(toolboxButton, columnIndex++);
                grid.Children.Add(toolboxButton);
            }

            return grid;
        }

        private ITextBoxLike GetTextboxFromFrameworkElement(FrameworkElement frameworkElement)
        {
            if (frameworkElement is TextBox textBox)
                return new WpfTextBoxAdapter(textBox);

            if (frameworkElement is ComboBox comboBox)
                return new ComboBoxAdapter(comboBox);

            if (frameworkElement is ICSharpCode.AvalonEdit.TextEditor editor)
                return new AvalonEditAdapter(editor);

            if (frameworkElement is Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is FrameworkElement fe)
                    {
                        var result = GetTextboxFromFrameworkElement(fe);
                        if (result != null)
                            return result;
                    }
                }
            }

            if (frameworkElement is System.Windows.Controls.Grid grid)
            {
                foreach (UIElement child in grid.Children)
                {
                    if (child is FrameworkElement fe)
                    {
                        var result = GetTextboxFromFrameworkElement(fe);
                        if (result != null)
                            return result;
                    }
                }
            }

            throw new InvalidOperationException("TextBox-like element could not be found in the FrameworkElement.");
        }

        private static IEnumerable<string> ResolveSuggestions(object target, string suggestionMember)
        {
            var method = target.GetType().GetMethod(suggestionMember, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null || method.GetParameters().Length != 0)
                return Array.Empty<string>();

            var result = method.Invoke(target, null);
            if (result is not IEnumerable enumerable)
                return Array.Empty<string>();

            return enumerable
                .Cast<object>()
                .Select(x => x?.ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Attributes.FlowBlox.Core.Attributes;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Factory.Adapter;
using FlowBlox.UICore.Factory.PropertyView;
using FlowBlox.UICore.Interfaces;
using MahApps.Metro.IconPacks;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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
            var fieldSelectionAttribute = property.GetCustomAttribute<FieldSelectionAttribute>();
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
                    ApplyFileDialogInitialPath(dialog, property.GetValue(target)?.ToString());

                    if (dialog.ShowDialog() == true)
                    {
                        var replacedPath = ReplacePathPrefixesWithKnownPlaceholders(dialog.FileName);
                        property.SetValue(target, replacedPath);

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
                    ApplyFolderDialogInitialPath(dialog, property.GetValue(target)?.ToString());

                    var result = dialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        var replacedPath = ReplacePathPrefixesWithKnownPlaceholders(dialog.SelectedPath);
                        property.SetValue(target, replacedPath);

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
                        Kind = PackIconMaterialKind.Variable,
                        Width = IconSize,
                        Height = IconSize,
                        Foreground = (Brush)new BrushConverter().ConvertFromString("#2F6DB3")
                    },
                    Margin = new Thickness(4, 0, 0, 0),
                    Padding = new Thickness(6, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = FlowBloxResourceUtil.GetLocalizedString("TextBoxWithOptionalButtonsCreator_EnableFieldSelection_Tooltip", typeof(FlowBloxTexts)),
                    IsEnabled = !readOnly
                };

                fieldButton.Click += (s, e) =>
                {
                    Utilities.TextBoxHelper.ShowFieldSelectionDialog(target, flowBlockUI, fieldSelectionAttribute, textBoxControl, _window);
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

        private static void ApplyFileDialogInitialPath(Microsoft.Win32.OpenFileDialog dialog, string currentValue)
        {
            if (dialog == null)
                return;

            var resolvedValue = FlowBloxFieldHelper.ReplaceFieldsInString(currentValue);
            if (string.IsNullOrWhiteSpace(resolvedValue))
                return;

            try
            {
                if (Directory.Exists(resolvedValue))
                {
                    dialog.InitialDirectory = resolvedValue;
                    return;
                }

                var directory = Path.GetDirectoryName(resolvedValue);
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
                    dialog.InitialDirectory = directory;

                var fileName = Path.GetFileName(resolvedValue);
                if (!string.IsNullOrWhiteSpace(fileName))
                    dialog.FileName = fileName;
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Error("An error occurred while determining the directory and filename from the current value.", e);
            }
        }

        private static void ApplyFolderDialogInitialPath(System.Windows.Forms.FolderBrowserDialog dialog, string currentValue)
        {
            if (dialog == null)
                return;

            var resolvedValue = FlowBloxFieldHelper.ReplaceFieldsInString(currentValue);
            if (string.IsNullOrWhiteSpace(resolvedValue))
                return;

            try
            {
                if (Directory.Exists(resolvedValue))
                {
                    dialog.SelectedPath = resolvedValue;
                    return;
                }

                var directory = Path.GetDirectoryName(resolvedValue);
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
                    dialog.SelectedPath = directory;
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Error("An error occurred while determining the directory from the current value.", e);
            }
        }

        private static string ReplacePathPrefixesWithKnownPlaceholders(string selectedPath)
        {
            if (string.IsNullOrWhiteSpace(selectedPath))
                return selectedPath;

            var candidates = new List<(string DirectoryPath, string Placeholder)>();

            var activeProject = FlowBloxProjectManager.Instance.ActiveProject;
            if (activeProject != null)
            {
                candidates.Add((activeProject.ProjectInputDirectory, "$Project::InputDirectory"));
                candidates.Add((activeProject.ProjectOutputDirectory, "$Project::OutputDirectory"));
            }

            var options = FlowBloxOptions.GetOptionInstance();
            if (options.OptionCollection.TryGetValue("Paths.InputDir", out var inputDirOption))
            {
                candidates.Add((FlowBloxFieldHelper.ReplaceFieldsInString(inputDirOption.Value), "$Options::Paths.InputDir"));
            }

            if (options.OptionCollection.TryGetValue("Paths.OutputDir", out var outputDirOption))
            {
                candidates.Add((FlowBloxFieldHelper.ReplaceFieldsInString(outputDirOption.Value), "$Options::Paths.OutputDir"));
            }

            var normalizedSelectedPath = IOUtil.NormalizePath(selectedPath, trimTrailingDirectorySeparator: true);
            if (string.IsNullOrWhiteSpace(normalizedSelectedPath))
                return selectedPath;

            foreach (var candidate in candidates)
            {
                var normalizedCandidate = IOUtil.NormalizePath(candidate.DirectoryPath, trimTrailingDirectorySeparator: true);
                if (string.IsNullOrWhiteSpace(normalizedCandidate))
                    continue;

                if (normalizedSelectedPath.Equals(normalizedCandidate, StringComparison.OrdinalIgnoreCase))
                    return candidate.Placeholder;

                var candidateWithSeparator = IOUtil.EnsureTrailingDirectorySeparator(normalizedCandidate);
                if (normalizedSelectedPath.StartsWith(candidateWithSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    var suffix = normalizedSelectedPath.Substring(candidateWithSeparator.Length);
                    return string.IsNullOrWhiteSpace(suffix)
                        ? candidate.Placeholder
                        : $"{candidate.Placeholder}\\{suffix}";
                }
            }

            return selectedPath;
        }

    }
}

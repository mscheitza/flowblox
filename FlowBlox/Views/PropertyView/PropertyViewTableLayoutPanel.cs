using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FlowBlox.Core.Attributes;
using System.Data;
using FlowBlox.Core.Util;
using System.Collections;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Attributes.FlowBlox.Core.Attributes;
using FlowBlox.Core.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Options = FlowBlox.Core.Attributes.UIOptions;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core;
using FlowBlox.UICore.Views;
using FlowBlox.Core.Util.WPF;

namespace FlowBlox.Views.PropertyView
{
    public delegate void TargetChangedEventHandler(object target, PropertyInfo property);

    public class PropertyViewTableLayoutPanel : TableLayoutPanel
    {
        public event TargetChangedEventHandler TargetChanged;

        private object _target;

        private const int DefaultControlWidth = 250;
        private const int DefaultControlHeight = 24;

        private Dictionary<PropertyInfo, Control> _propertyControls = new Dictionary<PropertyInfo, Control>();
        private Dictionary<PropertyInfo, Control> _propertyLabels = new Dictionary<PropertyInfo, Control>();
        private Dictionary<PropertyInfo, WinFormsPropertyViewControlFactory> _propertyFactories = new Dictionary<PropertyInfo, WinFormsPropertyViewControlFactory>();

        private Dictionary<Control, Color> originalBackgroundColor = new Dictionary<Control, Color>();

        private ErrorProvider errorProvider;

        public IEnumerable<WinFormsPropertyViewControlFactory> GetAssociatedFactories() => _propertyFactories.Values;

        public PropertyViewTableLayoutPanel()
        {
            ColumnCount = 1;
            AutoSize = true;
            errorProvider = new ErrorProvider();
        }

        private bool IsEnabled(object target, PropertyInfo property, bool readOnly)
        {
            var flowBlockUIAttribute = property.GetCustomAttribute<FlowBlockUIAttribute>();
            bool isWritable = typeof(IList).IsAssignableFrom(property.PropertyType) || property.CanWrite;
            return isWritable && 
                flowBlockUIAttribute?.ReadOnly != true && 
                !FlowBlockUIAttributeHelper.IsDynamicallyReadOnly(target, flowBlockUIAttribute) && 
                !readOnly;
        }

        private void AppendLabelRow(string displayName, ref int rowIndex, PropertyInfo property)
        {
            // Label
            if (!string.IsNullOrEmpty(displayName))
            {
                var uiAttribute = property.GetCustomAttribute<FlowBlockUIAttribute>();
                var highlight = uiAttribute?.Factory == UIFactory.ListView || uiAttribute?.Factory == UIFactory.GridView;
                var tag = highlight ? 
                    FlowBloxStyleTags.StyleHighlight : 
                    FlowBloxStyleTags.StyleHeader;

                var flp = new FlowLayoutPanel()
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0,3,0,3),
                    Tag = tag,
                    TabStop = false
                };

                Label label = new Label
                {
                    Text = string.Concat(displayName, ":"),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0),
                    AutoSize = true,
                    Tag = tag
                };
                flp.Controls.Add(label);

                var requiredAttribute = property.GetCustomAttribute<RequiredAttribute>();
                if (requiredAttribute != null)
                {
                    Label asteriskLabel = new Label
                    {
                        Text = "*",
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = true,
                        ForeColor = Color.Red,
                        Font = new Font("Calibri", 11.25f),
                        Margin = new Padding(0),
                        Tag = FlowBloxStyleTags.StyleIgnore
                    };
                    flp.Controls.Add(asteriskLabel);
                }

                if (uiAttribute?.Factory == UIFactory.GridView &&
                    uiAttribute?.UiOptions.HasFlag(Options.EnableFieldSelection) == true)
                {
                    var hintLabel = new Label
                    {
                        Text = FlowBloxResourceUtil.GetLocalizedString("EnableFieldSelection_Hint"),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Margin = new Padding(0),
                        AutoSize = true,
                        Tag = FlowBloxStyleTags.StyleHighlightHint
                    };
                    flp.Controls.Add(hintLabel);
                }

                Controls.Add(flp, 0, rowIndex++);
                RowStyles.Add(new RowStyle(SizeType.AutoSize));
                _propertyLabels[property] = flp;
            }
        }

        public void Initialize(object target, IEnumerable<PropertyInfo> properties, ControlAlignment controlAlignment, bool readOnly)
        {
            Controls.Clear();
            RowStyles.Clear();

            _target = target;

            int rowIndex = 0;
            int tabIndex = 0;
            foreach (var property in properties)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                var flowBlockUIAttribute = property.GetCustomAttribute<FlowBlockUIAttribute>();

                if (displayAttribute == null)
                    continue;

                if (flowBlockUIAttribute?.Visible == false)
                    continue;

                // Display name
                string displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, false);

                // UseLabel
                bool useLabel = flowBlockUIAttribute?.DisplayLabel ?? true;

                // Control
                Control control = null;

                if (flowBlockUIAttribute == null ||
                    flowBlockUIAttribute.Factory == UIFactory.Default)
                {
                    // Checkbox
                    if (property.PropertyType == typeof(bool))
                    {
                        useLabel = false;

                        var checkbox = new FlowBloxCheckBox
                        {
                            Text = displayName,
                            Checked = (bool)property.GetValue(_target),
                            Enabled = IsEnabled(_target, property, readOnly)
                        };
                        checkbox.CheckedChanged += (sender, e) =>
                        {
                            property.SetValue(_target, ((FlowBloxCheckBox)control).Checked);
                            OnValueChange(property);
                        };
                        control = checkbox;
                    }
                    // ComboBox (Enum)
                    else if (property.PropertyType.IsEnum)
                    {
                        var enumValues = Enum.GetValues(property.PropertyType).Cast<Enum>().ToList();
                        var enumValuesToLocalizedNames = enumValues.ToDictionary(
                            keySelector: enumValue => enumValue,
                            elementSelector: enumValue => enumValue.GetLocalizedEnumName());

                        var localizedNamesToEnumValues = enumValuesToLocalizedNames.ReverseDictionary();

                        var comboBox = new ComboBox
                        {
                            DataSource = enumValuesToLocalizedNames.Values.ToList(),
                            Enabled = IsEnabled(_target, property, readOnly),
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            FlatStyle = FlatStyle.Flat
                        };

                        comboBox.BindingContext = new BindingContext();
                        comboBox.SelectedItem = enumValuesToLocalizedNames[(Enum)property.GetValue(_target)];

                        SetControlWidth(flowBlockUIAttribute, comboBox);

                        comboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            var selectedEnum = localizedNamesToEnumValues[comboBox.SelectedItem.ToString()];
                            property.SetValue(_target, selectedEnum);
                            OnValueChange(property);
                        };
                        control = comboBox;
                    }
                    else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                    {
                        var flowBloxNumericTextBox = new FlowBloxNumericTextBox();

                        flowBloxNumericTextBox.InnerNumericTextBox.Text = property.GetValue(_target)?.ToString();
                        flowBloxNumericTextBox.InnerNumericTextBox.ReadOnly = !IsEnabled(_target, property, readOnly);

                        SetControlWidth(flowBlockUIAttribute, flowBloxNumericTextBox);

                        flowBloxNumericTextBox.InnerNumericTextBox.TextChanged += (sender, e) =>
                        {
                            if (int.TryParse(flowBloxNumericTextBox.InnerNumericTextBox.Text, out int result))
                                property.SetValue(_target, result);
                            else
                                property.SetValue(_target, property.PropertyType == typeof(int?) ? (int?)null : 0);

                            OnValueChange(property);
                        };

                        control = flowBloxNumericTextBox;
                    }
                    // Textbox
                    else
                    {
                        var textBoxAttribute = property.GetCustomAttribute<FlowBlockTextBoxAttribute>();

                        var flowBloxTextBox = new FlowBloxTextBox();
                        flowBloxTextBox.ReadOnly = !IsEnabled(_target, property, readOnly);

                        flowBloxTextBox.InnerTextBox.Text = property.GetValue(_target)?.ToString();
                        flowBloxTextBox.InnerTextBox.Multiline = textBoxAttribute?.MultiLine ?? false;
                        flowBloxTextBox.InnerTextBox.WordWrap = false;

                        if (textBoxAttribute?.MultiLine == true)
                        {
                            var lineCount = flowBloxTextBox.InnerTextBox.Lines.Count();
                            if (lineCount > 0)
                            {
                                flowBloxTextBox.Height = flowBloxTextBox.InnerTextBox.Lines.Count() > 5 ?
                                    5 * DefaultControlHeight : flowBloxTextBox.InnerTextBox.Lines.Count() * DefaultControlHeight;
                            }
                            else
                            {
                                flowBloxTextBox.Height = 2 * DefaultControlHeight;
                            }
                        }
                            
                        if (textBoxAttribute?.PasswordChar != null)
                            flowBloxTextBox.InnerTextBox.PasswordChar = textBoxAttribute.PasswordChar;

                        if (textBoxAttribute?.IsCodingMode == true)
                            flowBloxTextBox.EnableDeveloperMode();

                        SetControlWidth(flowBlockUIAttribute, flowBloxTextBox, textBoxAttribute);

                        flowBloxTextBox.InnerTextBox.TextChanged += (sender, e) =>
                        {
                            property.SetValue(_target, flowBloxTextBox.InnerTextBox.Text);
                            OnValueChange(property);
                        };

                        control = flowBloxTextBox;
                    }
                }
                else if (flowBlockUIAttribute?.Factory == UIFactory.Association)
                {
                    var enabled = IsEnabled(_target, property, readOnly);
                    var factory = new AssociationTextBoxFactory(property, _target, !enabled);
                    var factoryResult = factory.Create();
                    SetControlWidth(flowBlockUIAttribute, factoryResult.Item1);
                    _propertyFactories[property] = factory;
                    control = factoryResult.Item2;
                }
                else if (flowBlockUIAttribute?.Factory == UIFactory.GridView)
                {
                    var enabled = IsEnabled(_target, property, readOnly);
                    var factory = new PropertyViewDataGridFactory(property, _target, !enabled);
                    var dataGridView = factory.Create();
                    _propertyFactories[property] = factory;
                    control = dataGridView;
                }
                else if (flowBlockUIAttribute?.Factory == UIFactory.ListView)
                {
                    var enabled = IsEnabled(_target, property, readOnly);
                    var factory = new PropertyViewListViewFactory(property, _target, !enabled);
                    var listView = factory.Create();
                    _propertyFactories[property] = factory;
                    control = listView;
                }
                else if (flowBlockUIAttribute?.Factory == UIFactory.ComboBox)
                {
                    var comboBox = new ComboBox();
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

                    Type propertyType = property.PropertyType;
                    Type underlyingType = Nullable.GetUnderlyingType(propertyType);

                    var items = new List<object>();
                    Array enumValues = null;
                    if (propertyType.IsEnum)
                    {
                        enumValues = Enum.GetValues(propertyType);
                    }
                    else if (underlyingType != null && underlyingType.IsEnum)
                    {
                        enumValues = Enum.GetValues(underlyingType);
                        items.Add(new { DisplayName = string.Empty, EnumValue = (Enum)null });
                    }

                    if (enumValues != null)
                    {
                        foreach (Enum value in enumValues)
                        {
                            items.Add(new
                            {
                                DisplayName = value.GetDisplayName(),
                                EnumValue = value
                            });
                        }
                    }

                    if (enumValues != null)
                    {
                        comboBox.BindingContextChanged += (s, e) =>
                        {
                            var currentValue = property.GetValue(_target);
                            var selectedItem = items.SingleOrDefault(x => {
                                var enumValue = (Enum)x.GetType().GetProperty("EnumValue").GetValue(x);
                                return enumValue?.Equals(currentValue) == true;
                            });
                            comboBox.SelectedItem = selectedItem;
                        };

                        comboBox.DataSource = items;
                        comboBox.DisplayMember = "DisplayName";
                        comboBox.ValueMember = "EnumValue";
                    }
                    else
                    {
                        if (flowBlockUIAttribute.SelectionFilterMethod == null)
                            throw new InvalidOperationException($"Expected a filter expression at property \"{property.Name}\".");

                        var filterMethod = _target.GetType().GetMethod(flowBlockUIAttribute.SelectionFilterMethod);
                        if (filterMethod == null)
                            throw new InvalidOperationException("No filter method found.");

                        var items_filterMethod = filterMethod.Invoke(target, null) as IEnumerable;
                        comboBox.DataSource = items_filterMethod;
                        comboBox.DisplayMember = flowBlockUIAttribute.SelectionDisplayMember;
                        comboBox.SelectedItem = property.GetValue(_target);
                    }

                    comboBox.SelectedIndexChanged += (sender, e) =>
                    {
                        Type underlyingType = Nullable.GetUnderlyingType(propertyType);
                        if (propertyType.IsEnum || (underlyingType != null && underlyingType.IsEnum))
                        {
                            var selectedItem = comboBox.SelectedItem;
                            var selectedValue = selectedItem.GetType().GetProperty("EnumValue").GetValue(selectedItem);
                            property.SetValue(_target, selectedValue);
                            OnValueChange(property);
                        }
                        else
                        {
                            property.SetValue(_target, comboBox.SelectedItem);
                        }
                        OnValueChange(property);
                    };

                    comboBox.Enabled = IsEnabled(_target, property, readOnly);
                    SetControlWidth(flowBlockUIAttribute, comboBox);
                    control = comboBox;
                }
                else if (flowBlockUIAttribute?.Factory == UIFactory.ListViewSplitMode)
                {
                    throw new NotSupportedException($"The factory '{nameof(UIFactory.ListViewSplitMode)}' is not supported in WinForms context.");
                }

                // Weiterleitung des ValueChanged-Events
                if (_propertyFactories.ContainsKey(property))
                {
                    _propertyFactories[property].ControlChanged += (t, p) =>
                    {
                        OnValueChange(p);
                    };
                }

                // Label einblenden
                if (useLabel)
                    AppendLabelRow(displayName, ref rowIndex, property);

                // Höhe der Zeile einstellen
                if (flowBlockUIAttribute?.Height > 0)
                    RowStyles.Add(new RowStyle(SizeType.Absolute, flowBlockUIAttribute.Height));
                else
                    RowStyles.Add(new RowStyle(SizeType.AutoSize));

                if (control is FlowBloxTextBox)
                    control = InitializeTextBoxOptions(property, flowBlockUIAttribute, control);

                control.TabIndex = tabIndex++;
                SetControlName(control, property.Name);
                Controls.Add(control, 0, rowIndex++);

                if (property.GetCustomAttribute<ActivationConditionAttribute>()?.IsActive(_target) == false)
                {
                    control.Visible = false;
                    if (_propertyControls.TryGetValue(property, out Control labelControl))
                        labelControl.Visible = false;
                }

                control.Leave += (sender, e) =>
                {
                    Validate(property);
                };

                // Steuerelement für diese Eigenschaft speichern
                _propertyControls[property] = control;
            }

            if (controlAlignment == ControlAlignment.Top)
            {
                Controls.Add(new Control(), 0, rowIndex++);
                RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            }

            if (_target is BaseFlowBlock)
            {
                ((BaseFlowBlock)_target).PropertyChanged += (s, e) =>
                {
                    var pi = _target.GetType().GetProperty(e.PropertyName);

                    if (_propertyFactories.ContainsKey(pi))
                        _propertyFactories[pi].Reload();

                    if (_propertyControls.ContainsKey(pi))
                    {
                        if (_propertyControls[pi] is ComboBox)
                        {
                            var comboBox = (ComboBox)_propertyControls[pi];
                            var currentValue = pi.GetValue(_target);
                            if (comboBox.DataSource is List<string>)
                            {
                                var enumValues = Enum.GetValues(pi.PropertyType).Cast<Enum>().ToList();
                                var enumValuesToLocalizedNames = enumValues.ToDictionary(
                                    keySelector: enumValue => enumValue,
                                    elementSelector: enumValue => enumValue.GetLocalizedEnumName());

                                comboBox.SelectedItem = enumValuesToLocalizedNames[(Enum)pi.GetValue(_target)];
                            }
                            else
                            {
                                var items = (List<object>)comboBox.DataSource;
                                comboBox.SelectedItem = items.SingleOrDefault(x => x.GetType().GetProperty("EnumValue").GetValue(x) == currentValue);
                            }
                        }
                    }
                };
            }
        }

        private void SetControlName(Control control, string name)
        {
            if (control is FlowLayoutPanel)
                control = ((FlowLayoutPanel)control).Controls[0];

            if (control is FlowBloxTextBox)
                control = ((FlowBloxTextBox)control).InnerTextBox;
            
            control.Name = name;
        }

        private void OnValueChange(PropertyInfo property)
        {
            var dependendProperties = _target.GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<ActivationConditionAttribute>()?.MemberName == property.Name);

            foreach(var dependendProperty in dependendProperties)
            {
                var activationCondition = dependendProperty.GetCustomAttribute<ActivationConditionAttribute>();
                var control = _propertyControls.Single(x => x.Key.Name == dependendProperty.Name).Value;
                var label = _propertyLabels.Single(x => x.Key.Name == dependendProperty.Name).Value;
                bool isActive = activationCondition.IsActive(_target);
                control.Visible = isActive;
                label.Visible = isActive;
            }

            TargetChanged?.Invoke(_target, property);
        }

        private Control InitializeTextBoxOptions(PropertyInfo property, FlowBlockUIAttribute flowBlockUIAttribute, Control control)
        {
            var panel = new FlowLayoutPanel()
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            panel.Controls.Add(control);

            // Toolbox-Handling
            if (flowBlockUIAttribute?.ToolboxCategory != null && 
                !string.IsNullOrEmpty(flowBlockUIAttribute.ToolboxCategory))
            {
                var textBox = ((FlowBloxTextBox)control).InnerTextBox;

                var button = new Button
                {
                    Width = 24,
                    Image = FlowBloxMainUIImages.Toolbox_16,
                };

                button.Click += (sender, e) =>
                {
                    var dialog = new ToolboxWindow(true, flowBlockUIAttribute.ToolboxCategory);
                    var result = WindowsFormWPFHelper.ShowDialog(dialog, this.FindForm());
                    if (result.HasValue && result.Value)
                        textBox.Text = dialog.SelectedToolboxElement.Content;
                };
                panel.Controls.Add(button);

                if (flowBlockUIAttribute.ToolboxCategory == nameof(FlowBloxToolboxCategory.Regex))
                    FieldUIUtil.RegisterRegexOnParameterSelectedAction(textBox);
                else if (flowBlockUIAttribute.UiOptions.HasFlag(Options.EnableFieldSelection))
                    FieldUIUtil.RegisterOnParameterSelectedAction(_target as BaseFlowBlock, textBox);
            }

            if (flowBlockUIAttribute != null)
            {
                // File Selection
                if (flowBlockUIAttribute.UiOptions.HasFlag(UIOptions.EnableFileSelection))
                {
                    var fileSelectionAttribute = property.GetCustomAttribute<FlowBlockUIFileSelectionAttribute>();
                    var filter = fileSelectionAttribute?.Filter ?? "All files (*.*)|*.*";

                    var button = new Button
                    {
                        Text = "...",
                        Image = null,
                        Width = 24
                    };
                    button.Click += (sender, e) =>
                    {
                        using (var openFileDialog = new OpenFileDialog())
                        {
                            openFileDialog.Filter = filter;
                            if (openFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                ((FlowBloxTextBox)control).InnerTextBox.Text = openFileDialog.FileName;
                            }
                        }
                    };
                    panel.Controls.Add(button);
                }

                // Field Selection
                if (flowBlockUIAttribute.UiOptions.HasFlag(UIOptions.EnableFieldSelection))
                {
                    var button = new Button
                    {
                        Image = FlowBloxMainUIImages.add_field_16,
                        Width = 24
                    };
                    button.Click += (sender, e) =>
                    {
                        var flowBlock = _target as BaseFlowBlock;
                        var fieldSelectionWindow = new FieldSelectionWindow(flowBlock) 
                        { 
                            IsRequired = !flowBlockUIAttribute.UiOptions.HasFlag(Options.FieldSelectionDefaultNotRequired) 
                        };
                        if (fieldSelectionWindow.ShowDialog(this) == DialogResult.OK)
                        {
                            // Apply field selection required option to FlowBlock
                            FlowBlockHelper.ApplyFieldSelectionRequiredOption(flowBlock, fieldSelectionWindow.SelectedFields, fieldSelectionWindow.IsRequired);

                            // Apply field selection to textbox
                            FieldUIUtil.ApplyFieldToTextBox(fieldSelectionWindow.SelectedFields, ((FlowBloxTextBox)control).InnerTextBox);
                        }
                    };

                    panel.Controls.Add(button);
                }
            }

            if (panel.Controls.Count > 1)
                return panel;
            else
                return control;
        }

        private void SetControlWidth(FlowBlockUIAttribute flowBlockUIAttribute, Control control, FlowBlockTextBoxAttribute textBoxAttribute = null)
        {
            if (flowBlockUIAttribute != null &&
                flowBlockUIAttribute.Width > 0)
                control.Width = flowBlockUIAttribute.Width;
            else
            {
                if (textBoxAttribute?.MultiLine == true)
                    control.Width = DefaultControlWidth * 2;
                else
                    control.Width = DefaultControlWidth;
            }
        }

        private bool ValidateFactory(PropertyInfo property)
        {
            if (!_propertyFactories.ContainsKey(property))
                return true;

            var factory = _propertyFactories[property];
            var interfaceType = ReflectionHelper.GetInterfaceTypeMatchingGenericDefinition(factory.GetType(), typeof(IValidatableFactory<>));
            if (interfaceType == null)
                return true;

            MethodInfo validateMethod = interfaceType.GetMethod("Validate");
            var control = _propertyControls[property];
            return (bool)validateMethod.Invoke(factory, new object[] { control });
        }

        private bool Validate(PropertyInfo property)
        {
            if (!ValidateFactory(property))
                return false;

            var control = _propertyControls[property];
            var context = new ValidationContext(_target) { MemberName = property.Name };
            var results = new List<ValidationResult>();

            // Check whether the cell's value is valid
            var value = property.GetValue(_target);
            if (!Validator.TryValidateProperty(value, context, results))
            {
                errorProvider.Icon = Icon.FromHandle(FlowBloxMainUIImages.Error_16.GetHicon());
                errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                errorProvider.SetError(control, string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage)));
                if (!originalBackgroundColor.ContainsKey(control))
                    originalBackgroundColor[control] = control.BackColor;
                control.BackColor = Color.LightSalmon;

                // Im Falle eines FlowLayoutPanels muss ein Außenabstand zur Anzeige des Fehler-Symbols hinzugefügt werden:
                if (control is FlowLayoutPanel ||
                    control is DataGridView ||
                    control is ListView)
                {
                    var margin = control.Margin;
                    control.Margin = new Padding(margin.Left, margin.Top, 20, margin.Bottom);
                }

                return false;
            }
            else
            {
                errorProvider.SetError(control, string.Empty);

                if (originalBackgroundColor.ContainsKey(control))
                    control.BackColor = originalBackgroundColor[control];

                return true;
            }
        }

        public bool Validate(ref List<string> invalidProperties)
        {
            bool isValid = true;
            foreach (var property in _propertyControls.Keys)
            {
                if (!Validate(property))
                {
                    var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
                    var propertyLocalizedName = FlowBloxResourceUtil.GetDisplayName(displayAttr, false) ?? displayAttr.Name;
                    invalidProperties.Add(propertyLocalizedName);
                    isValid = false;
                }
            }
            return isValid;
        }
    }
}

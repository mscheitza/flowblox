using FlowBlox.Core.Attributes;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Factory.PropertyView;
using FlowBlox.UICore.ViewModels.PropertyView;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FlowBlox.UICore.Resolver
{
    public class PropertyControlResolver
    {
        private Window _window;
        private TextBoxWithOptionalButtonsCreator _textBoxWithOptionalButtonsCreator;

        public PropertyControlResolver(Window window)
        {
            this._window = window;
            this._textBoxWithOptionalButtonsCreator = new TextBoxWithOptionalButtonsCreator(window);
        }

        public PropertyControlViewModel Resolve(
            PropertyInfo property, 
            object target, 
            bool contextIsReadOnly, 
            object preselectedInstance, 
            PropertyControlBindingContext bindingContext)
        {
            if (property == null) 
                throw new ArgumentNullException(nameof(property));

            if (target == null) 
                throw new ArgumentNullException(nameof(target));

            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            var flowBlockUI = property.GetCustomAttribute<FlowBlockUIAttribute>();

            if (displayAttribute == null || (flowBlockUI != null && !flowBlockUI.Visible))
                return null;

            var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, false) ?? property.Name;

            string description = default;
            if (!string.IsNullOrEmpty(displayAttribute?.Description) && displayAttribute?.ResourceType != null)
                description = FlowBloxResourceUtil.GetLocalizedString(displayAttribute.Description, displayAttribute.ResourceType);

            var readOnly = contextIsReadOnly ||
                flowBlockUI?.ReadOnly == true ||
                !property.CanWrite;

            FrameworkElement control = CreateControl(property, target, displayName, flowBlockUI, readOnly, preselectedInstance);

            if (!string.IsNullOrWhiteSpace(description))
            {
                ToolTipService.SetToolTip(control, new ToolTip
                {
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    MaxWidth = 1000,
                    Content = new TextBlock
                    {
                        Text = description,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 1000
                    }
                });
            }

            bool isActive = true;
            var activationAttr = property.GetCustomAttribute<ActivationConditionAttribute>();
            if (activationAttr != null)
                isActive = activationAttr.IsActive(target);

            var labelSettings = GetLabelSettings(property, displayName, flowBlockUI);

            var propertyControlViewModel = new PropertyControlViewModel()
            {
                PropertyName = property.Name,
                Target = target,
                UseLabel = labelSettings.UseLabel,
                Label = labelSettings.LabelText,
                Value = property.GetValue(target),
                ValueType = property.PropertyType,
                IsEnabled = !FlowBlockUIAttributeHelper.IsDynamicallyReadOnly(target, flowBlockUI),
                Control = control,
                IsActive = isActive
            };

            bindingContext.Register(property, propertyControlViewModel);

            return propertyControlViewModel;
        }

        private (bool UseLabel, string LabelText) GetLabelSettings(
            PropertyInfo property,
            string displayName,
            FlowBlockUIAttribute flowBlockUI)
        {
            // If DisplayLabel is explicitly set to false, never show a label.
            bool displayLabel = flowBlockUI?.DisplayLabel ?? true;
            if (!displayLabel)
                return (false, string.Empty);

            // Special handling for boolean properties (checkboxes)
            if (property.PropertyType == typeof(bool))
            {
                var checkboxAttribute = property.GetCustomAttribute<FlowBlockCheckboxAttribute>();
                if (string.IsNullOrWhiteSpace(checkboxAttribute?.HeaderLabel))
                    return (false, string.Empty);

                var resolvedLabel = FlowBloxResourceUtil.GetLocalizedString(checkboxAttribute.HeaderLabel, checkboxAttribute.ResourceType);
                return (true, resolvedLabel);
            }
            return (true, displayName);
        }

        private FrameworkElement CreateControl(
            PropertyInfo property, 
            object target, 
            string displayName, 
            FlowBlockUIAttribute flowBlockUI, 
            bool readOnly, 
            object preselectedInstance)
        {
            var binding = new Binding(property.Name)
            {
                Source = target,
                Mode = readOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidatesOnDataErrors = true,
                ValidatesOnExceptions = true
            };

            if (property.PropertyType == typeof(bool))
            {
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                var checkBox = new CheckBox
                {
                    IsEnabled = !readOnly,
                    VerticalAlignment = VerticalAlignment.Center
                };

                checkBox.Checked += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);
                checkBox.Unchecked += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);

                checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);

                var label = new TextBlock
                {
                    Text = displayName,
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                stackPanel.Children.Add(checkBox);
                stackPanel.Children.Add(label);

                return stackPanel;
            }

            if (property.PropertyType.IsEnum || Nullable.GetUnderlyingType(property.PropertyType)?.IsEnum == true)
            {
                var enumType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var enumValues = Enum.GetValues(enumType)
                    .Cast<Enum>()
                    .Select(e => new
                    {
                        DisplayName = e.GetDisplayName(),
                        EnumValue = e
                    })
                    .ToList();

                if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    enumValues.Insert(0, new 
                    { 
                        DisplayName = string.Empty, 
                        EnumValue = (Enum)null 
                    });
                }

                var comboBox = new ComboBox
                {
                    ItemsSource = enumValues,
                    DisplayMemberPath = "DisplayName",
                    SelectedValuePath = "EnumValue"
                };

                SetComboBoxReadOnly(comboBox, readOnly);

                comboBox.SelectionChanged += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);

                comboBox.SetBinding(ComboBox.SelectedValueProperty, binding);
                return new Border 
                { 
                    Child = comboBox,
                    Background = Brushes.Transparent
                };
            }

            // Selection-Filter
            if (flowBlockUI?.Factory == UIFactory.ComboBox)
            {
                var filterMethod = !string.IsNullOrEmpty(flowBlockUI?.SelectionFilterMethod) ?
                    target.GetType().GetMethod(flowBlockUI?.SelectionFilterMethod) :
                    null;

                if (filterMethod != null)
                {
                    var originalItems = filterMethod.Invoke(target, null) as IList;
                    
                    IList items = null;

                    bool isRequired = property.GetCustomAttribute<RequiredAttribute>() != null;
                    if (originalItems != null && !isRequired)
                    {
                        items = (IList)Activator.CreateInstance(originalItems.GetType());
                        items!.Add(null);

                        foreach (var item in originalItems)
                            items.Add(item);
                    }
                    else
                    {
                        items = originalItems;
                    }

                    var comboBox = new ComboBox
                    {
                        ItemsSource = items,
                        DisplayMemberPath = flowBlockUI?.SelectionDisplayMember
                    };

                    SetComboBoxReadOnly(comboBox, readOnly);

                    comboBox.SetBinding(ComboBox.SelectedValueProperty, binding);
                    return new Border
                    {
                        Child = comboBox,
                        Background = Brushes.Transparent
                    };
                }
            }

            if (property.PropertyType == typeof(int) || 
                property.PropertyType == typeof(int?) ||
                property.PropertyType == typeof(float) ||
                property.PropertyType == typeof(float?) ||
                property.PropertyType == typeof(double) ||
                property.PropertyType == typeof(double?))
            {
                var textBox = new TextBox
                {
                    IsReadOnly = readOnly
                };
                textBox.TextChanged += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);
                textBox.SetBinding(TextBox.TextProperty, binding);
                return textBox;
            }

            if (flowBlockUI?.Factory == UIFactory.Association)
            {
                AssociationControlFactory associationControlFactory = new AssociationControlFactory(_window, property, target, readOnly);
                return associationControlFactory.Create();
            }

            if (flowBlockUI?.Factory == UIFactory.GridView)
            {
                DataGridFactory dataGridFactory = new DataGridFactory(_window, property, target, readOnly);
                dataGridFactory.SetPreselectedInstance(preselectedInstance);
                return dataGridFactory.Create();
            }

            if (flowBlockUI?.Factory == UIFactory.ListView)
            {
                ListViewFactory listViewFactory = new ListViewFactory(_window, property, target, readOnly);
                listViewFactory.SetPreselectedInstance(preselectedInstance);
                return listViewFactory.Create();
            }

            if (flowBlockUI?.Factory == UIFactory.ListViewSplitMode)
            {
                ListViewSplitModeFactory listViewSplitModeFactory = new ListViewSplitModeFactory(_window, property, target, readOnly);
                return listViewSplitModeFactory.Create();
            }

            return _textBoxWithOptionalButtonsCreator.CreateTextBoxWithOptionalButtons(property, target, displayName, flowBlockUI, binding, readOnly);
        }

        private void SetComboBoxReadOnly(ComboBox comboBox, bool readOnly)
        {
            comboBox.IsHitTestVisible = !readOnly;
            comboBox.Focusable = !readOnly;
            comboBox.IsTabStop = !readOnly;
        }
    }
}

using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Converters.PropertyView;
using FlowBlox.UICore.Factory.PropertyView;
using FlowBlox.UICore.PropertyView.Resolver;
using FlowBlox.UICore.ViewModels.PropertyView;
using MahApps.Metro.Controls;
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
        private readonly object _parent;
        private TextBoxWithOptionalButtonsCreator _textBoxWithOptionalButtonsCreator;

        public PropertyControlResolver(Window window, object parent = null)
        {
            this._window = window;
            this._parent = parent;
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
            var uiAttribute = property.GetCustomAttribute<FlowBloxUIAttribute>();

            if (displayAttribute == null || (uiAttribute != null && !uiAttribute.Visible))
                return null;

            var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, false) ?? property.Name;

            string description = default;
            if (!string.IsNullOrEmpty(displayAttribute?.Description) && displayAttribute?.ResourceType != null)
                description = FlowBloxResourceUtil.GetLocalizedString(displayAttribute.Description, displayAttribute.ResourceType);

            var readOnly = contextIsReadOnly ||
                uiAttribute?.ReadOnly == true ||
                !property.CanWrite;

            var controlResult = CreateControl(property, target, displayName, uiAttribute, readOnly, preselectedInstance);

            bool isActive = true;
            var activationAttr = property.GetCustomAttribute<ActivationConditionAttribute>();
            if (activationAttr != null)
                isActive = activationAttr.IsActive(target);

            var labelSettings = GetLabelSettings(property, displayName, uiAttribute);

            var propertyControlViewModel = new PropertyControlViewModel()
            {
                PropertyName = property.Name,
                Target = target,
                UseLabel = labelSettings.UseLabel,
                Label = labelSettings.LabelText,
                Value = property.GetValue(target),
                ValueType = property.PropertyType,
                IsEnabled = !FlowBlockUIAttributeHelper.IsDynamicallyReadOnly(target, uiAttribute),
                Control = controlResult.FrameworkElement,
                IsActive = isActive,
                TooltipText = description
            };

            if (controlResult.PropertyViewControlFactory != null)
                controlResult.PropertyViewControlFactory.AssociationBeforeLink += propertyControlViewModel.RelayAssociationBeforeLink;

            bindingContext.Register(property, propertyControlViewModel);

            return propertyControlViewModel;
        }

        private (bool UseLabel, string LabelText) GetLabelSettings(
            PropertyInfo property,
            string displayName,
            FlowBloxUIAttribute uiAttribute)
        {
            // If DisplayLabel is explicitly set to false, never show a label.
            bool displayLabel = uiAttribute?.DisplayLabel ?? true;
            if (!displayLabel)
                return (false, string.Empty);

            return (true, displayName);
        }

        private PropertyControlWithFactoryResult CreateControl(
            PropertyInfo property, 
            object target, 
            string displayName, 
            FlowBloxUIAttribute uiAttribute, 
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
                var toggle = new ToggleSwitch
                {
                    IsEnabled = !readOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 4),
                    MinWidth = 80,
                    OnContent = FlowBloxResourceUtil.GetLocalizedString("PropertyControlResolver_ToggleSwitch_OnContent", typeof(FlowBloxTexts)),
                    OffContent = FlowBloxResourceUtil.GetLocalizedString("PropertyControlResolver_ToggleSwitch_OffContent", typeof(FlowBloxTexts))
                };

                toggle.SetBinding(ToggleSwitch.IsOnProperty, binding);

                toggle.Toggled += (s, e) =>
                    FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);

                return new PropertyControlWithFactoryResult(toggle);
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
                return new PropertyControlWithFactoryResult(new Border
                {
                    Child = comboBox,
                    Background = Brushes.Transparent
                });
            }

            // Selection-Filter
            if (uiAttribute?.Factory == UIFactory.ComboBox)
            {
                var selectionMethodResolution = SelectionMethodResolver.ResolveSelectionFilterMethodFromTargetOrParent(
                    target,
                    _parent,
                    uiAttribute?.SelectionFilterMethod);

                if (selectionMethodResolution?.Method != null)
                {
                    var originalItems = selectionMethodResolution.Method.Invoke(selectionMethodResolution.InvocationTarget, null) as IList;
                    
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
                        DisplayMemberPath = uiAttribute?.SelectionDisplayMember
                    };

                    comboBox.SelectionChanged += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);

                    SetComboBoxReadOnly(comboBox, readOnly);

                    comboBox.SetBinding(ComboBox.SelectedValueProperty, binding);
                    return new PropertyControlWithFactoryResult(new Border
                    {
                        Child = comboBox,
                        Background = Brushes.Transparent
                    });
                }
            }

            if (property.PropertyType == typeof(int) || 
                property.PropertyType == typeof(int?) ||
                property.PropertyType == typeof(float) ||
                property.PropertyType == typeof(float?) ||
                property.PropertyType == typeof(double) ||
                property.PropertyType == typeof(double?))
            {
                binding.Converter = new NumericTextValueConverter(property.PropertyType);

                var textBox = new TextBox
                {
                    IsReadOnly = readOnly
                };
                textBox.TextChanged += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(target, property.Name);
                textBox.SetBinding(TextBox.TextProperty, binding);
                return new PropertyControlWithFactoryResult(textBox);
            }

            if (uiAttribute?.Factory == UIFactory.Association)
            {
                AssociationControlFactory associationControlFactory = new AssociationControlFactory(_window, property, target, readOnly, _parent);
                return new PropertyControlWithFactoryResult(associationControlFactory.Create(), associationControlFactory);
            }

            if (uiAttribute?.Factory == UIFactory.GridView)
            {
                DataGridFactory dataGridFactory = new DataGridFactory(_window, property, target, readOnly);
                dataGridFactory.SetPreselectedInstance(preselectedInstance);
                return new PropertyControlWithFactoryResult(dataGridFactory.Create(), dataGridFactory);
            }

            if (uiAttribute?.Factory == UIFactory.ListView)
            {
                ListViewFactory listViewFactory = new ListViewFactory(_window, property, target, readOnly, _parent);
                listViewFactory.SetPreselectedInstance(preselectedInstance);
                return new PropertyControlWithFactoryResult(listViewFactory.Create(), listViewFactory);
            }

            if (uiAttribute?.Factory == UIFactory.ListViewSplitMode)
            {
                ListViewSplitModeFactory listViewSplitModeFactory = new ListViewSplitModeFactory(_window, property, target, readOnly, _parent);
                return new PropertyControlWithFactoryResult(listViewSplitModeFactory.Create(), listViewSplitModeFactory);
            }

            return new PropertyControlWithFactoryResult(
                _textBoxWithOptionalButtonsCreator
                    .CreateTextBoxWithOptionalButtons(property, target, displayName, uiAttribute, binding, readOnly));
        }

        private void Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SetComboBoxReadOnly(ComboBox comboBox, bool readOnly)
        {
            comboBox.IsHitTestVisible = !readOnly;
            comboBox.Focusable = !readOnly;
            comboBox.IsTabStop = !readOnly;
        }
    }
}

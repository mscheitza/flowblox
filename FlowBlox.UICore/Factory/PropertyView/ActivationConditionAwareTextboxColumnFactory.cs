using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Converters.PropertyView;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView
{
    internal static class ActivationConditionAwareTextboxColumnFactory
    {
        private const double DefaultMinWidth = 140;

        public static DataGridTemplateColumn Create(
            string headerText,
            PropertyInfo property,
            ActivationConditionAttribute activationCondition,
            bool isReadOnly)
        {
            ArgumentNullException.ThrowIfNull(property);
            ArgumentNullException.ThrowIfNull(activationCondition);

            EnsureSupportedPropertyType(property.PropertyType);

            var displayText = new FrameworkElementFactory(typeof(TextBlock));
            displayText.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            displayText.SetBinding(TextBlock.TextProperty, CreateValueBinding(property, isReadOnly: true));
            displayText.SetBinding(
                UIElement.VisibilityProperty,
                CreateActivationBinding(activationCondition, showWhenActive: true));

            var editText = new FrameworkElementFactory(typeof(TextBox));
            editText.SetValue(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            editText.SetValue(TextBox.IsReadOnlyProperty, isReadOnly);
            editText.SetBinding(TextBox.TextProperty, CreateValueBinding(property, isReadOnly));
            editText.SetBinding(
                UIElement.VisibilityProperty,
                CreateActivationBinding(activationCondition, showWhenActive: true));

            var displayGrid = new FrameworkElementFactory(typeof(System.Windows.Controls.Grid));
            displayGrid.AppendChild(displayText);
            displayGrid.AppendChild(CreateInactiveCellText(activationCondition));

            var editGrid = new FrameworkElementFactory(typeof(System.Windows.Controls.Grid));
            editGrid.AppendChild(editText);
            editGrid.AppendChild(CreateInactiveCellText(activationCondition));

            return new DataGridTemplateColumn
            {
                Header = headerText,
                MinWidth = DefaultMinWidth,
                SortMemberPath = property.Name,
                IsReadOnly = isReadOnly,
                CellTemplate = new DataTemplate { VisualTree = displayGrid },
                CellEditingTemplate = new DataTemplate { VisualTree = editGrid }
            };
        }

        private static Binding CreateValueBinding(PropertyInfo property, bool isReadOnly)
        {
            var binding = new Binding(property.Name)
            {
                Mode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                TargetNullValue = string.Empty
            };

            if (IsNumericType(property.PropertyType))
                binding.Converter = new NumericTextValueConverter(property.PropertyType);

            return binding;
        }

        private static FrameworkElementFactory CreateInactiveCellText(
            ActivationConditionAttribute activationCondition)
        {
            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetValue(TextBlock.TextProperty, FlowBloxResourceUtil.GetLocalizedString("Global_NotApplicable"));
            text.SetValue(TextBlock.BackgroundProperty, new SolidColorBrush(Color.FromRgb(255, 232, 204)));
            text.SetValue(TextBlock.ForegroundProperty, Brushes.DarkSlateGray);
            text.SetValue(TextBlock.PaddingProperty, new Thickness(4, 2, 4, 2));
            text.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            text.SetValue(UIElement.IsHitTestVisibleProperty, false);
            text.SetBinding(
                UIElement.VisibilityProperty,
                CreateActivationBinding(activationCondition, showWhenActive: false));
            return text;
        }

        private static MultiBinding CreateActivationBinding(
            ActivationConditionAttribute activationCondition,
            bool showWhenActive)
        {
            var binding = new MultiBinding
            {
                Converter = new ActivationConditionVisibilityConverter(
                    activationCondition,
                    showWhenActive)
            };
            binding.Bindings.Add(new Binding("."));

            if (!string.IsNullOrWhiteSpace(activationCondition.MemberName))
                binding.Bindings.Add(new Binding(activationCondition.MemberName));

            return binding;
        }

        private static void EnsureSupportedPropertyType(Type propertyType)
        {
            if (propertyType == typeof(string) || IsNumericType(propertyType))
                return;

            throw new NotSupportedException(
                $"Activation-aware textbox columns do not support properties of type '{propertyType.Name}'.");
        }

        private static bool IsNumericType(Type propertyType)
        {
            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            return type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(float) ||
                   type == typeof(double);
        }

        private sealed class ActivationConditionVisibilityConverter : IMultiValueConverter
        {
            private readonly ActivationConditionAttribute _condition;
            private readonly bool _showWhenActive;

            public ActivationConditionVisibilityConverter(
                ActivationConditionAttribute condition,
                bool showWhenActive)
            {
                _condition = condition;
                _showWhenActive = showWhenActive;
            }

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var isActive = values.Length > 0 &&
                    values[0] != null &&
                    values[0] != DependencyProperty.UnsetValue &&
                    _condition.IsActive(values[0]);

                return isActive == _showWhenActive ? Visibility.Visible : Visibility.Collapsed;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                => throw new NotSupportedException();
        }
    }
}

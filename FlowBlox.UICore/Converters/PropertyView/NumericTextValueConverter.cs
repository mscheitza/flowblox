using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.PropertyView
{
    public sealed class NumericTextValueConverter : IValueConverter
    {
        private readonly Type _targetType;
        private readonly Type _underlyingType;
        private readonly bool _isNullable;

        public NumericTextValueConverter(Type targetType)
        {
            _targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            _underlyingType = Nullable.GetUnderlyingType(_targetType) ?? _targetType;
            _isNullable = Nullable.GetUnderlyingType(_targetType) != null;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            if (value is IFormattable formattable)
                return formattable.ToString(null, culture ?? CultureInfo.CurrentCulture);

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString()?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                if (_isNullable)
                    return null;

                return Binding.DoNothing;
            }

            var currentCulture = culture ?? CultureInfo.CurrentCulture;
            var invariant = CultureInfo.InvariantCulture;

            if (_underlyingType == typeof(int))
            {
                if (int.TryParse(text, NumberStyles.Integer, currentCulture, out var parsedCurrent))
                    return parsedCurrent;

                if (int.TryParse(text, NumberStyles.Integer, invariant, out var parsedInvariant))
                    return parsedInvariant;

                return DependencyProperty.UnsetValue;
            }

            if (_underlyingType == typeof(long))
            {
                if (long.TryParse(text, NumberStyles.Integer, currentCulture, out var parsedCurrent))
                    return parsedCurrent;

                if (long.TryParse(text, NumberStyles.Integer, invariant, out var parsedInvariant))
                    return parsedInvariant;

                return DependencyProperty.UnsetValue;
            }

            if (_underlyingType == typeof(float))
            {
                if (float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, currentCulture, out var parsedCurrent))
                    return parsedCurrent;

                if (float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, invariant, out var parsedInvariant))
                    return parsedInvariant;

                return DependencyProperty.UnsetValue;
            }

            if (_underlyingType == typeof(double))
            {
                if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, currentCulture, out var parsedCurrent))
                    return parsedCurrent;

                if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, invariant, out var parsedInvariant))
                    return parsedInvariant;

                return DependencyProperty.UnsetValue;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}

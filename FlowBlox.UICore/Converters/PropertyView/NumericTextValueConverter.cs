using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.PropertyView
{
    public sealed class NumericTextValueConverter : IValueConverter
    {
        private static readonly Regex IntegerTextRegex = new(@"^[+-]?\d*$", RegexOptions.Compiled);
        private static readonly Regex FloatingPointTextRegex = new(@"^[+-]?\d*(?:[,.]\d*)?$", RegexOptions.Compiled);

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

            var invariant = CultureInfo.InvariantCulture;

            if (_underlyingType == typeof(int))
            {
                if (!IntegerTextRegex.IsMatch(text))
                    return DependencyProperty.UnsetValue;

                if (IsSignOnly(text))
                    return Binding.DoNothing;

                if (int.TryParse(text, NumberStyles.Integer, invariant, out var parsed))
                    return parsed;

                return DependencyProperty.UnsetValue;
            }

            if (_underlyingType == typeof(long))
            {
                if (!IntegerTextRegex.IsMatch(text))
                    return DependencyProperty.UnsetValue;

                if (IsSignOnly(text))
                    return Binding.DoNothing;

                if (long.TryParse(text, NumberStyles.Integer, invariant, out var parsed))
                    return parsed;

                return DependencyProperty.UnsetValue;
            }

            if (_underlyingType == typeof(float))
            {
                if (!FloatingPointTextRegex.IsMatch(text))
                    return DependencyProperty.UnsetValue;

                if (IsIncompleteFloatingPointText(text))
                    return Binding.DoNothing;

                if (float.TryParse(NormalizeDecimalSeparator(text), NumberStyles.Float, invariant, out var parsed))
                    return (float)parsed;

                return DependencyProperty.UnsetValue;
            }

            if (_underlyingType == typeof(double))
            {
                if (!FloatingPointTextRegex.IsMatch(text))
                    return DependencyProperty.UnsetValue;

                if (IsIncompleteFloatingPointText(text))
                    return Binding.DoNothing;

                if (double.TryParse(NormalizeDecimalSeparator(text), NumberStyles.Float, invariant, out var parsed))
                    return parsed;

                return DependencyProperty.UnsetValue;
            }

            return DependencyProperty.UnsetValue;
        }

        private static string NormalizeDecimalSeparator(string text) => text.Replace(',', '.');

        private static bool IsSignOnly(string text) => text is "+" or "-";

        private static bool IsIncompleteFloatingPointText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var trimmed = text.Trim();
            if (trimmed is "+" or "-" or "." or "," or "+." or "-." or "+," or "-,")
                return true;

            if (trimmed.EndsWith(".", StringComparison.Ordinal) ||
                trimmed.EndsWith(",", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }
    }
}
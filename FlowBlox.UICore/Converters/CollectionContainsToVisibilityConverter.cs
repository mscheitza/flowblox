using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public class CollectionContainsToVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        /// values[0] = IEnumerable
        /// values[1] = object
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return Visibility.Visible;

            var collection = values[0] as IEnumerable;
            var item = values[1];

            string[] parameterValues = ((string)parameter)?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            string yesValue = parameterValues.Length > 0 ? parameterValues.ElementAt(0) : null;
            string noValue = parameterValues.Length > 1 ? parameterValues.ElementAt(1) : null;
            string nullValue = parameterValues.Length > 2 ? parameterValues.ElementAt(2) : null;

            if (!Enum.TryParse(yesValue, out Visibility resultYes))
                resultYes = Visibility.Visible;

            if (!Enum.TryParse(noValue, out Visibility resultNo))
                resultNo = Visibility.Collapsed;

            if (!Enum.TryParse(nullValue, out Visibility resultNull))
                resultNull = Visibility.Visible;

            if (item == null)
                return resultNull;

            bool contains = false;
            if (collection != null && item != null)
            {
                foreach (var x in collection)
                {
                    if (ReferenceEquals(x, item) || Equals(x, item))
                    {
                        contains = true;
                        break;
                    }
                }
            }

            return contains ? 
                resultYes :
                resultNo;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

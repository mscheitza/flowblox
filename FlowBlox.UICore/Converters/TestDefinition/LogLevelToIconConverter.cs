using System;
using System.Globalization;
using System.Windows.Data;
using FlowBlox.Core.Enums;
using MahApps.Metro.IconPacks;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class LogLevelToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowBloxLogLevel logLevel)
            {
                switch (logLevel)
                {
                    case FlowBloxLogLevel.Success:
                        return PackIconModernKind.Check;
                    case FlowBloxLogLevel.Error:
                        return PackIconModernKind.Cancel;
                    case FlowBloxLogLevel.Warning:
                        return PackIconModernKind.Warning;
                    case FlowBloxLogLevel.Info:
                        return PackIconModernKind.Information;
                    default:
                        return PackIconModernKind.Question;
                }
            }
            return PackIconModernKind.Question;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

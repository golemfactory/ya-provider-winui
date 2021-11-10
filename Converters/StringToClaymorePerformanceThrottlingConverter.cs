using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GolemUI.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public sealed class StringToPhoenixPerformanceThrottlingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                PerformanceThrottlingEnumConverter.ConvertToString((PerformanceThrottlingEnum)value);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                return PerformanceThrottlingEnumConverter.FromString((string)value);
            }
            return 0;
        }

    }
}

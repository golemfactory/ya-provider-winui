using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GolemUI.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class ValueToVisibilityConverter : IValueConverter
    {
        public ValueToVisibilityConverter()
        {

        }

        public object? Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value?.Equals(parameter) ?? parameter == null)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object? ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}

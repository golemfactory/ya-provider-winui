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
    public sealed class NonNullToVisibilityConverter : IValueConverter
    {
        public NonNullToVisibilityConverter()
        {

        }

        public object? Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object? ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}

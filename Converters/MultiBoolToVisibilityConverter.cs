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
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class MultiBoolToVisibilityConverter : IMultiValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public MultiBoolToVisibilityConverter()
        {
            // set defaults
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        public object? Convert(object[] values, Type targetType,
            object parameter, CultureInfo culture)
        {
            var ret = values.All(v => (v is bool && (bool)v))
               ? TrueValue
               : FalseValue;
            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}

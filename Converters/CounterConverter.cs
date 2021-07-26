using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GolemUI.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public sealed class CounterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var idx = value as int?;
            if (idx == null)
            {
                return "";
            }
            return $"{idx + 1}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

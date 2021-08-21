using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GolemUI.Converters
{
    [ValueConversion(typeof(float?), typeof(string))]
    public class HashRateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            double? baseValue = value as double?;
            float? baseValue2 = value as float?;

            if (baseValue == null && baseValue2 != null)
            {
                baseValue = (double)baseValue2;
            }

            if (baseValue == null || baseValue < 0.1)
            {
                return "-- MH/s";
            }
            if (baseValue < 10.0f)
            {
                return $"{baseValue:0.00} MH/s";
            }
            if (baseValue < 100.0f)
            {
                return $"{baseValue:0.0} MH/s";
            }
            if (baseValue < 1000.0f)
            {
                return $"{baseValue:0} MH/s";
            }
            return $"{baseValue * .001:0.00} GH/s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

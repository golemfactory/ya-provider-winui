using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GolemUI.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public sealed class StringToClaymorePerformanceThrottlingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                switch ((int)value)
                {
                    case 0: return "Maximum";
                    case 5: return "Medium";
                    case 10: return "Comfort";
                    case 100: return "Minimum";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                string performanceThrottling = (string)value;
                if (string.Compare(performanceThrottling, "Maximum", true) == 0) return 0;
                if (string.Compare(performanceThrottling, "Medium", true) == 0) return 5;
                if (string.Compare(performanceThrottling, "Comfort", true) == 0) return 10;
                if (string.Compare(performanceThrottling, "Minimum", true) == 0) return 100;
            }
            return 0;
        }

    }
}

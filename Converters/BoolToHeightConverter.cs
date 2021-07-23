using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GolemUI;
namespace GolemUI.Converters
{
    [ValueConversion(typeof(bool?), typeof(int))]
    public class BoolToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var baseValue = value as bool?;
            if (baseValue == null)
            {
                return false;
            }
            return baseValue == true ? 0 : 20;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using GolemUI;
namespace GolemUI.Converters
{
    [ValueConversion(typeof(DashboardStatusEnum?), typeof(Visibility))]
    public class DashboardStatusToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var baseValue = value as DashboardStatusEnum?;
            if (baseValue == null || baseValue == DashboardStatusEnum.Hidden)
            {
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

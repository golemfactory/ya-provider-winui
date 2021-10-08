using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using GolemUI;
namespace GolemUI.Converters
{
    [ValueConversion(typeof(DashboardStatusEnum?), typeof(string))]
    public class DashboardStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var baseValue = value as DashboardStatusEnum?;
            if (baseValue == null)
            {
                return "";
            }
            return baseValue switch
            {
                DashboardStatusEnum.Error => "error",
                DashboardStatusEnum.Hidden => "",
                DashboardStatusEnum.Ready => "ready to mine",
                DashboardStatusEnum.Mining => "mining",
                _ => ""
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

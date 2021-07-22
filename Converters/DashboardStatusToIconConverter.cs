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
    [ValueConversion(typeof(DashboardStatusEnum?), typeof(BitmapImage))]
    public class DashboardStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var baseValue = value as DashboardStatusEnum?;
            if (baseValue == null)
            {
                return "";
            }
            String basePath = "/UI/Icons/Dashboard/Status/";
            string file = baseValue switch
            {
                DashboardStatusEnum.Error => "error",
                DashboardStatusEnum.Hidden => "ready",
                DashboardStatusEnum.Ready => "ready",
                DashboardStatusEnum.Mining => "mining",
                _ => "ready"
            };
            string path = basePath + file + ".png";

            return new BitmapImage(new Uri("pack://application:,,,/GolemUI;component" + path));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

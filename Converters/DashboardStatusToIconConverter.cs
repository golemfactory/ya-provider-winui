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
            String basePath = "/UI/Icons/DefaultStyle/png/MiningStatus/";
            string file = baseValue switch
            {
                DashboardStatusEnum.Error => "Error",
                DashboardStatusEnum.Hidden => "Ready",
                DashboardStatusEnum.Ready => "Ready",
                //DashboardStatusEnum.ReadyLowMemory => "Ready",
                DashboardStatusEnum.Mining => "Mining",
                //DashboardStatusEnum.MiningLowMemory => "Mining",
                _ => "ready"
            };
            string path = basePath + file + ".png";

            return new BitmapImage(new Uri("pack://application:,,,/ThorgMiner;component" + path));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

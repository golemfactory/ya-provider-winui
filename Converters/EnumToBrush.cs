using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GolemUI.Converters
{
    [ValueConversion(typeof(NavBarItem.ItemStatus), typeof(Brush))]
    class EnumToBrush : IValueConverter
    {
        public Brush InactiveBrush { get; set; }

        public Brush RealizedBrush { get; set; }

        public Brush ActiveBrush { get; set; }

        public EnumToBrush()
        {
            InactiveBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B9B9B"));
            RealizedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0FB2AB"));
            ActiveBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D119AD")); ;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value as NavBarItem.ItemStatus? ?? NavBarItem.ItemStatus.Inactive)
            {
                case NavBarItem.ItemStatus.Inactive:
                    return InactiveBrush;
                case NavBarItem.ItemStatus.Realized:
                    return RealizedBrush;
                case NavBarItem.ItemStatus.Active:
                    return ActiveBrush;
                default:
                    throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

﻿using GolemUI.Model;
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
            InactiveBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            RealizedBrush = new SolidColorBrush(Color.FromRgb(80, 200, 80)); ;
            ActiveBrush = new SolidColorBrush(Color.FromRgb(200, 80, 80)); ;
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
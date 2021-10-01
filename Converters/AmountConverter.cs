using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GolemUI.Converters
{
    public class AmountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter?.ToString() == "usdday")
            {
                return $"$-.-- / day";
            }
            if (value == null && parameter?.ToString() == "usdweek")
            {
                return $"$-.-- / week";
            }
            if (value == null && parameter?.ToString() == "usdmonth")
            {
                return $"$-.-- / month";
            }
            if (value == null && parameter?.ToString() == "glmmonth")
            {
                return $"-.-- GLM / month";
            }

            if (value == null)
            {
                return "-.--";
            }

            /*
            if (value == null)
            {
                return "N/A";
            }*/


            var dv = value as decimal?;
            if (dv == null && value.GetType() == typeof(double))
            {
                dv = (decimal?)(value as Double?);
            }
            if (dv != null)
            {
                if (parameter?.ToString() == "GLM")
                {
                    return $"{dv?.ToString("F4")} GLM";
                }
                if (parameter?.ToString() == "MATIC")
                {
                    return $"{dv?.ToString("F6")} MATIC";
                }
                if (parameter?.ToString() == "usdday")
                {
                    return $"${(dv)?.ToString("F2")} / day";
                }
                if (parameter?.ToString() == "usdweek")
                {
                    return $"${(dv)?.ToString("F2")} / week";
                }
                if (parameter?.ToString() == "usdmonth")
                {
                    return $"${(dv * 30)?.ToString("F2")} / month";
                }
                if (parameter?.ToString() == "glmmonth")
                {
                    return $"{(dv * 30)?.ToString("F2")} GLM / month";
                }
                return "$" + dv?.ToString("F2");
            }

            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

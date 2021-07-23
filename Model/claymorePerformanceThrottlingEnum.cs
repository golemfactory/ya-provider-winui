using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI
{
    public enum PerformanceThrottlingEnum 
    { 
        Maximum = 0, 
        Medium = 5, 
        Comfort = 10, 
        Minimum = 100 
    };

    static class PerformanceThrottlingEnumConverter
    {
        public static PerformanceThrottlingEnum FromInt(int value)
        {
            return value switch
            {
                0 => PerformanceThrottlingEnum.Maximum,
                5 => PerformanceThrottlingEnum.Medium,
                10 => PerformanceThrottlingEnum.Comfort,
                100 => PerformanceThrottlingEnum.Minimum,
                _ => PerformanceThrottlingEnum.Comfort,
            };
        }
        public static string ConvertToString(PerformanceThrottlingEnum value)
        {
            return value switch
            {
                PerformanceThrottlingEnum.Maximum => "Maximum",
                PerformanceThrottlingEnum.Medium => "Medium",
                PerformanceThrottlingEnum.Comfort => "Comfort",
                PerformanceThrottlingEnum.Minimum => "Minimum",
                _ => "Undefined"
            };
        }
        public static PerformanceThrottlingEnum FromString(string? value)
        {
            return value switch
            {
                "Maximum" => PerformanceThrottlingEnum.Maximum,
                "Medium" => PerformanceThrottlingEnum.Medium,
                "Comfort" => PerformanceThrottlingEnum.Comfort,
                "Minimum" => PerformanceThrottlingEnum.Minimum,
                _ => PerformanceThrottlingEnum.Comfort,
            };

        }
    }
}

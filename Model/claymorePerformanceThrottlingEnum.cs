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
        High = 10,
        Medium = 100,
        Low = 200,
        Minimum = 400,
    };

    static class PerformanceThrottlingEnumConverter
    {
        public static PerformanceThrottlingEnum FromInt(int value)
        {
            return value switch
            {
                0 => PerformanceThrottlingEnum.Maximum,
                10 => PerformanceThrottlingEnum.High,
                100 => PerformanceThrottlingEnum.Medium,
                200 => PerformanceThrottlingEnum.Low,
                400 => PerformanceThrottlingEnum.Minimum,
                _ => PerformanceThrottlingEnum.High,
            };
        }
        public static string ConvertToString(PerformanceThrottlingEnum value)
        {
            return value switch
            {
                PerformanceThrottlingEnum.Maximum => "Maximum",
                PerformanceThrottlingEnum.High => "High",
                PerformanceThrottlingEnum.Medium => "Medium",
                PerformanceThrottlingEnum.Low => "Low",
                PerformanceThrottlingEnum.Minimum => "Minimum",
                _ => "Undefined"
            };
        }
        public static PerformanceThrottlingEnum FromString(string? value)
        {
            return value switch
            {
                "Maximum" => PerformanceThrottlingEnum.Maximum,
                "High" => PerformanceThrottlingEnum.High,
                "Medium" => PerformanceThrottlingEnum.Medium,
                "Low" => PerformanceThrottlingEnum.Low,
                "Minimum" => PerformanceThrottlingEnum.Minimum,
                _ => PerformanceThrottlingEnum.High,
            };

        }
    }
}

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
        public const PerformanceThrottlingEnum Default = PerformanceThrottlingEnum.High;

        public static PerformanceThrottlingEnum FromInt(int value)
        {
            return typeof(PerformanceThrottlingEnum).IsEnumDefined(value) ? (PerformanceThrottlingEnum)value : Default;
        }
        public static string ConvertToString(PerformanceThrottlingEnum value) => Enum.GetName(typeof(PerformanceThrottlingEnum), value) ?? "Undefined";

        public static PerformanceThrottlingEnum FromString(string? value)
        {
            if (Enum.TryParse(value, out PerformanceThrottlingEnum result))
            {
                return result;
            }
            return Default;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GolemUI.Validators
{
    public class EthAddrValidator : ValidationRule
    {
        public bool ForceChecksum { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (text == null)
            {
                return ValidationResult.ValidResult;
            }
            var reBasicAddress = new Regex("^0x[0-9a-f]{40}$", RegexOptions.IgnoreCase);
            if (!reBasicAddress.Match(text).Success)
            {
                return new ValidationResult(false, "invalid address, it should be 20 byte hexencoded number with 0x prefix");
            }

            if (!ForceChecksum)
            {
                if (text == text.ToLower() || text == text.ToUpper())
                {
                    return ValidationResult.ValidResult;
                }
            }
            // TODO checksum check
            return ValidationResult.ValidResult;

        }
    }
}

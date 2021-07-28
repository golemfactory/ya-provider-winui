using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Nethereum.Util;

namespace GolemUI.Validators
{
    public class EthAddrValidator : ValidationRule
    {
        public bool ShouldCheckForChecksum { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (text == null)
            {
                return new ValidationResult(false, "Address can't be null");
            }
            var reBasicAddress = new Regex("^0x[0-9a-f]{40}$", RegexOptions.IgnoreCase);
            if (!reBasicAddress.Match(text).Success)
            {
                return new ValidationResult(false, "invalid address, it should be 20 byte hexencoded number with 0x prefix");
            }

            /*if (!ShouldCheckForChecksum)
            {
                if (text == text.ToLower() || text == text.ToUpper())
                {
                    return ValidationResult.ValidResult;
                }
            }*/
            if (ShouldCheckForChecksum)
            {
                if (new AddressUtil().IsChecksumAddress(text))
                {
                    return ValidationResult.ValidResult;
                }
                else
                {
                    return new ValidationResult(false, "invalid address, checksum does not match expected value");
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}

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
    public class NodeNameValidator : ValidationRule
    {
        //public bool ForceChecksum { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (text == null)
            {
                return new ValidationResult(false, "Node Name cannot be null");
            }
            if (text.Length < 3)
            {
                return new ValidationResult(false, "Node Name must have at least 3 characters");
            }
            var reBasicAddress = new Regex(@"^[a-zA-Z\d-_ ]+$", RegexOptions.IgnoreCase);
            if (!reBasicAddress.Match(text).Success)
            {
                return new ValidationResult(false, "wrong characters, use only alphanumeric characters, ' ', '-' and '_' ");
            }

            /*if (!ForceChecksum)
            {
                if (text == text.ToLower() || text == text.ToUpper())
                {
                    return ValidationResult.ValidResult;
                }
            }*/
            // TODO checksum check
            return ValidationResult.ValidResult;

        }
    }
}

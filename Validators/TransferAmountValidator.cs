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
    public class TransferAmountValidator : ValidationRule
    {

        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult vResult = ValidationResult.ValidResult;
            decimal parameter = 0;
            try
            {

                if (((string)value).Length > 0) //Check if there is a input in the textbox
                {
                    parameter = Decimal.Parse((String)value);
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((parameter < this.Min) || (parameter > this.Max))
            {
                return new ValidationResult(false, $"Please enter value in the range: {Min} - {Max}.");
            }
            return vResult;
        }
    }
}

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

        public double Min { get; set; }
        public double Max { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult vResult = ValidationResult.ValidResult;
            double parameter = 0;
            try
            {

                if (((string)value).Length > 0) //Check if there is a input in the textbox
                {
                    parameter = Double.Parse((String)value);
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((parameter < this.Min) || (parameter > this.Max))
            {
                return new ValidationResult(false, "Please enter value in the range: " + this.Min + " - " + this.Max + ".");
            }
            return vResult;
        }
    }
}

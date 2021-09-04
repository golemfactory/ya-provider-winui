using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GolemUI.Validators
{
    public class NonEmptyStringValidator : ValidationRule
    {
        public string ErrorMessage { get; set; } = "Value cannot be empty.";
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult validationResult = new ValidationResult(false, ErrorMessage);
            if (value != null)
            {
                string? valueAsString = value as string;
                if (!String.IsNullOrEmpty(valueAsString))
                    validationResult = ValidationResult.ValidResult;
            }
            return validationResult;
        }
    }
}

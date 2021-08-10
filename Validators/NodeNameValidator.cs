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
            var reNodeName = new Regex(@"^[a-zA-Z0-9_]+[a-zA-Z\d-_ ]+$");
            if (!reNodeName.Match(text).Success)
            {
                return new ValidationResult(false, "wrong characters, use only alphanumeric characters, ' ', '-' and '_' name must start witch letter, underscore or digit");
            }

            return ValidationResult.ValidResult;
        }
    }
}

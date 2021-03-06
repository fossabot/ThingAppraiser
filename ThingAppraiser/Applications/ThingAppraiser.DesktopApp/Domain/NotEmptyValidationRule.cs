﻿using System.Globalization;
using System.Windows.Controls;

namespace ThingAppraiser.DesktopApp.Domain
{
    internal sealed class NotEmptyValidationRule : ValidationRule
    {
        public NotEmptyValidationRule()
        {
        }

        #region ValidationRule Overridden Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? string.Empty).ToString())
                ? new ValidationResult(false, "Field is required.")
                : ValidationResult.ValidResult;
        }

        #endregion
    }
}

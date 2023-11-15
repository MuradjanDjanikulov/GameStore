using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebAPI.Utils
{

    public class UppercaseLetterAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string password = "";
            const string pattern = @"^(?=.*[A-Z])";

            if (Regex.IsMatch(password, pattern))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

    public class LowercaseLetterAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string password="";
            const string pattern = @"^(?=.*[a-z])";

            if (Regex.IsMatch(password, pattern))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

    public class NumericSymbolAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string password = "";
            const string pattern = @"^(?=.*[0-9])";

            if (Regex.IsMatch(password, pattern))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }

    }

    public class NonAlphanumericSymbolAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string password = "";
            const string pattern = @"^(?=.*[^a-zA-Z0-9])\S+$";

            if (Regex.IsMatch(password, pattern))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
//    [RegularExpression(@"^(?=.*[^a-zA-Z0-9])\S+$", ErrorMessage = "The {0} must contain at least one non-alphabetic and non-numeric symbol.")]


}

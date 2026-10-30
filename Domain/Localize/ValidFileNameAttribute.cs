using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Localize
{
    public class ValidFileNameAttribute : ValidationAttribute
    {
        private static readonly char[] InvalidChars =
        {
        '<', '>', ':', '"', '/', '\\', '|', '?', '*',
        '#', '$', '%', '&', '@'
    };

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value is not IFormFile file)
                return ValidationResult.Success;

            string fileName = Path.GetFileName(file.FileName);

            if (string.IsNullOrWhiteSpace(fileName))
                return new ValidationResult("Filename is required.");

            if (fileName.IndexOfAny(InvalidChars) >= 0)
                return new ValidationResult(
                    "Filename contains illegal characters. Use letters, numbers, underscore, hyphen and spaces.");

            return ValidationResult.Success;
        }
    }
}

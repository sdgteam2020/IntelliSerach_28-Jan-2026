using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObject.Localize
{
    public class MaxFileNameLengthAttribute : ValidationAttribute
    {
        private readonly int _maxLength;

        public MaxFileNameLengthAttribute(int maxLength)
        {
            _maxLength = maxLength;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IFormFile file)
                return ValidationResult.Success;

            var fileName = Path.GetFileName(file.FileName);

            if (fileName.Length > _maxLength)
            {
                return new ValidationResult(
                    $"File name must not exceed {_maxLength} characters.");
            }

            return ValidationResult.Success;
        }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObject.Helpers
{
    public class AllowedContentTypeAttribute : ValidationAttribute
    {
        private readonly string[] _contenttype;
        public AllowedContentTypeAttribute(string[] contenttype)
        {
            _contenttype = contenttype;
        }

        protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var ContentType = file.ContentType;
                if (!_contenttype.Contains(ContentType.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Not a Valid Type of File.!";
        }
    }
}

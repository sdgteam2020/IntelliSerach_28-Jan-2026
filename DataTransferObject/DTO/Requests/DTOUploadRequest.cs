using DataTransferObject.Helpers;
using DataTransferObject.Localize;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTransferObject.DTO.Requests
{
    public class DTOUploadRequest
    {
        [AllowedExtensions(new[] { ".pdf" })]
        [AllowedContentType(new[] { "application/pdf" })]
        [MaxFileNameLength(100)]
        public required IFormFile? FileName { get; set; }
    }
}

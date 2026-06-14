using Domain.Helpers;
using Domain.Localize;
using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.Requests
{
    public class DTOUploadRequest
    {
        [AllowedExtensions(new[] { ".pdf" })]
        [AllowedContentType(new[] { "application/pdf" })]
        [MaxFileNameLength(100)]
        public IFormFile? FileName { get; set; }
    }
}
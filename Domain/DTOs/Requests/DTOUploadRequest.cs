using Domain.Helpers;
using Domain.Localize;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Requests
{
    public class DTOUploadRequest
    {
        [AllowedExtensions(new[] { ".pdf" })]
        [AllowedContentType(new[] { "application/pdf" })]
        [MaxFileNameLength(50)]
        [ValidFileName]

        public IFormFile? FileName { get; set; }
    }
}
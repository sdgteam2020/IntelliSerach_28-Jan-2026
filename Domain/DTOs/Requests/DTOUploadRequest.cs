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

        public required IFormFile? FileName { get; set; }

        public required string IndexName { get; set; }
        public required string uuid { get; set; }
    }
}
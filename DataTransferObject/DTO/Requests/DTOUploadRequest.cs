using DataTransferObject.Helpers;
using DataTransferObject.Localize;
using Microsoft.AspNetCore.Http;

namespace DataTransferObject.DTO.Requests
{
    public class DTOUploadRequest
    {
        [AllowedExtensions(new[] { ".pdf" })]
        [AllowedContentType(new[] { "application/pdf" })]
        [MaxFileNameLength(100)]
        public IFormFile? FileName { get; set; }
    }
}
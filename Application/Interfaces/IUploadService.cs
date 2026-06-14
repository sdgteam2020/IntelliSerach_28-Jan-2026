using Domain.DTOs.Response;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IUploadService
    {
        public string ProcessUploadedFile(IFormFile UploadDoc, string FileAddress, string FileName);

        public bool IsPdf(IFormFile postedFile);
    }
}

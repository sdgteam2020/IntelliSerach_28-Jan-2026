using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicsLayer.Service
{
    public interface IService
    {
        public string ProcessUploadedFile(IFormFile UploadDoc, string FileAddress, string FileName);
        public bool IsPdf(IFormFile postedFile);
    }
}

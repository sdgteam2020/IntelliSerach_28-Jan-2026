using Microsoft.AspNetCore.Http;

namespace BusinessLogicsLayer.Service
{
    public interface IService
    {
        public string ProcessUploadedFile(IFormFile UploadDoc, string FileAddress, string FileName);

        public bool IsPdf(IFormFile postedFile);
    }
}
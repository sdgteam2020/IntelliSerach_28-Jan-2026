using Domain.Constants;
using Domain.DTOs.Response;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Application.Interfaces.Repository;
using Application.Interfaces;

namespace Infrastructure.Shared.Services
{
    public class Upload : IUpload
    {
        private readonly IUploadService _service;
        private readonly IUploadFiles _uploadFilesDB;

        public Upload(IUploadService service, IUploadFiles uploadFilesDB)
        {
            _service = service;
            _uploadFilesDB = uploadFilesDB;
        }

        

        public async Task<DTOGenericResponse<object>> UploadFileAsync(IFormFile file, string uploadPath, int UserId)
        {
            if (uploadPath != null)
            {
                string ExitImagePath = Path.Combine(uploadPath, "PDF");
                if (System.IO.File.Exists(ExitImagePath))
                {
                    System.IO.File.Delete(ExitImagePath);
                }
            }
            string FileName = "";
            if (file != null)
            {
                // 5 MB = 5 * 1024 * 1024 bytes
                const long MaxFileSize = 5 * 1024 * 1024;

                if (file.Length > MaxFileSize)
                {
                    return new DTOGenericResponse<object>(
                        ConnKeyConstants.BadRequest,
                        "File size must be less than 5 MB.",
                        false
                    );
                }

                string originalName = Path.GetFileNameWithoutExtension(file.FileName);
                string extension = Path.GetExtension(file.FileName);

                // Add datetime (yyyyMMdd_HHmmss)
                string filename = $"{originalName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                FileName = _service.ProcessUploadedFile(file, uploadPath, filename);

                string path = Path.Combine(uploadPath, FileName);
                bool pdfcontentresult = _service.IsPdf(file);

                if (!pdfcontentresult)
                {
                    return new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ConnKeyConstants.NotPdfFile, false);
                }
                var now = DateTime.UtcNow;

                TrnUploadFiles trnUploadFiles = new TrnUploadFiles
                {
                    FileName = FileName,
                    CreatedOn = now,
                    UpdatedOn = now,
                    CreatedBy = UserId,
                    UpdatedBy = UserId,
                    IsActive = true,
                    IsDeleted = false
                };

                await _uploadFilesDB.AddWithReturn(trnUploadFiles);

                return new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.UploadSucess, true);
            }
            return new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.UploadSucess, true);
        }
    }
}
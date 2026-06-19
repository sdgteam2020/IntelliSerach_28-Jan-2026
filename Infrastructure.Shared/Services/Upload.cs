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



        public async Task<DTOGenericResponse<object>> UploadFileAsync(
     IFormFile file,
     string uploadPath,
     int userId,
     string userName)
        {
            if (file == null)
            {
                return new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    "No file selected.",
                    false);
            }

            // Create user folder
            string userFolder = Path.Combine(uploadPath, userName);

            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            // 5 MB = 5 * 1024 * 1024 bytes
            const long MaxFileSize = 5 * 1024 * 1024;

            if (file.Length > MaxFileSize)
            {
                return new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    "File size must be less than 5 MB.",
                    false);
            }

            string originalName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);

            string fileName = $"{originalName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

            bool pdfContentResult = _service.IsPdf(file);

            if (!pdfContentResult)
            {
                return new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    ConnKeyConstants.NotPdfFile,
                    false);
            }

            // Upload into user folder
            string savedFileName = _service.ProcessUploadedFile(
                file,
                userFolder,
                fileName);

            var now = DateTime.UtcNow;

            TrnUploadFiles trnUploadFiles = new TrnUploadFiles
            {
                FileName = savedFileName,
                FolderName= userName,
                OrignalFileName = file.FileName,
                CreatedOn = now,
                UpdatedOn = now,
                CreatedBy = userId,
                UpdatedBy = userId,
                IsActive = true,
                IsDeleted = false
            };

            await _uploadFilesDB.AddWithReturn(trnUploadFiles);

            return new DTOGenericResponse<object>(
                ConnKeyConstants.Success,
                ConnKeyConstants.UploadSucess,
                true);
        }
    }
}
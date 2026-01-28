using BusinessLogicsLayer.Service;
using DataAccessLayer;
using DataAccessLayer.GenericRepository;
using DataAccessLayer.UploadFiles;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLogicsLayer.UploadPdf
{
    public class UploadFiles:IUploadFiles
    {
        private readonly IService _service;
        private readonly IUploadFilesDB _uploadFilesDB;
        public UploadFiles(IService service, IUploadFilesDB uploadFilesDB)
        {
            _service = service;
           _uploadFilesDB = uploadFilesDB;
        }

        public Task<DTOGenericResponse<object>> GetUploadFileByUserId(int UserId)
        {
           return _uploadFilesDB.GetUploadFileByUserId(UserId);
        }

        public async Task<DTOGenericResponse<object>> UploadFileAsync(IFormFile file, string uploadPath,int UserId)
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
                FileName =_service.ProcessUploadedFile(file, uploadPath, filename);

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

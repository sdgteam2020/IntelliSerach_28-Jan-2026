using DataAccessLayer.GenericRepository;
using DataAccessLayer.UploadFiles;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicsLayer.UploadPdf
{
    public interface IUploadFiles
    {
        Task<DTOGenericResponse<object>> UploadFileAsync(IFormFile file, string uploadPath, int UserId);
        Task<DTOGenericResponse<object>> GetUploadFileByUserId(int UserId);


    }
}

using Domain.DTOs.Response;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IUpload
    {
        Task<DTOGenericResponse<object>> UploadFileAsync(IFormFile file, string uploadPath, int UserId,string UserName);

        Task<bool> FileDelete(string uploadPath,string FileName);
        Task<bool> CheckFileExits(string uploadPath, string FileName);
        Task<bool> CheckFileExits(string Path);
    }
}

using DataTransferObject.DTO.Response;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicsLayer.UploadPdf
{
    public interface IUploadFiles
    {
        Task<DTOGenericResponse<object>> UploadFileAsync(IFormFile file, string uploadPath, int UserId);

        Task<DTOGenericResponse<object>> GetUploadFileByUserId(int UserId);
    }
}
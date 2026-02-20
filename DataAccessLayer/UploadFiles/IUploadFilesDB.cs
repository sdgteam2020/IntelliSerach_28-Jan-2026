using DataAccessLayer.GenericRepository;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;

namespace DataAccessLayer.UploadFiles
{
    public interface IUploadFilesDB : IGenericRepository<TrnUploadFiles>
    {
        Task<DTOGenericResponse<object>> GetUploadFileByUserId(int UserId);
    }
}
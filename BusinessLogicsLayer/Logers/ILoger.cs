using DataAccessLayer.GenericRepository;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;

namespace BusinessLogicsLayer.Logers
{
    public interface ILoger : IGenericRepository<ExceptionLog>
    {
        Task<DTOGenericResponse<object>> AddAsync(DTOExceptionLogRequest Log);
    }
}
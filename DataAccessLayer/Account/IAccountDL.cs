using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;

namespace DataAccessLayer.Account
{
    public interface IAccountDL
    {
        Task<DTODataTablesResponse<DTOUserDataResponse>> GetAllUsers(DTODataTablesRequest request);
    }
}
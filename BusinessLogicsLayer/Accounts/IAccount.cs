using DataAccessLayer.GenericRepository;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.IdentityModel;

namespace BusinessLogicsLayer.Accounts
{
    public interface IAccount : IGenericRepository<ApplicationUser>
    {
        Task<DTODataTablesResponse<DTOUserDataResponse>> GetAllUsers(DTODataTablesRequest request);
    }
}
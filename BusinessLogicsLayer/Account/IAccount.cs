using DataAccessLayer.GenericRepository;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicsLayer.Account
{
    public interface IAccount :IGenericRepository<ApplicationUser>
    {
        Task<DTODataTablesResponse<DTOUserDataResponse>> GetAllUsers(DTODataTablesRequest request);
    }
}

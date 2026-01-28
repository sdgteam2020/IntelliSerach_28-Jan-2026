using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Account
{
    public interface IAccountDL
    {
        Task<DTODataTablesResponse<DTOUserDataResponse>> GetAllUsers(DTODataTablesRequest request);
    }
}

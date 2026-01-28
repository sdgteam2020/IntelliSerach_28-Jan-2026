using DataAccessLayer;
using DataAccessLayer.Account;
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
    public class Account :GenericRepository<ApplicationUser>, IAccount
    {
        private readonly IAccountDL accountDL;

        public Account(ApplicationDbContext context, IAccountDL _accountDL) : base(context)
        {
            accountDL = _accountDL;
        }

        public Task<DTODataTablesResponse<DTOUserDataResponse>> GetAllUsers(DTODataTablesRequest request)
        {
           return accountDL.GetAllUsers(request);
        }
    }
}

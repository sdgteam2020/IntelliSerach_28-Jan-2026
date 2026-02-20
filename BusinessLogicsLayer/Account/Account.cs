using DataAccessLayer;
using DataAccessLayer.Account;
using DataAccessLayer.GenericRepository;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.IdentityModel;

namespace BusinessLogicsLayer.Account
{
    public class Account : GenericRepository<ApplicationUser>, IAccount
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
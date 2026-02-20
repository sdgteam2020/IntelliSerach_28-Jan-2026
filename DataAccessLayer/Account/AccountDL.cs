using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Account
{
    public class AccountDL : IAccountDL
    {
        protected new readonly ApplicationDbContext _context;

        public AccountDL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DTODataTablesResponse<DTOUserDataResponse>> GetAllUsers(DTODataTablesRequest request)
        {
            var Data = (from u in _context.Users.OrderByDescending(x => x.Id)
                        select new DTOUserDataResponse()
                        {
                            Id = u.Id,
                            DomainId = u.UserName,
                            RoleNames = (from ur in _context.UserRoles.Where(x => x.UserId == u.Id)
                                         join r in _context.Roles on ur.RoleId equals r.Id
                                         select r.Name).ToList(),
                            Name = u.Name,
                            Active = u.Active
                        }).AsQueryable();

            var TotRec = await Data.CountAsync();
            //Apply filtering
            if (!string.IsNullOrEmpty(request.searchValue))
            {
                string searchValue = request.searchValue.ToLower();
                Data = Data.Where(x => x.DomainId.ToLower().Contains(searchValue));
            }
            // Apply sorting

            if (!string.IsNullOrEmpty(request.sortColumn) && !string.IsNullOrEmpty(request.sortDirection))
            {
                //queryableData = queryableData.OrderBy(request.SortColumn + " " + request.SortColumnDirection);
                Data = request.sortDirection.ToLower() == "asc"
                ? Data.OrderBy(item => EF.Property<object>(item, request.sortColumn))
                : Data.OrderByDescending(item => EF.Property<object>(item, request.sortColumn));
            }
            // Total records after filtering
            var filteredRecords = await Data.CountAsync();
            // Paginate the result
            var paginatedData = await Data.Skip(request.Start).Take(request.Length).ToListAsync();

            return new DTODataTablesResponse<DTOUserDataResponse>
            {
                draw = request.Draw,
                recordsTotal = TotRec,
                recordsFiltered = filteredRecords,
                data = paginatedData
            };
        }
    }
}
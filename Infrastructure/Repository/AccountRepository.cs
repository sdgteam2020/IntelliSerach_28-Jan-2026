using Application.Interfaces.Repository;
using Azure.Core;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.IdentityEntities;
using Infrastructure.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;

namespace Infrastructure.Repository
{
    public class AccountRepository : GenericRepository<ApplicationUser>, IUserAccount
    {
        protected new readonly ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context) : base(context)
        { 
            _context = context;
        }


        public async Task<DTODataTablesResponse<DTOUserDataResponse>> GetAllUsers(DTODataTablesRequest request)
        {
            var Data = (from u in _context.Users.OrderByDescending(x => x.Id).Where(i => i.Id != 1)
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
        public async Task<string> GetIndexWiseAssginUsers(string IndexId)
        {
            var users = await (
                from u in _context.Users
                        .Where(x => x.Id != 1 && x.Active)
                join map in _context.TrnIndexUserMapping
                        .Where(x => x.IndexId == IndexId)
                    on u.Id equals map.UserId
                select u.UserName
            ).ToListAsync();

            StringBuilder sb = new StringBuilder();

            foreach (var userName in users)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(userName);
            }

            return sb.ToString();
        }
        public async Task<DTODataTablesResponse<DTOUserDataResponse>> GetActive_ForIndex_Mapping_Users(DTODataTablesRequest request, string IndexId)
        {
            var Data =
    from u in _context.Users
            .Where(x => x.Id != 1 && x.Active)
            .OrderByDescending(x => x.Id)
    join rank in _context.MRank on u.RankId equals rank.RankId
    join map in _context.TrnIndexUserMapping
            .Where(x => x.IndexId == IndexId)
        on u.Id equals map.UserId into maps

    from map in maps.DefaultIfEmpty() // LEFT JOIN

    select new DTOUserDataResponse
    {
        Id = u.Id,
        DomainId = u.UserName,
        RoleNames =
            (from ur in _context.UserRoles.Where(x => x.UserId == u.Id)
             join r in _context.Roles on ur.RoleId equals r.Id
             select r.Name).ToList(),
        RankName = rank.RankAbbreviation,
        IndexId = map != null ? map.IndexId : null,
        Name = u.Name,
        Active = u.Active
    };

            var result = Data.AsQueryable();

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

        public async Task<List<DTOUserDataResponse>> GetActiveUsers()
        {
            return await _context.Users
          .Where(x => x.Id != 1 && x.Active)
          .Select(u => new DTOUserDataResponse
          {
              Id = u.Id,

          })
          .ToListAsync();
        }

        public async Task<string[]> GetAllIndexUserWise(int? UserId)
        {
            return await (
         from u in _context.Users.Where(x => x.Id != 1 && x.Active && x.Id == UserId)
         join map in _context.TrnIndexUserMapping
             on u.Id equals map.UserId
         select map.IndexId
     ).Distinct().ToArrayAsync();
        }
    }
}
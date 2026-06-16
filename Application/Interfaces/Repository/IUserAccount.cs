using Application.Interfaces.GenericRepository;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repository
{
    public interface IUserAccount : IGenericRepository<ApplicationUser>
    {
        Task<DTODataTablesResponse<DTOUserDataResponse>> GetAllUsers(DTODataTablesRequest request);
        Task<List<DTOUserDataResponse>> GetActiveUsers();
        Task<DTODataTablesResponse<DTOUserDataResponse>> GetActive_ForIndex_Mapping_Users(DTODataTablesRequest request,string IndexId);
        Task<string> GetIndexWiseAssginUsers(string IndexId);
        Task<string[]> GetAllIndexUserWise(int? UserId);


    }
}

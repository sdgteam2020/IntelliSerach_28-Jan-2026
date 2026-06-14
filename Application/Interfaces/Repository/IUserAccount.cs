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
    }
}

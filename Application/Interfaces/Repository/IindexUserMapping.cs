using Application.Interfaces.GenericRepository;
using Domain.DTOs.Requests;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repository
{
    public interface IindexUserMapping : IGenericRepository<TrnIndexUserMapping>
    {
        Task<TrnIndexUserMapping> AddWithCheckIndexId(TrnIndexUserMapping Data);
        Task<bool> UserAssginAndDeAssignIndex(DTOIndexAssignRequest Data);

        Task<bool> CheckUserIndexingExists(int UserId,string IndexId);
        // Task<string[]> GetAllIndexUserWise(int? UserId);
    }
}

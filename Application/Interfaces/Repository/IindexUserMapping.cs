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

        Task<bool> CheckUserIndexingExists(int UserId,string IndexId,String RoleName);
        Task<bool> CheckUserIndexingExists(int UserId, string IndexId);
        Task<bool> UpdateIndexPrivatePublic(string CrawlerId,bool IsPrivate, string userId, string roleName);
        Task<string> GetAllPrivateIndex(string roleName, string userId);
        Task<bool> IsPrivateIndex(string IndexId);
        Task<List<string>> GetPrivateIndexNoAcessThisuserId(string userId);
        // Task<string[]> GetAllIndexUserWise(int? UserId);
    }
}

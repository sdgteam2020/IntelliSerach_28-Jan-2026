using Application.Interfaces.GenericRepository;
using Domain.DTOs.Requests;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repository
{
    public interface IIndexer : IGenericRepository<MIndexes>
    {
        Task<MIndexes> Addindexes(MIndexes Data);
        Task<bool> SoftDeleteIndex(int id, int? updatedBy);
        Task<MIndexes?> GetActiveIndexById(int id);
        Task<List<MIndexes>> GetActiveIndexes();
        Task<bool> ActiveAndDeActiveIndex(int id, int? updatedBy);

        Task<bool> IsRegisterCrawler(DTORegisterCrawler Data);
    }
}

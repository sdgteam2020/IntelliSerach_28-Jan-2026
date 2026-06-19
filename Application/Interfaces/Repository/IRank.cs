using Application.Interfaces.GenericRepository;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repository
{
    public interface IRank : IGenericRepository<MRank>
    {
        Task<List<MRank>> GetAllOrderByRank();
    }
}

using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repository
{
    public class RankRepository : GenericRepository<MRank>, IRank
    {
        public RankRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<MRank>> GetAllOrderByRank()
        {
            return await _context.MRank.OrderBy(x => x.Orderby).ToListAsync();
        }
    }
}

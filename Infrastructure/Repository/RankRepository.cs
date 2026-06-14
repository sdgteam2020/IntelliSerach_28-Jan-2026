using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Repository.GenericRepository;
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
    }
}

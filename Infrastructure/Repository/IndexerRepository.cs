using Application.Interfaces.GenericRepository;
using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repository
{
    public class IndexerRepository : GenericRepository<MIndexes>, IIndexer
    {
        protected new readonly ApplicationDbContext _context;

        public IndexerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<MIndexes> Addindexes(MIndexes Data)
        {
            if (Data.Id == 0)
            {
                return await AddWithReturn(Data);
            }
            else if (Data.Id > 0)
            {
                await UpdateWithReturn(Data);
                return Data;
            }
            return Data;
        }
        public async Task<bool> SoftDeleteIndex(int id, int? updatedBy)
        {
            var entity = await _context.MIndexes.Where(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();

            if (entity == null)
                return false;
            entity.IsActive = false;
            entity.IsDeleted = true;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return true;
        }
        public Task<MIndexes?> GetActiveIndexById(int id)
        {
            return _context.MIndexes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }
        public Task<List<MIndexes>> GetActiveIndexes()
        {
            return _context.MIndexes
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.UpdatedOn)
                .ToListAsync();
        }

        public async Task<bool> ActiveAndDeActiveIndex(int id, int? updatedBy)
        {
            var entity = await _context.MIndexes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (entity == null)
                return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

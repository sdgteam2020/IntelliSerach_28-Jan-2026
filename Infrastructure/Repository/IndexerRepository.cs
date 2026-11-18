using Application.Interfaces.GenericRepository;
using Application.Interfaces.Repository;
using Domain.DTOs.Requests;
using Domain.Entities;
using Infrastructure.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            if (Data.IndexId == 0)
            {
                return await AddWithReturn(Data);
            }
            else if (Data.IndexId > 0)
            {
                await UpdateWithReturn(Data);
                return Data;
            }
            return Data;
        }
        public async Task<bool> SoftDeleteIndex(int id, int? updatedBy)
        {
            var entity = await _context.MIndexes.Where(x => x.IndexId == id && !x.IsDeleted).FirstOrDefaultAsync();

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
                .FirstOrDefaultAsync(x => x.IndexId == id && !x.IsDeleted);
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
                .FirstOrDefaultAsync(x => x.IndexId == id && !x.IsDeleted);

            if (entity == null)
                return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsPrivateIndex(string IndexId)
        {
            return await _context.MIndexes.AnyAsync(i => i.Uuid == IndexId && i.IsPrivate == true);
        }
        public async Task<bool> IsRegisterCrawler(DTORegisterCrawler Data)
        {
            if (Data == null ||
       string.IsNullOrWhiteSpace(Data.CrawlerId) ||
       string.IsNullOrWhiteSpace(Data.IndexerKey) ||
       string.IsNullOrWhiteSpace(Data.SystemIp))
            {
                return false;
            }
            var crawlerId = Data.CrawlerId.Trim();

            crawlerId = crawlerId.Replace("fs-", "");

            bool ret= await _context.MIndexes.AnyAsync(i =>
                i.IndexName.ToLower() == crawlerId.ToLower() &&
                i.Key.ToLower() == Data.IndexerKey.ToLower() &&
                i.IpAddress == Data.SystemIp && i.IsActive==true);
            return ret;
        }

        public async Task<int> GetIndexIdBUuid(string Uuid, string IndexName)
        {
            if (string.IsNullOrWhiteSpace(Uuid))
                return 0;

            // Check existing UUID in database starts with fs-
            var index = await _context.MIndexes
                .FirstOrDefaultAsync(x =>
                    x.IndexName == IndexName.Replace("fs-","") &&
                    x.Uuid.StartsWith("fs-") &&
                    !x.IsDeleted &&
                    x.IsActive);

            // Update existing UUID with new UUID
            if (index != null && index.Uuid != Uuid)
            {
                index.Uuid = Uuid;

                await _context.SaveChangesAsync();
            }

            // Return IndexId using updated UUID
            return await _context.MIndexes
                .Where(x =>
                    x.Uuid == Uuid &&
                    !x.IsDeleted &&
                    x.IsActive)
                .Select(x => x.IndexId)
                .FirstOrDefaultAsync();
        }
    }
}

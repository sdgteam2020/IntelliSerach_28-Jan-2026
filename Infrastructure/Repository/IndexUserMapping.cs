using Application.Interfaces.Repository;
using Domain.DTOs.Requests;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repository
{
    public class IndexUserMapping : GenericRepository<TrnIndexUserMapping>, IindexUserMapping
    {
        protected new readonly ApplicationDbContext _context;
        public IndexUserMapping(ApplicationDbContext context) : base(context)
        {
            _context=context;
        }

        public async Task<TrnIndexUserMapping> AddWithCheckIndexId(TrnIndexUserMapping data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var existing = await _context.TrnIndexUserMapping.FirstOrDefaultAsync(x =>
                    x.IndexId == data.IndexId &&
                    x.UserId == data.UserId);

            if (existing == null)
            {
                _context.TrnIndexUserMapping.Add(data);
                await _context.SaveChangesAsync();
                return data;
            }

            // Record already exists, so return the existing one
            return existing;
        }

        public async Task<bool> CheckUserIndexingExists(int UserId, string IndexId)
        {
            if ((int)UsersId.Admin == UserId)
                return true;
          
            return await _context.TrnIndexUserMapping.AnyAsync(i =>i.UserId == UserId && i.IndexId == IndexId);
        }

        public async Task<bool> UserAssginAndDeAssignIndex(DTOIndexAssignRequest Data)
        {

            var existingMappings = _context.TrnIndexUserMapping
     .Where(x => x.IndexId == Data.IndexId &&
                 !Data.UserIds.Contains(x.UserId))
     .ToList();

            if (existingMappings.Any())
            {
                _context.TrnIndexUserMapping.RemoveRange(existingMappings);
                _context.SaveChanges();
            }

            return true;
        }
    }
}

using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
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
    }
}

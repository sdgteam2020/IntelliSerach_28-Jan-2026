using Application.Interfaces;
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
using System.Text.Json;
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
        public async Task<bool> IsPrivateIndex(string IndexId)
        { 
            return await _context.TrnIndexUserMapping.AnyAsync(i => i.IndexId == IndexId);
        }
        public async Task<bool> CheckUserIndexingExists(int UserId, string IndexId, String RoleName)
        { // Admin has access to all indexes
            if (string.Equals(RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            //if ((int)UsersId.Admin == UserId)
            //    return true;
          
            return await _context.TrnIndexUserMapping.AnyAsync(i =>i.UserId == UserId && i.IndexId == IndexId);
        }
        public async Task<bool> CheckUserIndexingExists(int UserId, string IndexId)
        { // Admin has access to all indexes
           
            return await _context.TrnIndexUserMapping.AnyAsync(i => i.UserId == UserId && i.IndexId == IndexId);
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
        public async Task<bool> UpdateIndexPrivatePublic(string CrawlerId,bool IsPrivate, string userId, string roleName)
        {
            try
            {
                // 1. Check if the role is Admin (ignoring case)
                bool isAdmin = string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase);

                int parsedUserId = 0;

                // 2. If not an Admin, parse and require a valid UserId
                if (!isAdmin)
                {
                    if (!int.TryParse(userId, out parsedUserId))
                    {
                        return false; // Invalid user ID format for non-admin users
                    }
                }

                // 3. Build the query dynamically based on the role
                var query = _context.TrnIndexUserMapping
                    .Where(m => m.IndexName == CrawlerId);

                // If not Admin, filter by UserId as well
                //if (!isAdmin)
                //{
                //    query = query.Where(m => m.UserId == parsedUserId);
                //}

                // 4. Fetch matching records
                var mappings = await query.ToListAsync();

                if (mappings == null || !mappings.Any())
                {
                    return false; // No records found to update
                }

                // 5. Determine the new state by toggling the current state of the first record
                bool newPrivateState = !mappings.First().IsPrivate;

                // 6. Apply the new state to all matching records
                foreach (var mapping in mappings)
                {
                    mapping.IsPrivate = IsPrivate;
                }

                // 7. Save changes to the database
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                // Optional: Add logging here (e.g., _logger.LogError(ex, ...))
                return false;
            }
        }

        public async Task<string> GetAllPrivateIndex(string roleName, string userId)
        {
            try
            {
                // 1. Check if the role is Admin (ignoring case)
                bool isAdmin = string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase);

                int parsedUserId = 0;

                // 2. If not an Admin, parse and validate the UserId
                if (!isAdmin)
                {
                    if (!int.TryParse(userId, out parsedUserId))
                    {
                        return JsonSerializer.Serialize(new { success = false, message = "Invalid User ID format." });
                    }
                }

                // 3. Build the query to get only private records (IsPrivate == true)
                var query = _context.TrnIndexUserMapping
                    .Where(m => m.IsPrivate);

                // 4. If not Admin, restrict results to the specific UserId
                if (!isAdmin)
                {
                    query = query.Where(m => m.UserId == parsedUserId);
                }

                // 5. Fetch the records (projecting to anonymous type or keeping entity)
                var privateIndexes = await query
                    .Select(m => new
                    {
                        m.IndexUserMapId,
                        m.IndexId,
                        m.IndexName,
                        m.UserId,
                        m.IsPrivate
                    })
                    .ToListAsync();

                // 6. Serialize the list to a JSON string and return
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    count = privateIndexes.Count,
                    data = privateIndexes
                });
            }
            catch (Exception ex)
            {
                // Return error details as a JSON string
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        public async Task<List<string>> GetPrivateIndexNoAcessThisuserId(string userId)
        {
            try
            {
                // 1. Parse string UserId to int
                if (!int.TryParse(userId, out int parsedUserId))
                {
                    return new List<string>(); // Return empty list on invalid ID
                }

                // 2. Get the list of IndexNames that the current user already has marked as private
                var currentUserPrivateIndexNames = await _context.TrnIndexUserMapping
                    .Where(m => m.UserId == parsedUserId && m.IsPrivate)
                    .Select(m => m.IndexName)
                    .Distinct()
                    .ToListAsync();

                // 3. Query private index names from OTHER users, 
                // while excluding any IndexNames that the current user already has set to private.
                var privateIndexNames = await _context.TrnIndexUserMapping
                    .Where(m => m.IsPrivate
                                && m.UserId != parsedUserId
                                && !currentUserPrivateIndexNames.Contains(m.IndexName))
                    .Select(m => m.IndexName)
                    .Distinct() // Ensures unique index names
                    .ToListAsync();

                return privateIndexNames;
            }
            catch (Exception)
            {
                // Optional: Log exception here
                return new List<string>();
            }
        }
    }
}

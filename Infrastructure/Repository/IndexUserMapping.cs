using Application.Interfaces;
using Application.Interfaces.Repository;
using Domain.DTOs.Requests;
using Domain.Entities;
using Infrastructure.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Repository
{
    public class IndexUserMapping :
        GenericRepository<TrnIndexUserMapping>, IindexUserMapping
    {
        protected new readonly ApplicationDbContext _context;

        public IndexUserMapping(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        // 1. Assign index to user with duplicate check
        public async Task<TrnIndexUserMapping> AddWithCheckIndexId(
            TrnIndexUserMapping data)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (data.IndexId <= 0 || data.UserId <= 0)
                throw new ArgumentException("Invalid Index ID or User ID.");

            var existing = await _context.TrnIndexUserMapping
                .FirstOrDefaultAsync(x =>
                    x.IndexId == data.IndexId &&
                    x.UserId == data.UserId);

            if (existing != null)
                return existing;

            await _context.TrnIndexUserMapping.AddAsync(data);
            await _context.SaveChangesAsync();

            return data;
        }

        // 2. Check index access with Role
        public async Task<bool> CheckUserIndexingExists(
            int UserId, string IndexId, string RoleName)
        {
            if (UserId <= 0 || string.IsNullOrWhiteSpace(IndexId))
                return false;

            // Admin can access any existing index
            if (string.Equals(RoleName, "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return await _context.MIndexes
                    .AnyAsync(x => x.Uuid == IndexId);
            }

            return await CheckUserIndexingExists(IndexId);
        }

        // 3. Check index access without Role
        public async Task<bool> CheckUserIndexingExists(
           string IndexId)
        {
            if (string.IsNullOrWhiteSpace(IndexId))
                return false;

            return await _context.MIndexes
                .AnyAsync(x =>
                    
                    x.Uuid== IndexId);
        }

        // 4. Assign and DeAssign users
        public async Task<bool> UserAssginAndDeAssignIndex(
     DTOIndexAssignRequest Data)
        {
            if (Data == null ||
                string.IsNullOrWhiteSpace(Data.IndexId) ||
                Data.UserIds == null)
                return false;

            var userIds = Data.UserIds.Distinct().ToHashSet();

            if (userIds.Any(x => x <= 0))
                return false;

            // Get actual integer IndexId using UUID
            var index = await _context.MIndexes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Uuid == Data.IndexId);

            if (index == null)
                return false;

            int actualIndexId = index.IndexId;

            // Get existing mappings
            var existingMappings = await _context.TrnIndexUserMapping
                .Where(x => x.IndexId == actualIndexId)
                .ToListAsync();

            // Remove unselected users
            var removeMappings = existingMappings
                .Where(x => !userIds.Contains(x.UserId))
                .ToList();

            if (removeMappings.Count > 0)
            {
                _context.TrnIndexUserMapping.RemoveRange(removeMappings);
            }

            // Get existing assigned user IDs
            var existingUserIds = existingMappings
                .Select(x => x.UserId)
                .ToHashSet();

            // Assign newly selected users
            var newMappings = userIds
                .Where(x => !existingUserIds.Contains(x))
                .Select(x => new TrnIndexUserMapping
                {
                    IndexId = actualIndexId,
                    UserId = x
                })
                .ToList();

            if (newMappings.Count > 0)
            {
                await _context.TrnIndexUserMapping.AddRangeAsync(newMappings);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        // 5. Update Index Private/Public status
        public async Task<bool> UpdateIndexPrivatePublic(
            string CrawlerId,
            bool IsPrivate,
            string userId,
            string roleName)
        {
            bool isAdmin = string.Equals(
                roleName, "Admin",
                StringComparison.OrdinalIgnoreCase);

            // Only Admin can change global index privacy
            if (!isAdmin)
                return false;

            if (string.IsNullOrWhiteSpace(CrawlerId))
                return false;

            var indexes = await _context.MIndexes
                .Where(x => x.IndexName == CrawlerId.Replace("fs-",""))
                .Take(2)
                .ToListAsync();

            // Reject missing or ambiguous index names
            if (indexes.Count != 1)
                return false;

            var index = indexes[0];

            index.IsPrivate = IsPrivate;

            await _context.SaveChangesAsync();

            return true;
        }

        // 6. Get all private indexes
        public async Task<string> GetAllPrivateIndex(
            string roleName, string userId)
        {
            bool isAdmin = string.Equals(
                roleName, "Admin",
                StringComparison.OrdinalIgnoreCase);

            var query = _context.MIndexes
                .AsNoTracking()
                .Where(x => x.IsPrivate);

            if (!isAdmin)
            {
                if (!int.TryParse(userId, out int parsedUserId)
                    || parsedUserId <= 0)
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "Invalid User ID."
                    });
                }

                query = query.Where(index =>
                    _context.TrnIndexUserMapping.Any(mapping =>
                        mapping.IndexId == index.IndexId &&
                        mapping.UserId == parsedUserId));
            }

            var privateIndexes = await query
                .Select(x => new
                {
                    x.IndexId,
                    x.IndexName,
                    x.Uuid,
                    x.IsPrivate
                })
                .ToListAsync();

            return JsonSerializer.Serialize(new
            {
                success = true,
                count = privateIndexes.Count,
                data = privateIndexes
            });
        }

        // 7. Get private indexes not accessible by current user
        public async Task<List<string>> GetPrivateIndexNoAcessThisuserId(
            string userId)
        {
            if (!int.TryParse(userId, out int parsedUserId)
                || parsedUserId <= 0)
            {
                throw new UnauthorizedAccessException(
                    "Invalid User ID.");
            }

            return await _context.MIndexes
                .AsNoTracking()
                .Where(index =>
                    index.IsPrivate &&
                    !_context.TrnIndexUserMapping.Any(mapping =>
                        mapping.IndexId == index.IndexId &&
                        mapping.UserId == parsedUserId))
                .Select(index => index.IndexName)
                .Distinct()
                .ToListAsync();
        }
    }
}
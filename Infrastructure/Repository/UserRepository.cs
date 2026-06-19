using Application.Interfaces.Repository;
using Domain.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ApplicationUser> FindByNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            var normalized = userName.Trim().ToUpper();

            return await _context.Users.FirstOrDefaultAsync(u => u.UserName != null && u.UserName.ToUpper() == normalized);
        }
    }
}

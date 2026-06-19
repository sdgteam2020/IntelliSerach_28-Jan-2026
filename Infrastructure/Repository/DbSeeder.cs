using Domain.Entities;
using Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            Microsoft.Extensions.Logging.ILogger? logger = null)
        {
            try
            {
            // Seed Ranks (idempotent, do not set identity values explicitly)
            if (!await context.MRank.AnyAsync())
            {
                var ranks = new List<MRank>
                {
                    new() { RankName = "General",                RankAbbreviation = "Gen",     Orderby = 1 },
                    new() { RankName = "Lieutenant General",     RankAbbreviation = "Lt Gen",  Orderby = 2 },
                    new() { RankName = "Major General",          RankAbbreviation = "Maj Gen", Orderby = 3 },
                    new() { RankName = "Brigadier",             RankAbbreviation = "Brig",    Orderby = 4 },
                    new() { RankName = "Colonel",               RankAbbreviation = "Col",     Orderby = 5 },
                    new() { RankName = "Colonel (Time Scale)",  RankAbbreviation = "Col(TS)", Orderby = 6 },
                    new() { RankName = "Lieutenant Colonel",    RankAbbreviation = "Lt Col",  Orderby = 7 },
                    new() { RankName = "Major",                 RankAbbreviation = "Maj",     Orderby = 8 },
                    new() { RankName = "Captain",               RankAbbreviation = "Capt",    Orderby = 9 },
                    new() { RankName = "Lieutenant",            RankAbbreviation = "Lt",      Orderby = 10 }
                };

                await context.MRank.AddRangeAsync(ranks);
                await context.SaveChangesAsync();
                logger?.LogInformation("Seeded MRank entries: {Count}", ranks.Count);
            }

            // Create Roles
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var res = await roleManager.CreateAsync(new ApplicationRole { Name = role });
                    if (!res.Succeeded)
                    {
                        logger?.LogWarning("Failed to create role {Role}: {Errors}", role, string.Join(';', res.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        logger?.LogInformation("Created role {Role}", role);
                    }
                }
            }

            // Create Admin User
            var adminUser = await userManager.FindByNameAsync("admin");

            if (adminUser == null)
            {
                // Choose a sensible default RankId from seeded ranks
                var defaultRank = await context.MRank.OrderBy(r => r.Orderby).FirstOrDefaultAsync();

                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Name = "Administrator",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    Active = true,
                    RankId = defaultRank != null ? defaultRank.RankId : (short)0,
                    UpdatedOn = DateTime.UtcNow,
                    Updatedby = 0
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    var addRoleRes = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!addRoleRes.Succeeded)
                    {
                            logger?.LogWarning("Failed to add admin user to role Admin: {Errors}", string.Join(';', addRoleRes.Errors.Select(e => e.Description)));
                    }
                    else
                    {

                            await userManager.AddToRoleAsync(adminUser, "Admin");
                            var claims = new[] { new Claim("User Approval", "1") };
                            await userManager.AddClaimsAsync(adminUser, claims);

                            // Add login info and registration token similar to previous behavior
                            var loginInfo = new UserLoginInfo("IntelliSearch", adminUser.Id.ToString(), "Indian Army IntelliSearch");
                            await userManager.AddLoginAsync(adminUser, loginInfo);
                            await userManager.SetAuthenticationTokenAsync(adminUser, "IntelliSearch", "RegistrationToken", System.Guid.NewGuid().ToString());
                            // Seed Role Claims
                            logger?.LogInformation("Created admin user and assigned Admin role");
                    }
                }
                else
                {
                    logger?.LogWarning("Failed to create admin user: {Errors}", string.Join(';', result.Errors.Select(e => e.Description)));
                }
            }
               

             
                var adminRole = await roleManager.FindByNameAsync("Admin");

            if (adminRole != null)
            {
                var claims = await roleManager.GetClaimsAsync(adminRole);

                if (!claims.Any(c => c.Type == "User Approval"))
                {
                    var addClaimRes = await roleManager.AddClaimAsync(adminRole, new Claim("User Approval", "1"));
                    if (addClaimRes.Succeeded)
                        logger?.LogInformation("Added claim 'User Approval' to Admin role");
                    else
                        logger?.LogWarning("Failed to add claim to Admin role");
                }
            }

            var userRole = await roleManager.FindByNameAsync("User");

            if (userRole != null)
            {
                var claims = await roleManager.GetClaimsAsync(userRole);

                if (!claims.Any(c => c.Type == "Manage Application"))
                {
                    var addClaimRes = await roleManager.AddClaimAsync(userRole, new Claim("Manage Application", "2"));
                    if (addClaimRes.Succeeded)
                        logger?.LogInformation("Added claim 'Manage Application' to User role");
                    else
                        logger?.LogWarning("Failed to add claim to User role");
                }
            }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error while running DbSeeder");
                throw;
            }
        }
    }
}

using Domain.Entities;
using Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<MRank> MRank { get; set; } = null!;
        public DbSet<TrnUploadFiles> trnUploadFiles { get; set; } = null!;
        public DbSet<TrnWebServer> trnWebServer { get; set; } = null!;
        public DbSet<WebScraperSetting> WebScraperSetting { get; set; } = null!;
        public DbSet<ExceptionLog> Log { get; set; }
        public DbSet<TrnIndexUserMapping> TrnIndexUserMapping { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Ignore(x => x.Email);
                entity.Ignore(x => x.EmailConfirmed);
                entity.Ignore(x => x.NormalizedEmail);
                entity.Ignore(x => x.PhoneNumber);
                entity.Ignore(x => x.PhoneNumberConfirmed);
                entity.Ignore(x => x.TwoFactorEnabled);
 
            });
         

        }
    }
}
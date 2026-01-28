using DataTransferObject.IdentityModel;
using DataTransferObject.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<MRank> MRank { get; set; } = null!; 
        public DbSet<TrnUploadFiles> trnUploadFiles { get; set; } = null!; 
        public DbSet<ExceptionLog> Log { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MRank>()
                .HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MRank>()
                .HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);


        }

    }
}

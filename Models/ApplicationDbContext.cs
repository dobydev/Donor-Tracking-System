using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DonorTrackingSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets for donation tracking
        public DbSet<Donation> Donations { get; set; }
        public DbSet<FundDesignation> FundDesignations { get; set; }
        public DbSet<Congregant> Congregants { get; set; }
        public DbSet<NonMember> NonMembers { get; set; }
        public DbSet<Committee> Committees { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize AspNetUsers table to only include required fields
            builder.Entity<ApplicationUser>(entity =>
            {
                // Ignore unnecessary Identity fields
                entity.Ignore(u => u.Email);
                entity.Ignore(u => u.NormalizedEmail);
                entity.Ignore(u => u.EmailConfirmed);
                entity.Ignore(u => u.PhoneNumber);
                entity.Ignore(u => u.PhoneNumberConfirmed);
                entity.Ignore(u => u.TwoFactorEnabled);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.AccessFailedCount);
            });

            // Seed some default fund designations
            builder.Entity<FundDesignation>().HasData(
                new FundDesignation { ID = 1, Name = "General Fund", ActiveStatus = true },
                new FundDesignation { ID = 2, Name = "Building Fund", ActiveStatus = true },
                new FundDesignation { ID = 3, Name = "Missions", ActiveStatus = true },
                new FundDesignation { ID = 4, Name = "Benevolence", ActiveStatus = true }
            );
        }
    }
}

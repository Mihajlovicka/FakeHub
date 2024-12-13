using FakeHubApi.Model.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Organization> Organizations { get; set; }
    public virtual DbSet<Team> Teams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<IdentityRole<int>>()
            .HasData(
                new IdentityRole<int>
                {
                    Id = 1,
                    Name = Role.SUPERADMIN.ToString(),
                    NormalizedName = Role.SUPERADMIN.ToString(),
                },
                new IdentityRole<int>
                {
                    Id = 2,
                    Name = Role.ADMIN.ToString(),
                    NormalizedName = Role.ADMIN.ToString(),
                },
                new IdentityRole<int>
                {
                    Id = 3,
                    Name = Role.USER.ToString(),
                    NormalizedName = Role.USER.ToString(),
                }
            );

        modelBuilder
            .Entity<Organization>()
            .HasOne(o => o.Owner)
            .WithMany(u => u.OwnedOrganizations)
            .HasForeignKey(o => o.OwnerId);

        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Organizations)
            .WithMany(o => o.Users)
            .UsingEntity<Dictionary<string, object>>(
                "UserOrganization",
                j => j.HasOne<Organization>().WithMany().HasForeignKey("OrganizationId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
            );
    }
}

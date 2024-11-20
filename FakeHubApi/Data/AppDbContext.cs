using FakeHubApi.Model.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public virtual DbSet<User> Users { get; set; }

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
    }
}

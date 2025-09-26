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
    
    public virtual DbSet<UserOrganization> UserOrganizations { get; set; }

    public virtual DbSet<Model.Entity.Repository> Repositories { get; set; }
   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Identity roles
        modelBuilder
            .Entity<IdentityRole<int>>()
            .HasData(
                new IdentityRole<int> { Id = 1, Name = Role.SUPERADMIN.ToString(), NormalizedName = Role.SUPERADMIN.ToString() },
                new IdentityRole<int> { Id = 2, Name = Role.ADMIN.ToString(), NormalizedName = Role.ADMIN.ToString() },
                new IdentityRole<int> { Id = 3, Name = Role.USER.ToString(), NormalizedName = Role.USER.ToString() }
            );

        // Configure Organization and Owner relationship
        modelBuilder
            .Entity<Organization>()
            .HasOne(o => o.Owner)
            .WithMany(u => u.OwnedOrganizations)
            .HasForeignKey(o => o.OwnerId);

        // Configure UserOrganization
        modelBuilder.Entity<UserOrganization>(entity =>
        {
            // Define composite key
            entity.HasKey(uo => new { uo.UserId, uo.OrganizationId });

            // Configure relationships
            entity.HasOne(uo => uo.User)
                .WithMany(u => u.UserOrganizations)
                .HasForeignKey(uo => uo.UserId);

            entity.HasOne(uo => uo.Organization)
                .WithMany(o => o.UserOrganizations)
                .HasForeignKey(uo => uo.OrganizationId);

            // Add global query filter for soft delete
            entity.HasQueryFilter(uo => uo.Active);
        });

        // Configure many-to-many relationship using UserOrganization
        modelBuilder.Entity<User>()
            .HasMany(u => u.Organizations)
            .WithMany(o => o.Users)
            .UsingEntity<UserOrganization>(
                j => j.HasOne(uo => uo.Organization)
                    .WithMany(o => o.UserOrganizations)
                    .HasForeignKey(uo => uo.OrganizationId),
                j => j.HasOne(uo => uo.User)
                    .WithMany(u => u.UserOrganizations)
                    .HasForeignKey(uo => uo.UserId)
            );

        modelBuilder
            .Entity<Team>()
            .HasMany(t => t.Users)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserTeam",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j => j.HasOne<Team>().WithMany().HasForeignKey("TeamId")
            );

        modelBuilder
            .Entity<Model.Entity.Repository>()
            .HasMany(r => r.Collaborators)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RepositoryCollaborator",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j => j.HasOne<Model.Entity.Repository>().WithMany().HasForeignKey("RepositoryId")
            );
    }

}

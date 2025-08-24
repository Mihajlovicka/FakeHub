using FakeHubApi.Data;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class OrganizationRepository(AppDbContext context)
    : CrudRepository<Organization>(context),
        IOrganizationRepository
{
    public Task<Organization?> GetByName(string name) =>
        _context
            .Organizations.Include(x => x.Owner)
            .Include(x => x.Teams)
                .ThenInclude(t => t.Users)
            .Include("Teams.Repository")
            .Include(x => x.Users)
            .FirstOrDefaultAsync(x => x.Name == name && x.Active);

    public Task<List<Organization>> GetByUser(int userId) =>
        _context
            .Organizations.Where(x =>
                x.Active && (x.OwnerId == userId || x.Users.Any(u => u.Id == userId))
            )
            .Include(x => x.Owner)
            .Include(x => x.Users)
            .ToListAsync();

    public Task<List<Organization>> Search(string? query, int userId) =>
        _context
            .Organizations.Where(x =>
                x.Active
                && (string.IsNullOrEmpty(query) || EF.Functions.Like(x.Name, $"%{query}%"))
                && (x.OwnerId == userId || x.Users.Any(u => u.Id == userId))
            )
            .Include(x => x.Owner)
            .Include(x => x.Users)
            .ToListAsync();
}

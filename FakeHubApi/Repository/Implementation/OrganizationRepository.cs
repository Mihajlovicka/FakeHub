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
            .FirstOrDefaultAsync(x => x.Name == name);

    public Task<List<Organization>> GetByUser(int userId) =>
        _context.Organizations.Where(x => x.OwnerId == userId).Include(x => x.Owner).ToListAsync();

    public Task<List<Organization>> Search(string query, int userId) =>
        _context
            .Organizations.Where(x =>
                EF.Functions.Like(x.Name, $"%{query}%") && x.OwnerId == userId
            )
            .Include(x => x.Owner)
            .ToListAsync();
}

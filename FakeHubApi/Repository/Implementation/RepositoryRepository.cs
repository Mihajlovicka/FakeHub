using FakeHubApi.Data;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class RepositoryRepository(AppDbContext context): CrudRepository<Model.Entity.Repository>(context), IRepositoryRepository
{
    public async Task<Model.Entity.Repository?> GetByOwnerAndName(RepositoryOwnedBy ownedBy, int ownerId, string name)
    {
        return await _context.Repositories.FirstOrDefaultAsync(x =>
            x.Name.Equals(name) && x.OwnerId == ownerId && x.OwnedBy.Equals(ownedBy));
    }

    public async Task<IEnumerable<Model.Entity.Repository>> GetUserRepositoriesByOwnerId(int ownerId, bool onlyPublic = false)
    {
        var ownedRepositories = await _context.Repositories
            .Where(r => r.OwnedBy > 0 && r.OwnerId == ownerId && (onlyPublic ? !r.IsPrivate : true))
            .ToListAsync();

        var userOrganizations = await _context.UserOrganizations
            .Where(ou => ou.UserId == ownerId)
            .Select(ou => ou.OrganizationId)
            .ToListAsync();

        var ownedOrganizations = await _context.Organizations
            .Where(o => o.OwnerId == ownerId)
            .Select(o => o.Id)
            .ToListAsync();

        var allOrganizations = userOrganizations.Concat(ownedOrganizations).Distinct().ToList();

        var organizationRepositories = await _context.Repositories
            .Where(r => allOrganizations.Contains(r.OwnerId) && r.OwnedBy == RepositoryOwnedBy.Organization && (onlyPublic ? !r.IsPrivate : true))
            .ToListAsync();

        return ownedRepositories.Concat(organizationRepositories);
    }
}
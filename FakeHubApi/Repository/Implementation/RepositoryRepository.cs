using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
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

        var ownedOrganizations = await _context.Organizations
            .Where(o => o.OwnerId == ownerId)
            .Select(o => o.Id)
            .ToListAsync();

        var organizationRepositories = await _context.Repositories
            .Where(r => ownedOrganizations.Contains(r.OwnerId) && r.OwnedBy == RepositoryOwnedBy.Organization && (onlyPublic ? !r.IsPrivate : true))
            .ToListAsync();

        return ownedRepositories.Concat(organizationRepositories);
    }

    public async Task<IEnumerable<Model.Entity.Repository>> GetOrganizationRepositoriesByOrganizationId(int organizationId)
    {
        var ownedRepositories = await _context.Repositories
           .Where(r => r.OwnedBy == RepositoryOwnedBy.Organization && r.OwnerId == organizationId)
           .ToListAsync();

        return ownedRepositories;
    }
    
     public async Task<IEnumerable<Model.Entity.Repository>> SearchByOwnerId(string? query, int ownerId)
    {
        var ownedRepositoriesQuery = _context.Repositories
            .Where(r => r.OwnedBy > 0 && r.OwnerId == ownerId);

        var userOrganizations = await _context.UserOrganizations
            .Where(ou => ou.UserId == ownerId)
            .Select(ou => ou.OrganizationId)
            .ToListAsync();

        var ownedOrganizations = await _context.Organizations
            .Where(o => o.OwnerId == ownerId)
            .Select(o => o.Id)
            .ToListAsync();

        var allOrganizations = userOrganizations.Concat(ownedOrganizations).Distinct().ToList();

        var organizationRepositoriesQuery = _context.Repositories
            .Where(r => allOrganizations.Contains(r.OwnerId) && r.OwnedBy == RepositoryOwnedBy.Organization);

        if (!string.IsNullOrWhiteSpace(query))
        {
            ownedRepositoriesQuery = ownedRepositoriesQuery
                .Where(r => EF.Functions.Like(r.Name, $"%{query}%") 
                            || EF.Functions.Like(r.Description, $"%{query}%"));

            organizationRepositoriesQuery = organizationRepositoriesQuery
                .Where(r => EF.Functions.Like(r.Name, $"%{query}%") 
                            || EF.Functions.Like(r.Description, $"%{query}%"));
        }

        var ownedRepositories = await ownedRepositoriesQuery.ToListAsync();
        var organizationRepositories = await organizationRepositoriesQuery.ToListAsync();

        return ownedRepositories.Concat(organizationRepositories);
    }

    public async Task<IEnumerable<Model.Entity.Repository>> SearchAllAsync(string? query)
    {
        var repositoriesQuery = _context.Repositories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            repositoriesQuery = repositoriesQuery
                .Where(r => EF.Functions.Like(r.Name, $"%{query}%") 
                            || EF.Functions.Like(r.Description, $"%{query}%"));
        }

        return await repositoriesQuery.ToListAsync();
    }

    public async Task<IEnumerable<Model.Entity.Repository>> GetAllPublicRepositories(RepositorySearchDto filters)
    {
        var query = _context.Repositories
            .Where(r => !r.IsPrivate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Name))
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{filters.Name}%"));

        if (!string.IsNullOrWhiteSpace(filters.Description))
            query = query.Where(r => EF.Functions.Like(r.Description, $"%{filters.Description}%"));

        if (filters.Badges != null && filters.Badges.Any())
            query = query.Where(r => filters.Badges.Contains(r.Badge));

        if (!string.IsNullOrEmpty(filters.AuthorName))
        {
            query = query.Where(r => (filters.AuthorUserIds.Count > 0 ? r.OwnedBy == RepositoryOwnedBy.User && filters.AuthorUserIds.Contains(r.OwnerId) : false) 
            || (filters.AuthorOrganizationIds.Count > 0 ? r.OwnedBy == RepositoryOwnedBy.Organization && filters.AuthorOrganizationIds.Contains(r.OwnerId) : false));
        }

        if (filters.GeneralTerms.Count > 0)
        {
            foreach (var term in filters.GeneralTerms)
            {
                var like = $"%{term}%";
                query = query.Where(r =>
                    EF.Functions.Like(r.Name, like) ||
                    EF.Functions.Like(r.Description, like));
            }
        }

        return await query.ToListAsync();
    }

    public async Task<Model.Entity.Repository?> GetByIdWithCollaboratorsAsync(int repositoryId)
    {
        var repo = await _dbSet.FindAsync(repositoryId);
        if (repo == null)
            return null;

        await _context.Entry(repo)
            .Collection(r => r.Collaborators)
            .LoadAsync();

        return repo;
    }

    public async Task<IEnumerable<Model.Entity.Repository>> GetContributedByUserIdAsync(int userId, bool onlyPublic = false)
    {
        var userTeams = await _context.Teams
            .Where(t => t.Users.Any(u => u.Id == userId) && (onlyPublic ? !t.Repository.IsPrivate : true))
            .ToListAsync();
        var organizationRepoIds = userTeams.Select(t => t.RepositoryId);

        var contributedRepositories = await _context.Repositories
            .Where(r => (r.Collaborators.Any(u => u.Id == userId) || organizationRepoIds.Any(repoId => repoId == r.Id)) 
                        && (onlyPublic ? !r.IsPrivate : true))
            .ToListAsync();

        return contributedRepositories;
    }
}
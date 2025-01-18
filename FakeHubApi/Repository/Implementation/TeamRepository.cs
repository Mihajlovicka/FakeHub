using FakeHubApi.Data;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class TeamRepository(AppDbContext context) : CrudRepository<Team>(context), ITeamRepository
{
    public async Task<Team?> GetTeam(string organizationName, string teamName)
    {
        return await context
            .Teams.Include(x => x.Organization)
            .ThenInclude(x => x.Users)
            .Include(x => x.Organization)
            .ThenInclude(x => x.Owner)
            .Include(x => x.Repository)
            .Include(x => x.Users)
            .FirstOrDefaultAsync(x =>
                x.Organization.Name == organizationName && x.Name == teamName && x.Active
            );
    }
    public async Task<List<Team>> GetAllByRepositoryIdAsync(int repositoryId)
    {
        return await context.Teams
            .Include(t => t.Organization)
            .ThenInclude(o => o.Users)
            .Include(t => t.Organization)
            .ThenInclude(o => o.Owner)
            .Include(t => t.Repository)
            .Include(t => t.Users)
            .Where(t => t.Repository.Id == repositoryId && t.Active)
            .ToListAsync();
    }
}

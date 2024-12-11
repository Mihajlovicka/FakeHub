using FakeHubApi.Data;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;

namespace FakeHubApi.Repository.Implementation;

public class TeamRepository(AppDbContext context) : CrudRepository<Team>(context), ITeamRepository
{
    public Task<Team?> GetByName(string name)
    {
        throw new NotImplementedException();
    }
}

using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface ITeamRepository : ICrudRepository<Team>
{
    Task<Team?> GetByName(string name);
    Task<Team?> GetTeam(string organizationName, string teamName);
}

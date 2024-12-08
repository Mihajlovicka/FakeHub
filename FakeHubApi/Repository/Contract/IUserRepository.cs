using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IUserRepository : ICrudRepository<User>
{
    Task<List<User>> GetUsersByQueries(List<string> queriesUsername, List<string> queriesEmail, Role role);
}
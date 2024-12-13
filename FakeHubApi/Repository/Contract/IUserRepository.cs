using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IUserRepository : ICrudRepository<User>
{
    Task<List<User>> GetUsersByQueries(List<string> queriesUsername, List<string> queriesEmail, Role role);
    Task<User> GetByUsername(string username);
    Task<List<Organization>> GetOwnedOrganizationsByUsername(string username);
    Task<List<Organization>> GetAllOrganizationsByUsername(string username);
}
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IUserRepository : ICrudRepository<User>
{
    Task<User?> GetByUsername(string username);
    Task<List<Organization>> GetOwnedOrganizationsByUsername(string username);
    Task<List<Organization>> GetAllOrganizationsByUsername(string username);
    Task<List<User>> GetUsersByRoleAsync(string roleName);
    Task<List<User>> GetUsersByUsernameContaining(string username);
}

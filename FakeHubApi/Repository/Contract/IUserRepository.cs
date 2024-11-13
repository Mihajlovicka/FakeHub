using FakeHubApi.Model;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IUserRepository : ICrudRepository<User>
{
    Task<User> GetByUsername(string username);
}

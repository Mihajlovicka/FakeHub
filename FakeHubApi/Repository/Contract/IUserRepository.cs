using FakeHubApi.Model;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IUserRepository : ICrudRepository<ApplicationUser>
{
    Task<ApplicationUser> GetByUsername(string username);
}
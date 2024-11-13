using FakeHubApi.Model.Entity;

namespace FakeHubApi.Service.Contract;

public interface IUserContextService
{
    Task<User> GetCurrentUserAsync();
}

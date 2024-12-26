using FakeHubApi.Model.Entity;

namespace FakeHubApi.Service.Contract;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IEnumerable<string> roles);
}

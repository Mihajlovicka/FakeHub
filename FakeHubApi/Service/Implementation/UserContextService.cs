using FakeHubApi.Mapper;
using FakeHubApi.Model.Entity;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Service.Implementation;

public class UserContextService(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor
) : IUserContextService
{
    public async Task<User> GetCurrentUserAsync()
    {
        var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");
        return user;
    }

    public async Task<(User, string)> GetCurrentUserWithRoleAsync()
    {
        var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        var userRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();

        return (user, userRole);
    }
}

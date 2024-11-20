using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FakeHubApi.Helpers;

public class NoRoleRequirement: IAuthorizationRequirement { }

public class NoRoleHandler : AuthorizationHandler<NoRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NoRoleRequirement requirement)
    {
        var hasRole = context.User.HasClaim(c => c.Type == ClaimTypes.Role);
        if (!hasRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
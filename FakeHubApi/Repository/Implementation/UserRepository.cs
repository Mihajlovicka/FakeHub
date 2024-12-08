using FakeHubApi.Data;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class UserRepository(AppDbContext context) : CrudRepository<User>(context), IUserRepository
{
    public async Task<List<User>> GetUsersByQueries(List<string> queriesUsername, List<string> queriesEmail, Role role)
    {
        var usersByRole = await GetUsersByRoleAsync(role.ToString());
        if (queriesEmail.Count <= 0 || queriesUsername.Count <= 0) return await Task.FromResult(usersByRole);
        
        var result = new List<User>();
        foreach (var q in queriesUsername)
        {
            var userNameExact = usersByRole.Where(u => u.UserName.Equals(q)).ToList();

            var userNameContains = usersByRole.Where(u => u.UserName.Contains(q)).ToList();

            result = result.Union(userNameExact)
                .Union(userNameContains)
                .ToList();
        }

        foreach (var q in queriesEmail)
        {
            var emailExact = usersByRole.Where(u => u.Email.Equals(q)).ToList();

            var emailContains = usersByRole.Where(u => u.Email.Contains(q)).ToList();

            result = result.Union(emailExact)
                .Union(emailContains)
                .ToList();
        }

        return await Task.FromResult(
            result
                .OrderBy(u => !queriesUsername.Contains(u.UserName) ? 1 : 0)
                .ThenBy(u => !queriesUsername.Contains(u.Email) ? 1 : 0)
                .ThenBy(u => queriesUsername.Any(q => u.UserName.Contains(q)) ? 0 : 1)
                .ThenBy(u => queriesUsername.Any(q => u.Email.Contains(q)) ? 0 : 1)
                .ToList()
        );
    }
    
    private async Task<List<User>> GetUsersByRoleAsync(string roleName)
    {
        var users = await context.Users
            .Where(u => context.UserRoles
                .Any(ur => ur.UserId == u.Id && 
                           context.Roles.Any(r => r.Id == ur.RoleId && r.Name == roleName)))
            .ToListAsync();

        return users;
    }
}
using FakeHubApi.Data;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class UserRepository(AppDbContext context) : CrudRepository<User>(context), IUserRepository
{
    
    public async Task<List<User>> GetUsersByRoleAsync(string roleName)
    {
        var users = await context.Users
            .Where(u => context.UserRoles
                .Any(ur => ur.UserId == u.Id && 
                           context.Roles.Any(r => r.Id == ur.RoleId && r.Name == roleName)))
            .ToListAsync();

        return users;
    }
    public async Task<User?> GetByUsername(string username) =>
        await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);

    public Task<List<Organization>> GetOwnedOrganizationsByUsername(string username) =>
         _context.Users
            .Where(x => x.UserName == username)
            .SelectMany(u => u.OwnedOrganizations)
            .ToListAsync();

    public Task<List<Organization>> GetAllOrganizationsByUsername(string username) =>
         _context.Users
            .Where(x => x.UserName == username)
            .SelectMany(u => u.Organizations
                .Concat(u.OwnedOrganizations))
            .ToListAsync();
}
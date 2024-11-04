using FakeHubApi.Data;
using FakeHubApi.Model;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class UserRepository(AppDbContext context) : CrudRepository<ApplicationUser>(context), IUserRepository
{
    public async Task<ApplicationUser> GetByUsername(string username)
        => await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == username);
}
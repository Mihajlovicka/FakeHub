using FakeHubApi.Data;
using FakeHubApi.Model;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class UserRepository(AppDbContext context) : CrudRepository<User>(context), IUserRepository
{
    public async Task<User> GetByUsername(string username) =>
        await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);
}
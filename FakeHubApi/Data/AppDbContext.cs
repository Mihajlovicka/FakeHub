using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
}

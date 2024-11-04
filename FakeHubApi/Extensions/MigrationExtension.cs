using FakeHubApi.Data;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Extensions;

public static class MigrationExtensions
{
    public static void ApplyPendingMigrations(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }
}

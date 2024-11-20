using FakeHubApi.Model.Entity;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Data;

public static class AppDbContextSeed
{
    public static async Task SeedSuperAdminUserAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        
        foreach (var role in Enum.GetValues<Role>())
        {
            if (!await roleManager.RoleExistsAsync(role.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = role.ToString(), NormalizedName = role.ToString().ToUpper() });
            }
        }
        var superAdminUser = await userManager.FindByNameAsync("superadmin");

        if (superAdminUser == null)
        {
            superAdminUser = new User
            {
                UserName = "superadmin",
                Email = "superadmin@example.com",
                TwoFactorEnabled = false,
                EmailConfirmed = true
            };
            var password = configuration["Constants:password"];

            if (password != null)
            {
                await userManager.CreateAsync(superAdminUser, password);

                await userManager.AddToRoleAsync(superAdminUser, "SUPERADMIN");
            }
        }
        
        await SavePassToTxt(configuration);
    }

    private static async Task SavePassToTxt(IConfiguration configuration)
    {
        var filePath = configuration["Constants:passwordPath"];
        var password = configuration["Constants:password"];
        
        if (!File.Exists(filePath) && filePath != null && password != null)
        {
            await File.WriteAllTextAsync(filePath, password);
        }
    }
}
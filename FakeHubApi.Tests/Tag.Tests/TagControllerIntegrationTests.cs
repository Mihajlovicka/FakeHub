using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace FakeHubApi.Tests.Tag.Tests;

public class TagControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        await SetupData();
        var token = await GetTokenFromSuccessfulUserLogin(
            new LoginRequestDto { Email = "test@example.com", Password = "Password123!" }
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test, Order(1)]
    public async Task CanUserDelete_ReturnsTrue_WhenUserIsOwnerOfOrgRepository()
    {
        var response = await _client.GetAsync("/api/repositories/tag/2/canUserDelete");
        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(bool.TryParse(responseObj?.Result?.ToString(), out var canDelete) && canDelete, Is.True);
        });
    }

    [Test, Order(2)]
    public async Task CanUserDelete_ReturnsFalse_WhenUserIsNotAllowed()
    {
        var token = await GetTokenFromSuccessfulUserLogin(
            new LoginRequestDto { Email = "testtest@example.com", Password = "Password123!" }
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
        var response = await _client.GetAsync("/api/repositories/tag/2/canUserDelete");
        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.True);
           Assert.That(bool.TryParse(responseObj?.Result?.ToString(), out var canDelete) && canDelete, Is.False);
        });
    }

    [Test, Order(3)]
    public async Task CanUserDelete_ReturnsFalse_WhenRepositoryNotFound()
    {
        var response = await _client.GetAsync("/api/repositories/tag/9999/canUserDelete");
        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(bool.TryParse(responseObj?.Result?.ToString(), out var canDelete) && canDelete, Is.False);
        });
    }

    private async Task SetupData()
    {
        using var scope = _factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        if (!await roleManager.RoleExistsAsync("USER"))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "USER" });
        }

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test@example.com",
            PasswordHash =
                "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
            SecurityStamp = "Q7++M6z5N+Tly9yfor8HhJxhg52bNmZ",
        };

        var user2 = new User
        {
            Id = 2,
            Email = "testtest@example.com",
            UserName = "testtest@example.com",
            PasswordHash =
                "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
            SecurityStamp = "Q7++M6z5N+Tly9yfor8HhJxhg52bNmZ",
        };

        await db.Users.AddAsync(user);
        await db.Users.AddAsync(user2);
        await db.SaveChangesAsync();
        await userManager.AddToRolesAsync(user, new[] { "USER" });
        await userManager.AddToRolesAsync(user2, new[] { "USER" });

        var organization = new Model.Entity.Organization
        {
            Id = 1,
            Name = "Test Team Organization",
            Description = "Test Description",
            ImageBase64 = "",
            Owner = user,
            OwnerId = user.Id,
        };

        await db.Organizations.AddAsync(organization);

        var repository2 = new Model.Entity.Repository
        {
            Id = 2,
            Name = "Test Repository 2",
            Description = "Test Repository Description 2",
            IsPrivate = false,
            OwnedBy = RepositoryOwnedBy.Organization,
            OwnerId = organization.Id
        };

        await db.Repositories.AddAsync(repository2);
        await db.SaveChangesAsync();
    }

    private async Task<string> GetTokenFromSuccessfulUserLogin(LoginRequestDto loginRequest)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var responseObj = await loginResponse.Content.ReadFromJsonAsync<ResponseBase>();
        var loginResponseDtoString = responseObj?.Result?.ToString() ?? string.Empty;

        var loginResponseDtoObject = JsonConvert.DeserializeObject<LoginResponseDto>(
            loginResponseDtoString
        );

        return loginResponseDtoObject?.Token ?? "";
    }
}

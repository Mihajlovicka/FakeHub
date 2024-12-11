using System.Net.Http.Headers;
using System.Net.Http.Json;
using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace FakeHubApi.Tests.Team.Tests;

public class TeamControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    private const string imageBase64 =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/wQAAwAC/1EpeEkAAAAASUVORK5CYII=";

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
    public async Task AddTeam()
    {
        var teamDto = new TeamDto
        {
            Name = "Test Team",
            Description = "Test Description",
            OrganizationName = "Test Team Organization",
        };

        var response = await _client.PostAsJsonAsync("/api/organization/team", teamDto);

        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(responseObj?.Result, Is.Null);
        });
    }

    [Test, Order(2)]
    public async Task AddTeam_Fails_NameNotUnique()
    {
        var teamDto = new TeamDto
        {
            Name = "Test Team",
            Description = "Test Description",
            OrganizationName = "Test Team Organization",
        };

        var response = await _client.PostAsJsonAsync("/api/organization/team", teamDto);

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(responseObj?.ErrorMessage, Is.EqualTo("Team name is not unique."));
        });
    }

    [Test, Order(3)]
    public async Task AddTeam_Fails_NotAuthorized()
    {
        var token = await GetTokenFromSuccessfulUserLogin(
            new LoginRequestDto { Email = "testtest@example.com", Password = "Password123!" }
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
        var teamDto = new TeamDto
        {
            Name = "Test Team 2",
            Description = "Test Description",
            OrganizationName = "Test Team Organization",
        };

        var response = await _client.PostAsJsonAsync("/api/organization/team", teamDto);

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(
                responseObj?.ErrorMessage,
                Is.EqualTo("You are not the owner of this organization.")
            );
        });
    }

    private async Task SetupData()
    {
        using var scope = _factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<
            RoleManager<IdentityRole<int>>
        >();

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
            UserName = "testest@example.com",
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
            ImageBase64 = imageBase64,
            Owner = user,
            OwnerId = user.Id,
        };

        await db.Organizations.AddAsync(organization);
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

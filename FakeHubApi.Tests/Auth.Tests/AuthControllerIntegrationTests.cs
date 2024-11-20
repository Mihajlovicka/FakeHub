using System.Net.Http.Json;
using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FakeHubApi.Tests.Auth.Tests;

public class AuthControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    [OneTimeSetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        SetupDbData().Wait();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Register_UserCreatedSuccessfully_ReturnsOk()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "register@example.com",
            Username = "RegisterUserName",
            Password = "Password123!",
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequestDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(responseObj?.Success, Is.True);
    }

    [Test]
    public async Task Register_ErrorReturned_ValidationError()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "testexample",
            Username = "RegisterErrorUserName",
            Password = "Password123!",
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequestDto);

        // Assert
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(responseObj?.Success, Is.False);
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!",
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(resp is { Success: true }, Is.True);
    }

    [Test]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "wrong@example.com",
            Password = "WrongPassword!",
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(resp is { Success: true }, Is.False);
    }

    private async Task SetupDbData()
    {
        using var scope = _factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<
            UserManager<User>
        >();
        var roleManager = scope.ServiceProvider.GetRequiredService<
            RoleManager<IdentityRole<int>>
        >();

        string[] roles = ["USER"];
        var i = 1;
        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role)) continue;
            await roleManager.CreateAsync(new IdentityRole<int> { Id = i, Name = role });
            i++;
        }
        // Seed the database with a user
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test@example.com",
            PasswordHash =
                "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
            SecurityStamp = "Q7++M6z5N+Tly9yfor8HhJxhg52bNmZ",
        };

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        await userManager.AddToRolesAsync(user, new[] { "USER" });
    }
}

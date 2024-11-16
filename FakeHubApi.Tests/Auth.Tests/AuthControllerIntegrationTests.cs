using System.Net;
using System.Net.Http.Json;
using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
        Assert.IsTrue(responseObj?.Success);
    }

    [Test]
    public async Task Register_ErrorReturned_VaidationError()
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
        Assert.IsFalse(responseObj?.Success);
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
        Assert.IsTrue(resp.Success);
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
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.IsFalse(resp.Success);
    }

    private async Task SetupDbData()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<
                UserManager<User>
            >();
            var roleManager = scope.ServiceProvider.GetRequiredService<
                RoleManager<IdentityRole<int>>
            >();

            string[] roles = { "USER" };
            int i = 1;
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Id = i, Name = role });
                    i++;
                }
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

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            await userManager.AddToRolesAsync(user, new[] { "USER" });
        }
    }

    [Test]
    public async Task GetUserProfileByUsername_ValidUsername_ReturnsOk()
    {
        // Arrange
        var username = "test@example.com";

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{username}");
        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var responseUserString = responseObj?.Result?.ToString() ?? string.Empty;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(responseObj?.Result, Is.Not.Null);
            Assert.That(responseUserString, Is.Not.Empty);
        });

        var responseUserObject = JsonConvert.DeserializeObject<UserProfileResponseDto>(responseUserString);
        Assert.Multiple(() =>
        {
            Assert.That(responseUserObject, Is.Not.Null);
            Assert.That(responseUserObject?.Username, Is.Not.Null);
            Assert.That(responseUserObject?.Username, Is.EqualTo(username));
        });
    }

    [Test]
    public async Task GetUserProfileByUsername_ValidUsernameWithDifferentCase_ReturnsOk()
    {
        // Arrange
        var username = "TEST@EXAMPLE.COM";

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{username}");
        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var responseUserString = responseObj?.Result?.ToString() ?? string.Empty;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseUserString, Is.Not.Empty);
        });

        var responseUserObject = JsonConvert.DeserializeObject<UserProfileResponseDto>(responseUserString);
        Assert.Multiple(() =>
        {
            Assert.That(responseUserObject, Is.Not.Null);
            Assert.That(responseUserObject?.Username, Is.Not.Null);
            Assert.That(responseUserObject?.Username, Is.EqualTo(username.ToLower()));
        });
    }

    [Test]
    public async Task GetUserProfileByUsername_NonExistentUsername_ReturnsNotFound()
    {
        // Arrange
        var username = "nonexistentuser";

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{username}");
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(responseObj?.ErrorMessage, Is.Not.Empty);
            Assert.That(responseObj?.ErrorMessage, Is.EqualTo("User not found"));
            Assert.That(responseObj?.Result, Is.Null);
        });
    }
}
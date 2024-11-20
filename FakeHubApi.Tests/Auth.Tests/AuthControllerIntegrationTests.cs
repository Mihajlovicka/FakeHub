using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FakeHubApi.Tests.Auth.Tests;

public class AuthControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        await SetupDbData();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test, Order(1)]
    public async Task Register_UserCreatedSuccessfully_ReturnsOk()
    {
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "register@example.com",
            Username = "RegisterUserName",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequestDto);

        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(responseObj?.Success, Is.True);
    }

    [Test, Order(2)]
    public async Task Register_ErrorReturned_ValidationError()
    {
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "testexample",
            Username = "RegisterErrorUserName",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequestDto);

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(responseObj?.Success, Is.False);
    }

    [Test, Order(3)]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.EnsureSuccessStatusCode();
        var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(resp?.Success, Is.True);
    }

    [Test, Order(4)]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "wrong@example.com",
            Password = "WrongPassword!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(resp?.Success, Is.False);
    }
    
    [Test, Order(5)]
    public async Task RegisterAdmin_UserIsSuperAdmin_AdminCreatedSuccessfully_ReturnsOk()
    {
        var adminLogin = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", adminLogin);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseJson = await loginResponse.Content.ReadAsStringAsync();
        var loginResponseObj = JsonSerializer.Deserialize<ResponseBase>(loginResponseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var result = loginResponseObj?.Result is JsonElement resultJson
            ? JsonSerializer.Deserialize<LoginResponseDto>(resultJson.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            : null;

        Assert.That(result?.Token, Is.Not.Null, "Auth token should not be null.");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "newadmin@example.com",
            Username = "NewAdmin",
            Password = "AdminPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register/admin", registrationRequestDto);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<ResponseBase>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(responseObj?.Success, Is.True, "Admin registration should be successful.");
    }
    
    [Test, Order(6)]
    public async Task RegisterAdmin_UserNotSuperAdmin_ReturnsForbidden()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "teest@example.com",
            Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseJson = await loginResponse.Content.ReadAsStringAsync();
        var loginResponseObj = JsonSerializer.Deserialize<ResponseBase>(loginResponseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var result = loginResponseObj?.Result is JsonElement resultJson
            ? JsonSerializer.Deserialize<LoginResponseDto>(resultJson.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            : null;

        Assert.That(result?.Token, Is.Not.Null, "Auth token should not be null.");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);


        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "newadmin@example.com",
            Username = "NewAdmin",
            Password = "AdminPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register/admin", registrationRequestDto);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), "Expected 403 Forbidden for unauthorized role.");
    }

    [Test, Order(7)]
    public async Task ChangePassword_InvalidRequest_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseJson = await loginResponse.Content.ReadAsStringAsync();
        var loginResponseObj = JsonSerializer.Deserialize<ResponseBase>(loginResponseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var result = loginResponseObj?.Result is JsonElement resultJson
            ? JsonSerializer.Deserialize<LoginResponseDto>(resultJson.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            : null;

        Assert.That(result?.Token, Is.Not.Null, "Auth token should not be null.");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

        var changePasswordRequestDto = new ChangePasswordRequestDto
        {
            OldPassword = "Password123!",
            NewPassword = "NewPassword123!",
            NewPasswordConfirm = "DifferentPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/change-password", changePasswordRequestDto);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(responseObj?.Success, Is.False);
    }
    
    [Test, Order(8)]
    public async Task ChangePassword_ValidRequest_PasswordChangedSuccessfully_ReturnsOk()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseJson = await loginResponse.Content.ReadAsStringAsync();
        var loginResponseObj = JsonSerializer.Deserialize<ResponseBase>(loginResponseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var result = loginResponseObj?.Result is JsonElement resultJson
            ? JsonSerializer.Deserialize<LoginResponseDto>(resultJson.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            : null;

        Assert.That(result?.Token, Is.Not.Null, "Auth token should not be null.");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

        var changePasswordRequestDto = new ChangePasswordRequestDto
        {
            OldPassword = "Password123!",
            NewPassword = "NewPassword123!",
            NewPasswordConfirm = "NewPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/change-password", changePasswordRequestDto);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<ResponseBase>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(responseObj?.Success, Is.True, "Password change should be successful.");
    }

    private async Task SetupDbData()
    {
        using var scope = _factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        if (!await roleManager.RoleExistsAsync("SUPERADMIN"))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "SUPERADMIN" });
        }
        
        if (!await roleManager.RoleExistsAsync("ADMIN"))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "ADMIN" });
        }

        var user = new User
        {
            Email = "test@example.com",
            UserName = "test@example.com",
            PasswordHash = "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
            SecurityStamp = "Q7++M6z5N+Tly9yfor8HhJxhg52bNmZ"
        };
        
        var user2 = new User
        {
            Email = "teest@example.com",
            UserName = "teest@example.com",
            PasswordHash = "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
            SecurityStamp = "Q7++M6z5N+Tly9yfor8HhJxhg52bNmZ"
        };

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        await userManager.AddToRolesAsync(user, new[] { "SUPERADMIN" });
        
        await db.Users.AddAsync(user2);
        await db.SaveChangesAsync();
        await userManager.AddToRolesAsync(user2, new[] { "ADMIN" });
    }

    [Test]
    public async Task GetUserProfileByUsername_ValidUsername_ReturnsOk()
    {
        var username = "test@example.com";

        var response = await _client.GetAsync($"/api/auth/profile/{username}");
        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var responseUserString = responseObj?.Result?.ToString() ?? string.Empty;

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
        var username = "TEST@EXAMPLE.COM";

        var response = await _client.GetAsync($"/api/auth/profile/{username}");
        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var responseUserString = responseObj?.Result?.ToString() ?? string.Empty;

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
        var username = "nonexistentuser";

        var response = await _client.GetAsync($"/api/auth/profile/{username}");
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();

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
using System.IdentityModel.Tokens.Jwt;
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
using JsonElement = System.Text.Json.JsonElement;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FakeHubApi.Tests.Users.Tests;

public class UserControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;
    private string _regularUserToken;
    private string _adminToken;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        await SetupDbData();
        InitializeTokens();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }
    
    [Test, Order(1)]
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

        var response = await _client.PostAsJsonAsync("/api/users/change-password", changePasswordRequestDto);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.That(responseObj?.Success, Is.False);
    }
    
    [Test, Order(2)]
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

        var response = await _client.PostAsJsonAsync("/api/users/change-password", changePasswordRequestDto);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<ResponseBase>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(responseObj?.Success, Is.True, "Password change should be successful.");
    }

    [Test, Order(3)]
    public async Task GetUserProfileByUsername_ValidUsername_ReturnsOk()
    {
        var username = "test@example.com";

        var response = await _client.GetAsync($"/api/users/{username}");
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

        var responseUserObject = JsonConvert.DeserializeObject<UserDto>(responseUserString);
        Assert.Multiple(() =>
        {
            Assert.That(responseUserObject, Is.Not.Null);
            Assert.That(responseUserObject?.Username, Is.Not.Null);
            Assert.That(responseUserObject?.Username, Is.EqualTo(username));
        });
    }

    [Test, Order(4)]
    public async Task GetUserProfileByUsername_ValidUsernameWithDifferentCase_ReturnsOk()
    {
        var username = "TEST@EXAMPLE.COM";

        var response = await _client.GetAsync($"/api/users/{username}");
        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var responseUserString = responseObj?.Result?.ToString() ?? string.Empty;

        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseUserString, Is.Not.Empty);
        });

        var responseUserObject = JsonConvert.DeserializeObject<UserDto>(responseUserString);
        Assert.Multiple(() =>
        {
            Assert.That(responseUserObject, Is.Not.Null);
            Assert.That(responseUserObject?.Username, Is.Not.Null);
            Assert.That(responseUserObject?.Username, Is.EqualTo(username.ToLower()));
        });
    }

    [Test, Order(5)]
    public async Task GetUserProfileByUsername_NonExistentUsername_ReturnsNotFound()
    {
        var username = "nonexistentuser";

        var response = await _client.GetAsync($"/api/users/{username}");
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

    [Test, Order(6)]
    public async Task ChangeEmail_NoBearerToken_ReturnsUnauthorized()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "Password123!",
            NewEmail = "test3_new_email@example.com"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", null);
        var response = await _client.PostAsJsonAsync("/api/users/change-email", changeEmailRequestDto);

        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test, Order(7)]
    public async Task ChangeEmail_InvalidPassword_ReturnsBadRequest()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "InvalidPassword123!",
            NewEmail = "test3_new_email@example.com"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
        var response = await _client.PostAsJsonAsync("/api/users/change-email", changeEmailRequestDto);
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseObj?.ErrorMessage, Is.Not.Empty);
            Assert.That(responseObj?.ErrorMessage, Is.EqualTo("Password is incorrect"));
            Assert.That(responseObj?.Result, Is.Null);
        });
    }

    [Test, Order(8)]
    public async Task ChangeEmail_SameEmail_ReturnsBadRequest()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "Password123!",
            NewEmail = "test3@example.com"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
        var response = await _client.PostAsJsonAsync("/api/users/change-email", changeEmailRequestDto);
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseObj?.ErrorMessage, Is.Not.Empty);
            Assert.That(responseObj?.ErrorMessage, Is.EqualTo("Email can't be the same as current"));
            Assert.That(responseObj?.Result, Is.Null);
        });
    }

    [Test, Order(9)]
    public async Task ChangeEmail_SuccessfulChange_ReturnsOk()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "Password123!",
            NewEmail = "test3_new_email@example.com"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
        var response = await _client.PostAsJsonAsync("/api/users/change-email", changeEmailRequestDto);
        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var loginResponseDtoString = responseObj?.Result?.ToString() ?? string.Empty;

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(responseObj?.Result, Is.Not.Null);
            Assert.That(loginResponseDtoString, Is.Not.Empty);
        });

        var loginResponseDtoObject = JsonConvert.DeserializeObject<LoginResponseDto>(loginResponseDtoString);

        Assert.That(loginResponseDtoObject, Is.Not.Null);
        Assert.That(loginResponseDtoObject?.Token, Is.Not.Null);

        var emailFromResponseToken = ExtractEmailFromJwt(loginResponseDtoObject.Token);

        Assert.That(emailFromResponseToken, Is.EqualTo(changeEmailRequestDto.NewEmail));
    }
    
    [Test, Order(16)]
    public async Task ChangeUserBadge_Unauthorized_ReturnsForbidden()
    {
        var changeUserBadgeRequestDto = new ChangeUserBadgeRequestDto
        {
            Badge = Badge.VerifiedPubisher,
            Username = "test3@example.com"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
        var response = await _client.PostAsJsonAsync("/api/users/change-user-badge", changeUserBadgeRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        });
    }

    [Test, Order(17)]
    public async Task ChangeUserBadge_NonExistentUser_ReturnsBadRequest()
    {
        var changeUserBadgeRequestDto = new ChangeUserBadgeRequestDto
        {
            Badge = Badge.VerifiedPubisher,
            Username = "test333@example.com"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        var response = await _client.PostAsJsonAsync("/api/users/change-user-badge", changeUserBadgeRequestDto);
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseObj?.ErrorMessage, Is.Not.Empty);
            Assert.That(responseObj?.ErrorMessage, Is.EqualTo("User not found"));
            Assert.That(responseObj?.Result, Is.Null);
        });
    }

    [Test, Order(18)]
    public async Task ChangeUserBadge_SuccessfulChange_ReturnsOk()
    {
        var changeUserBadgeRequestDto = new ChangeUserBadgeRequestDto
        {
            Badge = Badge.VerifiedPubisher,
            Username = "test3@example.com"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        var response = await _client.PostAsJsonAsync("/api/users/change-user-badge", changeUserBadgeRequestDto);
        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var changeUserBadgeResponseDtoString = responseObj?.Result?.ToString() ?? string.Empty;

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(responseObj?.Result, Is.Not.Null);
            Assert.That(changeUserBadgeResponseDtoString, Is.Not.Empty);
        });

        var changeUserBadgeResponseDtoObject = JsonConvert.DeserializeObject<UserDto>(changeUserBadgeResponseDtoString);

        Assert.That(changeUserBadgeResponseDtoObject, Is.Not.Null);
        Assert.That(changeUserBadgeResponseDtoObject?.Badge, Is.EqualTo(changeUserBadgeRequestDto.Badge));
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

        if (!await roleManager.RoleExistsAsync("USER"))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "USER" });
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

        var user3 = new User
        {
            Email = "test3@example.com",
            UserName = "test3@example.com",
            PasswordHash = "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
            SecurityStamp = "Q7++M6z5N+Tly9yfor8HhJxhg52bNmZ"
        };

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        await userManager.AddToRolesAsync(user, new[] { "SUPERADMIN" });
        
        await db.Users.AddAsync(user2);
        await db.SaveChangesAsync();
        await userManager.AddToRolesAsync(user2, new[] { "ADMIN" });

        await db.Users.AddAsync(user3);
        await db.SaveChangesAsync();
        await userManager.AddToRolesAsync(user3, new[] { "USER" });
    }

    private async Task<string> GetTokenFromSuccessfulUserLogin()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test3@example.com",
            Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();


        var responseObj = await loginResponse.Content.ReadFromJsonAsync<ResponseBase>();
        var loginResponseDtoString = responseObj?.Result?.ToString() ?? string.Empty;

        var loginResponseDtoObject = JsonConvert.DeserializeObject<LoginResponseDto>(loginResponseDtoString);

        return loginResponseDtoObject?.Token ?? "";
    }

    private string ExtractEmailFromJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        if (string.IsNullOrEmpty(token) || !handler.CanReadToken(token))
        {
            throw new ArgumentException("Invalid JWT token.", nameof(token));
        }

        var jwtToken = handler.ReadJwtToken(token);
        var emailClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        return emailClaim;
    }
    
    private async void InitializeTokens()
    {
        var regularUser = new LoginRequestDto
        {
            Email = "test3@example.com",
            Password = "Password123!"
        };
        _regularUserToken = await GetTokenFromSuccessfulUserLogin(regularUser);

        var adminUser = new LoginRequestDto
        {
            Email = "teest@example.com",
            Password = "Password123!"
        };
        _adminToken = await GetTokenFromSuccessfulUserLogin(adminUser);
    }
    
    private async Task<string> GetTokenFromSuccessfulUserLogin(LoginRequestDto loginRequestDto)
    {

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequestDto);
        loginResponse.EnsureSuccessStatusCode();


        var responseObj = await loginResponse.Content.ReadFromJsonAsync<ResponseBase>();
        var loginResponseDtoString = responseObj?.Result?.ToString() ?? string.Empty;

        var loginResponseDtoObject = JsonConvert.DeserializeObject<LoginResponseDto>(loginResponseDtoString);

        return loginResponseDtoObject?.Token ?? "";
    }
}
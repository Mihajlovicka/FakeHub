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

namespace FakeHubApi.Tests.Organization.Tests;

public class OrganizationControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        await AddUser();
        var token = await GetTokenFromSuccessfulUserLogin();
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
    public async Task AddOrganization()
    {
        var organizationDto = new OrganizationDto
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 =
                "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/wQAAwAC/1EpeEkAAAAASUVORK5CYII=",
        };

        var response = await _client.PostAsJsonAsync("/api/organization", organizationDto);

        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(responseObj?.Result, Is.Null);
        });
    }

    [Test, Order(2)]
    public async Task AddOrganization_Fails_NameNotUnique()
    {
        var organizationDto = new OrganizationDto
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 =
                "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/wQAAwAC/1EpeEkAAAAASUVORK5CYII=",
        };

        var response = await _client.PostAsJsonAsync("/api/organization", organizationDto);

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(responseObj?.ErrorMessage, Is.EqualTo("Organization name is not unique."));
        });
    }

    [Test, Order(3)]
    public async Task AddOrganization_Fails_ImageFormatValidationFailed()
    {
        var organizationDto = new OrganizationDto
        {
            Name = "Test Organization 1",
            Description = "Test Description",
            ImageBase64 = "someraandomstring",
        };

        var response = await _client.PostAsJsonAsync("/api/organization", organizationDto);

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(
                responseObj?.ErrorMessage,
                Is.EqualTo("The image must be a valid Base64 encoded image.")
            );
        });
    }

    private async Task AddUser()
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

    private async Task<string> GetTokenFromSuccessfulUserLogin()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "Password123!",
        };

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

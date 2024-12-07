using System.Net.Http.Headers;
using System.Net.Http.Json;
using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace FakeHubApi.Tests.Organization.Tests;

public class OrganizationControllerIntegrationTests
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
        await AddUser();
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
    public async Task AddOrganization()
    {
        var organizationDto = new OrganizationDto
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = imageBase64,
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
    public async Task SearchOrganization()
    {
        var query = "Organization";

        var response = await _client.GetFromJsonAsync<ResponseBase>(
            $"/api/organization?query={query}"
        );

        Assert.Multiple(() =>
        {
            Assert.That(response?.Success, Is.True);
            var jsonString = response?.Result?.ToString() ?? string.Empty;
            var responseOrganization = JsonConvert.DeserializeObject<List<OrganizationDto>>(
                jsonString
            );

            Assert.That(responseOrganization, Is.Not.Null);
            Assert.That(responseOrganization?.Count, Is.EqualTo(1));
        });

        query = "Not Found";

        response = await _client.GetFromJsonAsync<ResponseBase>($"/api/organization?query={query}");

        Assert.Multiple(() =>
        {
            Assert.That(response?.Success, Is.True);
            var jsonString = response?.Result?.ToString() ?? string.Empty;
            var responseOrganization = JsonConvert.DeserializeObject<List<OrganizationDto>>(
                jsonString
            );

            Assert.That(responseOrganization, Is.Not.Null);
            Assert.That(responseOrganization?.Count, Is.EqualTo(0));
        });
    }

    [Test, Order(2)]
    public async Task AddOrganization_Fails_NameNotUnique()
    {
        var organizationDto = new OrganizationDto
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = imageBase64,
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

    [Test, Order(4)]
    public async Task EditOrganization()
    {
        var name = "Test Organization";
        var organizationDto = new UpdateOrganizationDto
        {
            Description = "Test Description",
            ImageBase64 = imageBase64,
        };

        var response = await _client.PutAsJsonAsync($"/api/organization/{name}", organizationDto);

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        response.EnsureSuccessStatusCode();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.True);
            Assert.That(responseObj?.Result, Is.Null);
        });
    }

    [Test, Order(5)]
    public async Task EditOrganization_UserWihoutPermition()
    {
        var name = "Test Organization";
        var organizationDto = new UpdateOrganizationDto
        {
            Description = "Test Description",
            ImageBase64 = imageBase64,
        };

        var token = await GetTokenFromSuccessfulUserLogin(
            new LoginRequestDto { Email = "testtest@example.com", Password = "Password123!" }
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        var response = await _client.PutAsJsonAsync($"/api/organization/{name}", organizationDto);

        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.Multiple(() =>
        {
            Assert.That(responseObj?.Success, Is.False);
            Assert.That(
                responseObj?.ErrorMessage,
                Is.EqualTo("You are not authorized to update this organization.")
            );
        });
    }

    [Test, Order(6)]
    public async Task GetByName()
    {
        var name = "Test Organization";

        var response = await _client.GetFromJsonAsync<ResponseBase>($"/api/organization/{name}");

        Assert.Multiple(() =>
        {
            Assert.That(response?.Success, Is.True);
            var jsonString = response?.Result?.ToString() ?? string.Empty;
            var responseOrganization = JsonConvert.DeserializeObject<OrganizationDto>(jsonString);

            Assert.That(responseOrganization, Is.Not.Null);
            Assert.That(responseOrganization?.Name, Is.EqualTo(name));
            Assert.That(responseOrganization?.Description, Is.EqualTo("Test Description"));
            Assert.That(responseOrganization?.ImageBase64, Is.EqualTo(imageBase64));
        });
    }

    [Test, Order(7)]
    public async Task GetByName_NoOrganization()
    {
        var name = "Test Organization 1";

        var response = await _client.GetAsync($"/api/organization/{name}");

        var responseBody = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseBody?.Success, Is.False);
            Assert.That(responseBody?.ErrorMessage, Is.EqualTo("Organization not found."));
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

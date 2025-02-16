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
            TeamRole = TeamRole.ReadOnly.ToString(),
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
            TeamRole = TeamRole.ReadOnly.ToString(),
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
            TeamRole = TeamRole.ReadOnly.ToString(),
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

    [Test, Order(4)]
    public async Task GetTeam()
    {
        var token = await GetTokenFromSuccessfulUserLogin(
            new LoginRequestDto { Email = "test@example.com", Password = "Password123!" }
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
        var teamName = "Test Team";
        var organizationName = "Test Team Organization";

        var response = await _client.GetFromJsonAsync<ResponseBase>(
            $"/api/organization/team/{organizationName}/{teamName}"
        );

        Assert.Multiple(() =>
        {
            Assert.That(response?.Success, Is.True);
            var jsonString = response?.Result?.ToString() ?? string.Empty;
            var responseOrganization = JsonConvert.DeserializeObject<TeamDto>(jsonString);

            Assert.That(responseOrganization, Is.Not.Null);
            Assert.That(responseOrganization?.Name, Is.EqualTo(teamName));
        });
    }

    [Test, Order(5)]
    public async Task GetTeam_NoTeam()
    {
        var teamName = "Test Team 5";
        var organizationName = "Test Team Organization";

        var response = await _client.GetAsync(
            $"/api/organization/team/{organizationName}/{teamName}"
        );

        var responseBody = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseBody?.Success, Is.False);
            Assert.That(responseBody?.ErrorMessage, Is.EqualTo("Team not found."));
        });
    }

    [Test, Order(6)]
    public async Task EditTeam()
    {
        var teamName = "Test Team";
        var organizationName = "Test Team Organization";
        var teamDto = new UpdateTeamDto()
        {
            Name = "Test Team 2",
            Description = "Test Description 2",
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/organization/team/{organizationName}/{teamName}",
            teamDto
        );

        var responseBody = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseBody?.Success, Is.True);
            Assert.That(responseBody?.Result, Is.Null);
        });
    }

    [Test, Order(7)]
    public async Task EditTeam_NotExists()
    {
        var teamName = "Test Team Not Exists";
        var organizationName = "Test Team Organization";
        var teamDto = new UpdateTeamDto()
        {
            Name = "Test Team 2",
            Description = "Test Description 2",
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/organization/team/{organizationName}/{teamName}",
            teamDto
        );

        var responseBody = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseBody?.Success, Is.False);
            Assert.That(responseBody?.ErrorMessage, Is.EqualTo("Team not found."));
        });
    }

    [Test, Order(8)]
    public async Task EditTeam_NameNotUnique()
    {
        var teamDto = new TeamDto
        {
            Name = "Test Team 3",
            Description = "Test Description",
            OrganizationName = "Test Team Organization",
            TeamRole = TeamRole.ReadOnly.ToString(),
        };

        await _client.PostAsJsonAsync("/api/organization/team", teamDto);

        var teamName = "Test Team 2";
        var organizationName = "Test Team Organization";
        var updateTeamDto = new UpdateTeamDto()
        {
            Name = "Test Team 3",
            Description = "Test Description 2",
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/organization/team/{organizationName}/{teamName}",
            updateTeamDto
        );

        var responseBody = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseBody?.Success, Is.False);
            Assert.That(responseBody?.ErrorMessage, Is.EqualTo("Team name is not unique."));
        });
    }

    [Test, Order(9)]
    public async Task AddMember_MemerNotInOrganization()
    {
        var teamName = "Test Team 2";
        var organizationName = "Test Team Organization";
        var memberDto = new AddMembersDto
        {
            Usernames = new List<string> { "testest@example.com" },
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/organization/team/{organizationName}/{teamName}/add-user",
            memberDto
        );
        var responseBody = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var responseObjString = responseBody?.Result?.ToString() ?? string.Empty;
        var responseObjStringObject = JsonConvert.DeserializeObject<UserDto[]>(responseObjString);

        Assert.Multiple(() =>
        {
            Assert.That(responseBody?.Success, Is.True);
            Assert.That(responseObjStringObject.Length, Is.EqualTo(0));
        });
    }

    [Test, Order(10)]
    public async Task AddMember()
    {
        var teamName = "Test Team 2";
        var organizationName = "Test Team Organization";
        var memberDto = new AddMembersDto
        {
            Usernames = new List<string> { "testest@example.com" },
        };
        await _client.PostAsJsonAsync($"/api/organization/{organizationName}/add-user", memberDto);

        var response = await _client.PutAsJsonAsync(
            $"/api/organization/team/{organizationName}/{teamName}/add-user",
            memberDto
        );
        var responseBody = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var responseObjString = responseBody?.Result?.ToString() ?? string.Empty;
        var responseObjStringObject = JsonConvert.DeserializeObject<UserDto[]>(responseObjString);

        Assert.Multiple(() =>
        {
            Assert.That(responseBody?.Success, Is.True);
            Assert.That(responseObjStringObject.Length, Is.EqualTo(1));
        });
    }

    [Test, Order(11)]
    public async Task DeleteUser_NotTeamMember_ReturnsBadRequest()
    {
        const string teamName = "Test Team 2";
        const string organizationName = "Test Team Organization";
        const string username = "test@example.com";

        var response = await _client.DeleteAsync($"/api/organization/team/{organizationName}/{teamName}/delete-user/{username}");
        var responseBase = await response.Content.ReadFromJsonAsync<ResponseBase>();

        Assert.Multiple(() =>
        {
            Assert.That(responseBase?.Success, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseBase?.ErrorMessage, Is.EqualTo("User is not member of team"));
            Assert.That(responseBase?.Result, Is.Null);
        });
    }

    [Test, Order(12)]
    public async Task DeleteUser_ValidRequest_ReturnsOk()
    {
        const string teamName = "Test Team 2";
        const string organizationName = "Test Team Organization";
        const string username = "testest@example.com";

        var response = await _client.DeleteAsync($"/api/organization/team/{organizationName}/{teamName}/delete-user/{username}");
        response.EnsureSuccessStatusCode();
        var responseBase = await response.Content.ReadFromJsonAsync<ResponseBase>();
        var responseObjectString = responseBase?.Result?.ToString() ?? string.Empty;

        Assert.Multiple(() =>
        {
            Assert.That(responseBase?.Success, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBase?.ErrorMessage, Is.Empty);
            Assert.That(responseObjectString, Is.Not.Empty);
        });

        var responseObject = JsonConvert.DeserializeObject<UserDto>(responseObjectString);

        Assert.That(responseObject.Username, Is.EqualTo(username));
    }
    
    [Test, Order(13)]
    public async Task DeleteTeamFromOrganization_ValidRequest_ReturnsOk()
    {
        const string organizationName = "Test Team Organization";
        const string teamName = "Test Team 2";
        
        var response = await _client.DeleteAsync($"/api/organization/team/{organizationName}/{teamName}");
        
        response.EnsureSuccessStatusCode();
        var responseBase = await response.Content.ReadFromJsonAsync<ResponseBase>();
        
        Assert.Multiple(() =>
        {
            Assert.That(responseBase?.Success, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBase?.ErrorMessage, Is.Empty);
        });
    }

    [Test, Order(14)]
    public async Task DeleteTeamFromOrganization_TeamDoesNotExist_ReturnsBadRequest()
    {
        const string organizationName = "Test Team Organization";
        const string teamName = "Nonexistent Team";
    
        var response = await _client.DeleteAsync($"/api/organization/team/{organizationName}/{teamName}");
    
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBase = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseBase?.Success, Is.False);
            Assert.That(responseBase?.ErrorMessage, Is.EqualTo("Team not found in organization."));
        });
    }

    [Test, Order(15)]
    public async Task DeleteTeamFromOrganization_NotAuthorized_ReturnsBadRequest()
    {
        const string organizationName = "Test Team Organization";
        const string teamName = "Test Team 2";
    
        var token = await GetTokenFromSuccessfulUserLogin(
            new LoginRequestDto { Email = "testtest@example.com", Password = "Password123!" }
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
    
        var response = await _client.DeleteAsync($"/api/organization/team/{organizationName}/{teamName}");
    
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBase = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseBase?.Success, Is.False);
            Assert.That(responseBase?.ErrorMessage, Is.EqualTo("You are not the owner of this organization."));
        });   
    }

    [Test, Order(16)]
    public async Task DeleteTeamFromOrganization_OrganizationNotFound_ReturnsBadRequest()
    {
        const string organizationName = "Nonexistent Organization";
        const string teamName = "Test Team 2";
    
        var response = await _client.DeleteAsync($"/api/organization/team/{organizationName}/{teamName}");
    
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBase = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseBase?.Success, Is.False);
            Assert.That(responseBase?.ErrorMessage, Is.EqualTo("Organization not found."));
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

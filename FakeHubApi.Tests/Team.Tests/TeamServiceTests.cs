using FakeHubApi.ContainerRegistry;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Moq;

namespace FakeHubApi.Tests.Team.Tests;

public class TeamServiceTests
{
    private Mock<IMapperManager> _mapperManagerMock;
    private ITeamService _teamService;
    private Mock<IRepositoryManager> _repositoryManagerMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<IUserContextService> _userContextServiceMock;
    private Mock<IHarborService> _harborServiceMock;
    private Mock<IOrganizationService> _organizationServiceMock;

    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _userServiceMock = new Mock<IUserService>();
        _organizationServiceMock = new Mock<IOrganizationService>();
        _harborServiceMock = new Mock<IHarborService>();

        _teamService = new TeamService(
            _organizationServiceMock.Object,
            _mapperManagerMock.Object,
            _repositoryManagerMock.Object,
            _userServiceMock.Object,
            _harborServiceMock.Object
        );
    }

    [Test]
    public async Task AddTeam()
    {
        var teamDto = new TeamDto
        {
            Name = "Test Team",
            Description = "Test Description",
            OrganizationName = "Test Organization",
            TeamRole = TeamRole.ReadOnly.ToString(),
            Repository = new RepositoryDto { Id = 1, Name = "Test Repository" }
        };
        var user = new User { Id = 1, UserName = "Test User" };
        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = 1,
        };

        var team = new Model.Entity.Team
        {
            Name = "Test Team",
            Description = "Test Description",
            Organization = organization,
            TeamRole = TeamRole.ReadOnly,
        };

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(true));

        _mapperManagerMock.Setup(m => m.TeamDtoToTeamMapper.Map(It.IsAny<TeamDto>())).Returns(team);

        _organizationServiceMock
            .Setup(m => m.GetOrganization(It.IsAny<string>()))
            .ReturnsAsync(organization);

        _repositoryManagerMock
            .Setup(m => m.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Model.Entity.Repository
            {
                Id = 1,
                Name = "Test Repository"
            });

        _repositoryManagerMock
            .Setup(m => m.TeamRepository.AddAsync(It.IsAny<Model.Entity.Team>()))
            .Returns(Task.FromResult(team));

        var result = await _teamService.Add(teamDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Null);
        });
    }

    [Test]
    public async Task AddTeam_Fails_NameNotUnique()
    {
        var repository = new Model.Entity.Repository
        {
            Id = 1,
            Name = "Test Repository"
        };
        var teamDto = new TeamDto
        {
            Name = "Test Team",
            Description = "Test Description",
            OrganizationName = "Test Organization",
            TeamRole = TeamRole.ReadOnly.ToString(),
            Repository = new RepositoryDto { Id = 1, Name = "Test Repository" }
        };
        var user = new User { Id = 1, UserName = "Test User" };
        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = 1,
        };

        var team = new Model.Entity.Team
        {
            Name = "Test Team",
            Description = "Test Description",
            Organization = organization,
            TeamRole = TeamRole.ReadOnly,
        };

        organization.Teams.Add(team);

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(true));

        _organizationServiceMock
            .Setup(m => m.GetOrganization(It.IsAny<string>()))
            .ReturnsAsync(organization);

        _repositoryManagerMock
            .Setup(m => m.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(repository);

        _repositoryManagerMock
            .Setup(m => m.TeamRepository.AddAsync(It.IsAny<Model.Entity.Team>()))
            .Returns(Task.FromResult(team));
        _mapperManagerMock.Setup(m => m.TeamDtoToTeamMapper.Map(It.IsAny<TeamDto>())).Returns(team);

        var result = await _teamService.Add(teamDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Team name is not unique."));
        });
    }

    [Test]
    public async Task AddTeam_Fails_NotAuthorized()
    {
        var repository = new Model.Entity.Repository
        {
            Id = 1,
            Name = "Test Repository"
        };
        var teamDto = new TeamDto
        {
            Name = "Test Team",
            Description = "Test Description",
            OrganizationName = "Test Organization",
            TeamRole = TeamRole.ReadOnly.ToString(),
            Repository = new RepositoryDto { Id = 1, Name = "Test Repository" }
        };
        var user = new User { Id = 1, UserName = "Test User" };
        var user2 = new User { Id = 2, UserName = "Test User" };
        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = 1,
        };

        var team = new Model.Entity.Team
        {
            Name = "Test Team",
            Description = "Test Description",
            Organization = organization,
            TeamRole = TeamRole.ReadOnly,
        };

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(false));

        _mapperManagerMock.Setup(m => m.TeamDtoToTeamMapper.Map(It.IsAny<TeamDto>())).Returns(team);

        _repositoryManagerMock
            .Setup(m => m.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(repository);
            
        _organizationServiceMock
            .Setup(m => m.GetOrganization(It.IsAny<string>()))
            .ReturnsAsync(organization);

        var result = await _teamService.Add(teamDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.ErrorMessage,
                Is.EqualTo("You are not the owner of this organization.")
            );
        });
    }

    [Test]
    public async Task EditTeam()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = 1,
        };

        var team = new Model.Entity.Team
        {
            Name = "Test Team",
            Description = "Test Description",
            Organization = organization,
            TeamRole = TeamRole.ReadOnly,
        };

        organization.Teams.Add(team);

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(true));

        _organizationServiceMock
            .Setup(m => m.GetOrganization(It.IsAny<string>()))
            .ReturnsAsync(organization);

        _repositoryManagerMock
            .Setup(m => m.TeamRepository.UpdateAsync(It.IsAny<Model.Entity.Team>()))
            .Returns(Task.FromResult(team));

        var teamName = "Test Team";
        var organizationName = "Test Organization";
        var updateTeamDto = new UpdateTeamDto
        {
            Name = "Test Team",
            Description = "Test Description",
        };

        var result = await _teamService.Update(organizationName, teamName, updateTeamDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Null);
        });
    }

    [Test]
    public async Task EditTeam_NameNotUnique()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = 1,
        };

        var team = new Model.Entity.Team
        {
            Name = "Test Team",
            Description = "Test Description",
            Organization = organization,
            TeamRole = TeamRole.ReadOnly,
        };
        var team2 = new Model.Entity.Team
        {
            Name = "Test Team 2",
            Description = "Test Description",
            Organization = organization,
            TeamRole = TeamRole.ReadOnly,
        };

        organization.Teams.Add(team);
        organization.Teams.Add(team2);

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(true));

        _organizationServiceMock
            .Setup(m => m.GetOrganization(It.IsAny<string>()))
            .ReturnsAsync(organization);

        _repositoryManagerMock
            .Setup(m => m.TeamRepository.UpdateAsync(It.IsAny<Model.Entity.Team>()))
            .Returns(Task.FromResult(team));

        var teamName = "Test Team";
        var organizationName = "Test Organization";
        var updateTeamDto = new UpdateTeamDto
        {
            Name = "Test Team 2",
            Description = "Test Description",
        };

        var result = await _teamService.Update(organizationName, teamName, updateTeamDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Team name is not unique."));
        });
    }

    [Test]
    public async Task AddMember()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        var member = new User { Id = 2, UserName = "Test Member" };
        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            Owner = user,
            Users = new List<User> { member },
        };

        var team = new Model.Entity.Team
        {
            Name = "Test Team",
            Description = "Test Description",
            Organization = organization,
            TeamRole = TeamRole.ReadOnly,
        };

        _userServiceMock
            .Setup(m => m.GetUsers(It.IsAny<List<string>>()))
            .Returns(new List<User> { member });

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(true));

        _repositoryManagerMock
            .Setup(m => m.TeamRepository.GetTeam(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(team));

        _repositoryManagerMock
            .Setup(m => m.TeamRepository.UpdateAsync(It.IsAny<Model.Entity.Team>()))
            .Returns(Task.FromResult(team));

        _mapperManagerMock
            .Setup(m => m.UserToUserDtoMapper.Map(It.IsAny<User>()))
            .Returns(new UserDto { Username = "Test Member" });

        var teamName = "Test Team";
        var organizationName = "Test Organization";
        var usernames = new List<string> { "Test Member" };

        var result = await _teamService.AddUser(organizationName, teamName, usernames);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            var resultList = ((IEnumerable<UserDto>)result.Result).ToList();
            Assert.That(resultList, Is.Not.Null);
            Assert.That(resultList, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task AddMember_MemberNotInOrganization()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        var member = new User { Id = 2, UserName = "Test Member" };
        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = 1,
        };

        var team = new Model.Entity.Team
        {
            Name = "Test Team",
            Description = "Test Description",
            Organization = organization,
            TeamRole = TeamRole.ReadOnly,
        };

        organization.Teams.Add(team);

        _userServiceMock
            .Setup(m => m.GetUsers(It.IsAny<List<string>>()))
            .Returns(new List<User> { member });

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(true));

        _repositoryManagerMock
            .Setup(m => m.TeamRepository.GetTeam(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(team));

        _repositoryManagerMock
            .Setup(m => m.TeamRepository.UpdateAsync(It.IsAny<Model.Entity.Team>()))
            .Returns(Task.FromResult(team));

        _mapperManagerMock
            .Setup(m => m.UserToUserDtoMapper.Map(It.IsAny<User>()))
            .Returns(new UserDto { Username = "Test Member" });

        var teamName = "Test Team";
        var organizationName = "Test Organization";
        var usernames = new List<string> { "Test Member" };

        var result = await _teamService.AddUser(organizationName, teamName, usernames);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            var resultList = ((IEnumerable<UserDto>)result.Result).ToList();
            Assert.That(resultList, Is.Not.Null);
            Assert.That(resultList, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public async Task DeleteUser_UserNotInOrganization_ReturnsErrorResponse()
    {
        const string organizationName = "organization";
        const string teamName = "team";
        const string username = "user";

        var user = new UserDto { Username = username };
        var userResponseBase = new ResponseBase { Result = user, Success = true };

        var team = new Model.Entity.Team { Name = teamName, Users = []};

        _userServiceMock
            .Setup(us => us.GetUserProfileByUsernameAsync(username))
            .ReturnsAsync(userResponseBase);

        _repositoryManagerMock
            .Setup(rm => rm.TeamRepository.GetTeam(organizationName, teamName))
            .ReturnsAsync(team);

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(true));

        var result = await _teamService.DeleteUser(organizationName, teamName, username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("User is not member of team"));
        });
    }

    [Test]
    public async Task DeleteUser_ValidRequest_ReturnsSuccessResponse()
    {
        const string organizationName = "organization";
        const string teamName = "team";
        const string username = "user";

        var userDto = new UserDto { Username = username };
        var user = new User { UserName = username };
        var userResponseBase = new ResponseBase { Result = userDto, Success = true };

        var team = new Model.Entity.Team { Name = teamName, Users = [user] };

        _userServiceMock
            .Setup(us => us.GetUserProfileByUsernameAsync(username))
            .ReturnsAsync(userResponseBase);

        _repositoryManagerMock
            .Setup(rm => rm.TeamRepository.GetTeam(organizationName, teamName))
            .ReturnsAsync(team);

        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(true));

        var result = await _teamService.DeleteUser(organizationName, teamName, username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.ErrorMessage, Is.Empty);
            Assert.That((UserDto)result.Result, Is.EqualTo(userDto));
        });
    }
    
    [Test]
public async Task DeleteTeamFromOrganization_TeamExists_ReturnsSuccessResponse()
{
    const string organizationName = "Test Organization";
    const string teamName = "Test Team";
    var organization = new Model.Entity.Organization
    {
        Name = organizationName,
        Teams = new List<Model.Entity.Team>
        {
            new Model.Entity.Team { Name = teamName }
        }
    };
    _organizationServiceMock
        .Setup(m => m.GetOrganization(organizationName))
        .ReturnsAsync(organization);
    _repositoryManagerMock
        .Setup(m => m.TeamRepository.UpdateAsync(It.IsAny<Model.Entity.Team>()))
        .Returns(Task.FromResult(new Model.Entity.Team { Name = teamName }));
    _organizationServiceMock
        .Setup(m => m.IsLoggedInUserOwner(organization))
        .Returns(Task.FromResult(true));

    var result = await _teamService.DeleteTeamFromOrganization(organizationName, teamName);

    Assert.Multiple(() =>
    {
        Assert.That(result.Success, Is.True);
        Assert.That(organization.Teams.Any(t => t.Name == teamName), Is.False);
    });
}

    [Test]
    public async Task DeleteTeamFromOrganization_TeamDoesNotExist_ReturnsErrorResponse()
    {
        const string organizationName = "Test Organization";
        const string teamName = "Nonexistent Team";
        var organization = new Model.Entity.Organization { Name = organizationName };
        _organizationServiceMock
            .Setup(m => m.GetOrganization(organizationName))
            .ReturnsAsync(organization);

        var result = await _teamService.DeleteTeamFromOrganization(organizationName, teamName);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("You are not the owner of this organization."));
        });
    }

    [Test]
    public async Task DeleteTeamFromOrganization_NotAuthorized_ReturnsErrorResponse()
    {
        const string organizationName = "Test Organization";
        const string teamName = "Test Team";
        var organization = new Model.Entity.Organization
        {
            Name = organizationName,
            Teams = new List<Model.Entity.Team> { new Model.Entity.Team { Name = teamName } }
        };
        _organizationServiceMock
            .Setup(m => m.GetOrganization(organizationName))
            .ReturnsAsync(organization);
        _organizationServiceMock
            .Setup(m => m.IsLoggedInUserOwner(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(false));

        var result = await _teamService.DeleteTeamFromOrganization(organizationName, teamName);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("You are not the owner of this organization."));
        });
    }

    [Test]
    public async Task DeleteTeamFromOrganization_OrganizationNotFound_ReturnsErrorResponse()
    {
        const string organizationName = "Nonexistent Organization";
        const string teamName = "Test Team";
        _organizationServiceMock
            .Setup(m => m.GetOrganization(organizationName))
            .ReturnsAsync((Model.Entity.Organization)null);

        var result = await _teamService.DeleteTeamFromOrganization(organizationName, teamName);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Organization not found."));
        });
    }
}

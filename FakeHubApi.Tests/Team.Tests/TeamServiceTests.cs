using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
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
    private Mock<IUserContextService> _userContextServiceMock;

    private Mock<IOrganizationService> _organizationServiceMock;

    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _organizationServiceMock = new Mock<IOrganizationService>();

        _teamService = new TeamService(
            _organizationServiceMock.Object,
            _mapperManagerMock.Object,
            _repositoryManagerMock.Object,
            _userContextServiceMock.Object
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
        };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _mapperManagerMock.Setup(m => m.TeamDtoToTeamMapper.Map(It.IsAny<TeamDto>())).Returns(team);

        _repositoryManagerMock
            .Setup(um => um.TeamRepository.GetByName(It.IsAny<string>()))
            .ReturnsAsync((Model.Entity.Team)null);
        _organizationServiceMock
            .Setup(m => m.GetOrganization(It.IsAny<string>()))
            .ReturnsAsync(organization);

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
        var teamDto = new TeamDto
        {
            Name = "Test Team",
            Description = "Test Description",
            OrganizationName = "Test Organization",
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
        };

        organization.Teams.Add(team);

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _mapperManagerMock.Setup(m => m.TeamDtoToTeamMapper.Map(It.IsAny<TeamDto>())).Returns(team);

        _repositoryManagerMock
            .Setup(um => um.TeamRepository.GetByName(It.IsAny<string>()))
            .ReturnsAsync(team);
        _organizationServiceMock
            .Setup(m => m.GetOrganization(It.IsAny<string>()))
            .ReturnsAsync(organization);

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
        var teamDto = new TeamDto
        {
            Name = "Test Team",
            Description = "Test Description",
            OrganizationName = "Test Organization",
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
        };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user2);

        _mapperManagerMock.Setup(m => m.TeamDtoToTeamMapper.Map(It.IsAny<TeamDto>())).Returns(team);

        _repositoryManagerMock
            .Setup(um => um.TeamRepository.GetByName(It.IsAny<string>()))
            .ReturnsAsync(team);
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
}

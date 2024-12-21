using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FakeHubApi.Tests.Organization.Tests;

public class OrganizationServiceTests
{
    private Mock<IMapperManager> _mapperManagerMock;
    private IOrganizationService _organizationService;
    private Mock<IRepositoryManager> _repositoryManagerMock;
    private Mock<IUserContextService> _userContextServiceMock;
    private Mock<UserManager<User>> _userManagerMock;

    private Mock<IUserService> _userServiceMock;

    private Mock<ICrudRepository<Model.Entity.Organization>> _organizationRepositoryMock;

    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _organizationRepositoryMock = new Mock<ICrudRepository<Model.Entity.Organization>>();
        _userServiceMock = new Mock<IUserService>();

        _userManagerMock = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        _organizationService = new OrganizationService(
            _userManagerMock.Object,
            _mapperManagerMock.Object,
            _repositoryManagerMock.Object,
            _userContextServiceMock.Object,
            _userServiceMock.Object
        );
    }

    [Test]
    public async Task AddOrganization()
    {
        var organizationDto = new OrganizationDto
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
        };

        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
        };

        var user = new User { Id = 1, UserName = "Test User" };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _mapperManagerMock
            .Setup(m => m.OrganizationDtoToOrganizationMapper.Map(It.IsAny<OrganizationDto>()))
            .Returns(organization);

        _repositoryManagerMock
            .Setup(um => um.OrganizationRepository.GetByName(It.IsAny<string>()))
            .ReturnsAsync((Model.Entity.Organization)null);
        _repositoryManagerMock
            .Setup(um => um.OrganizationRepository.AddAsync(It.IsAny<Model.Entity.Organization>()))
            .Returns(Task.FromResult(organization));

        var result = await _organizationService.Add(organizationDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Null);
        });
    }

    [Test]
    public async Task AddOrganization_NameNotUnique()
    {
        var organizationDto = new OrganizationDto
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
        };

        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
        };

        var user = new User { Id = 1, UserName = "Test User" };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _mapperManagerMock
            .Setup(m => m.OrganizationDtoToOrganizationMapper.Map(It.IsAny<OrganizationDto>()))
            .Returns(organization);

        _repositoryManagerMock
            .Setup(um => um.OrganizationRepository.GetByName(It.IsAny<string>()))
            .ReturnsAsync(organization);

        var result = await _organizationService.Add(organizationDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Organization name is not unique."));
        });
    }

    [Test]
    public async Task EditOrganization()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        const string name = "Test Organization";
        var organizationDto = new UpdateOrganizationDto
        {
            Description = "Test Description Edit",
            ImageBase64 = "Test Image Base64",
        };

        var organization = new Model.Entity.Organization
        {
            Name = name,
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = user.Id,
        };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _organizationRepositoryMock
            .Setup(um => um.UpdateAsync(It.IsAny<Model.Entity.Organization>()))
            .Returns(() => null);

        _repositoryManagerMock
            .Setup(um => um.OrganizationRepository.GetByName(It.IsAny<string>()))
            .ReturnsAsync(organization);

        var result = await _organizationService.Update(name, organizationDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Null);
        });
    }

    [Test]
    public async Task EditOrganization_UserWithoutPermission()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        const string name = "Test Organization";
        var organizationDto = new UpdateOrganizationDto
        {
            Description = "Test Description Edit",
            ImageBase64 = "Test Image Base64",
        };

        var organization = new Model.Entity.Organization
        {
            Name = name,
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = 2,
        };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _organizationRepositoryMock
            .Setup(um => um.UpdateAsync(It.IsAny<Model.Entity.Organization>()))
            .Returns(() => null);

        _repositoryManagerMock
            .Setup(um => um.OrganizationRepository.GetByName(It.IsAny<string>()))
            .ReturnsAsync(organization);

        var result = await _organizationService.Update(name, organizationDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.ErrorMessage,
                Is.EqualTo("You are not authorized to update this organization.")
            );
        });
    }

    [Test]
    public async Task EditOrganization_NotExists()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        var name = "Test Organization";
        var organizationDto = new UpdateOrganizationDto
        {
            Description = "Test Description Edit",
            ImageBase64 = "Test Image Base64",
        };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _organizationRepositoryMock
            .Setup(um => um.UpdateAsync(It.IsAny<Model.Entity.Organization>()))
            .Returns(() => null);

        _repositoryManagerMock
            .Setup(um => um.OrganizationRepository.GetByName(It.IsAny<string>()))
            .ReturnsAsync((Model.Entity.Organization)null);

        var result = await _organizationService.Update(name, organizationDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Organization not found."));
        });
    }

    [Test]
    public async Task SearchOrganization()
    {
        var query = "Test";

        var user = new User { Id = 1, UserName = "Test User" };

        var organization = new Model.Entity.Organization
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            OwnerId = user.Id,
        };

        var organizationDto = new OrganizationDto
        {
            Name = "Test Organization",
            Description = "Test Description",
            ImageBase64 = "Test Image Base64",
            Owner = user.UserName,
        };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _mapperManagerMock
            .Setup(m =>
                m.OrganizationDtoToOrganizationMapper.ReverseMap(
                    It.IsAny<Model.Entity.Organization>()
                )
            )
            .Returns(organizationDto);

        _repositoryManagerMock
            .Setup(um => um.OrganizationRepository.Search(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Model.Entity.Organization> { organization });

        var result = await _organizationService.Search(query);

        var responseOrganizations = result.Result as IEnumerable<OrganizationDto>;
        Assert.Multiple(() =>
        {
            Assert.That(responseOrganizations, Is.Not.Null);

            var organizationDtoResult = responseOrganizations?.FirstOrDefault();
            Assert.That(organizationDtoResult, Is.Not.Null);

            Assert.That(organizationDtoResult?.Name, Is.EqualTo(organization.Name));
            Assert.That(organizationDtoResult?.Description, Is.EqualTo(organization.Description));
            Assert.That(organizationDtoResult?.Owner, Is.EqualTo(user.UserName));
        });
    }

    [Test]
    public async Task SearchOrganization_Empty()
    {
        const string wrongQuery = "Wrong";

        var user = new User { Id = 1, UserName = "Test User" };

        _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(user);

        _mapperManagerMock
            .Setup(m =>
                m.OrganizationDtoToOrganizationMapper.ReverseMap(
                    It.IsAny<Model.Entity.Organization>()
                )
            )
            .Returns(new OrganizationDto());
        _repositoryManagerMock
            .Setup(um => um.OrganizationRepository.Search(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Model.Entity.Organization>());

        var result = await _organizationService.Search(wrongQuery);

        var responseOrganizations = result.Result as IEnumerable<OrganizationDto>;
        Assert.Multiple(() =>
        {
            Assert.That(responseOrganizations, Is.Empty);
        });
    }

    [Test]
    public async Task AddUser_EmptyUsernamesList_ReturnsErrorResponse()
    {
        const string orgName = "organizationName";
        var emptyUsernames = new List<string>();

        var result = await _organizationService.AddUser(orgName, emptyUsernames);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("No usernames provided"));
        });
    }

    [Test]
    public async Task AddUser_OrganizationNotFound_ReturnsErrorResponse()
    {
        var usernames = new List<string> { "user1", "user2" };
        const string orgName = "nonExistentOrganization";

        _repositoryManagerMock
            .Setup(rm => rm.OrganizationRepository.GetByName(orgName))
            .ReturnsAsync((Model.Entity.Organization)null);

        var result = await _organizationService.AddUser(orgName, usernames);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Organization not found"));
        });
    }

    [Test]
    public async Task AddUser_NoEligibleUsers_ReturnsErrorResponse()
    {
        const string orgName = "organizationName";
        var usernames = new List<string> { "user1", "user2", "ownerUser" };

        var organization = new Model.Entity.Organization
        {
            Name = orgName,
            Owner = new User { UserName = "ownerUser" },
            Users = [new User { UserName = "user1" }, new User { UserName = "user2" }],
        };

        _repositoryManagerMock
            .Setup(rm => rm.OrganizationRepository.GetByName(orgName))
            .ReturnsAsync(organization);

        _userManagerMock.Setup(um => um.Users).Returns(organization.Users.AsQueryable());

        var result = await _organizationService.AddUser(orgName, usernames);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("No eligible users found"));
        });
    }

    [Test]
    public async Task AddUser_ValidUsersAddedSuccessfully_ReturnsSuccessResponse()
    {
        const string orgName = "organizationName";
        var usernames = new List<string> { "user1", "user2" };

        var organization = new Model.Entity.Organization
        {
            Name = orgName,
            Owner = new User { UserName = "ownerUser" },
            Users = [],
        };

        _repositoryManagerMock
            .Setup(rm => rm.OrganizationRepository.GetByName(orgName))
            .ReturnsAsync(organization);

        var responseUsers = new List<User>
        {
            new() { UserName = "user1" },
            new() { UserName = "user2" },
        };

        _userManagerMock.Setup(um => um.Users).Returns(responseUsers.AsQueryable());

        _mapperManagerMock
            .Setup(m => m.UserToUserDtoMapper.Map(It.IsAny<User>()))
            .Returns((User user) => new UserDto { Username = user.UserName });

        var result = await _organizationService.AddUser(orgName, usernames);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.Result, Has.Count.EqualTo(2));

            Assert.That(organization.Users.Count, Is.EqualTo(2));
            Assert.That(organization.Users.Select(u => u.UserName), Is.EquivalentTo(usernames));

            _repositoryManagerMock.Verify(
                rm => rm.OrganizationRepository.UpdateAsync(organization),
                Times.Once
            );

            var responseUsers = result.Result as List<UserDto>;
            Assert.That(responseUsers.Select(r => r.Username), Is.EquivalentTo(usernames));
            Assert.That(responseUsers, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task DeleteUser_UserNotFound_ReturnsErrorResponse()
    {
        const string username = "nonexistentUser";
        const string orgName = "organizationName";
        var organization = new Model.Entity.Organization
        {
            Name = orgName
        };

        _userManagerMock
            .Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync((User)null);

        _repositoryManagerMock
            .Setup(rm => rm.OrganizationRepository.GetByName(orgName))
            .ReturnsAsync(organization);

        var result = await _organizationService.DeleteUser(orgName, username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found"));
        });
    }

    [Test]
    public async Task DeleteUser_OrganizationNotFound_ReturnsErrorResponse()
    {
        const string username = "user";
        const string orgName = "nonExistentOrganization";

        var user = new User { UserName = username };

        _userManagerMock
            .Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync(user);

        _repositoryManagerMock
            .Setup(rm => rm.OrganizationRepository.GetByName(orgName))
            .ReturnsAsync((Model.Entity.Organization)null);

        var result = await _organizationService.DeleteUser(orgName, username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Organization not found"));
        });
     }

    [Test]
    public async Task DeleteUser_UserNotInOrganization_ReturnsErrorResponse()
    {
        const string username = "user";
        const string orgName = "organizationName";
        var user = new User { UserName = username };
        var organization = new Model.Entity.Organization
        {
            Name = orgName,
            Users = []
        };

        _userManagerMock
           .Setup(um => um.FindByNameAsync(username))
           .ReturnsAsync(user);

        _repositoryManagerMock
            .Setup(rm => rm.OrganizationRepository.GetByName(orgName))
            .ReturnsAsync(organization);

        var result = await _organizationService.DeleteUser(orgName, username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not in organization"));
        });
    }

    [Test]
    public async Task DeleteUser_ValidRequest_ReturnsSuccessResponse()
    {
        const string username = "user";
        const string orgName = "organizationName";
        var user = new User { UserName = username };
        var organization = new Model.Entity.Organization
        {
            Name = orgName,
            Users = new List<User> { user }
        };
        var responseUser = new UserDto { Username = username };

        _userManagerMock
          .Setup(um => um.FindByNameAsync(username))
          .ReturnsAsync(user);

        _repositoryManagerMock
            .Setup(rm => rm.OrganizationRepository.GetByName(orgName))
            .ReturnsAsync(organization);

        _mapperManagerMock.Setup(m => m.UserToUserDtoMapper.Map(user)).Returns(responseUser);

        var result = await _organizationService.DeleteUser(orgName, username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.ErrorMessage, Is.Empty);
            Assert.That((UserDto)result.Result, Is.EqualTo(responseUser));
        });
    }
}

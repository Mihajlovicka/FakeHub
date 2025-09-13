using FakeHubApi.ContainerRegistry;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Moq;
namespace FakeHubApi.Tests.Tag.Tests;   

public class TagServiceTests
{
    private Mock<IRepositoryManager> _repositoryManagerMock;
    private Mock<IUserContextService> _userContextServiceMock;
    private Mock<IHarborService> _harborServiceMock;
    private Mock<IRepositoryService> _repositoryServiceMock;
    private ITagService _tagService;

    [SetUp]
    public void Setup()
    {
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _harborServiceMock = new Mock<IHarborService>();
        _repositoryServiceMock = new Mock<IRepositoryService>();

        _tagService = new TagService(
            _repositoryManagerMock.Object,
            _userContextServiceMock.Object,
            _repositoryServiceMock.Object,
            _harborServiceMock.Object
        );
    }

    [Test]
    public async Task CanDelete_ReturnsTrue_WhenUserIsOwnerOfOrgRepository()
    {
        var user = new User { Id = 1, UserName = "owner" };
        var org = new Model.Entity.Organization
        {
            Id = 10,
            Owner = user,
            Teams = new List<Model.Entity.Team>()
        };
        var repository = new Model.Entity.Repository
        {
            Id = 100,
            OwnedBy = RepositoryOwnedBy.Organization,
            OwnerId = org.Id
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserWithRoleAsync())
            .ReturnsAsync((user, Role.ADMIN.ToString()));
        _repositoryManagerMock.Setup(x => x.RepositoryRepository.GetByIdAsync(repository.Id))
            .ReturnsAsync(repository);
        _repositoryManagerMock.Setup(x => x.OrganizationRepository.GetById(org.Id))
            .ReturnsAsync(org);

        var result = await _tagService.CanDelete(repository.Id);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.True);
    }

    [Test]
    public async Task CanDelete_ReturnsTrue_WhenUserIsAdminTeamMember()
    {
        var user = new User { Id = 2, UserName = "adminuser" };
        var adminTeam = new Model.Entity.Team
        {
            RepositoryId = 200,
            Users = new List<User> { user },
            TeamRole = TeamRole.Admin
        };
        var org = new Model.Entity.Organization
        {
            Id = 20,
            Owner = new User { Id = 99, UserName = "otherowner" },
            Teams = new List<Model.Entity.Team> { adminTeam }
        };
        var repository = new Model.Entity.Repository
        {
            Id = 200,
            OwnedBy = RepositoryOwnedBy.Organization,
            OwnerId = org.Id
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserWithRoleAsync())
            .ReturnsAsync((user, Role.USER.ToString()));
        _repositoryManagerMock.Setup(x => x.RepositoryRepository.GetByIdAsync(repository.Id))
            .ReturnsAsync(repository);
        _repositoryManagerMock.Setup(x => x.OrganizationRepository.GetById(org.Id))
            .ReturnsAsync(org);

        var result = await _tagService.CanDelete(repository.Id);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.True);
    }

    [Test]
    public async Task CanDelete_ReturnsFalse_WhenUserIsNotAllowed()
    {
        var user = new User { Id = 3, UserName = "notallowed" };
        var org = new Model.Entity.Organization
        {
            Id = 30,
            Owner = new User { Id = 88, UserName = "orgowner" },
            Teams = new List<Model.Entity.Team>()
        };
        var repository = new Model.Entity.Repository
        {
            Id = 300,
            OwnedBy = RepositoryOwnedBy.Organization,
            OwnerId = org.Id
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserWithRoleAsync())
            .ReturnsAsync((user, Role.USER.ToString()));
        _repositoryManagerMock.Setup(x => x.RepositoryRepository.GetByIdAsync(repository.Id))
            .ReturnsAsync(repository);
        _repositoryManagerMock.Setup(x => x.OrganizationRepository.GetById(org.Id))
            .ReturnsAsync(org);

        var result = await _tagService.CanDelete(repository.Id);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.False);
    }

    [Test]
    public async Task CanDelete_ReturnsTrue_WhenUserIsOwnerOfUserRepository()
    {
        var user = new User { Id = 4, UserName = "userowner" };
        var repository = new Model.Entity.Repository
        {
            Id = 400,
            OwnedBy = RepositoryOwnedBy.User,
            OwnerId = user.Id
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserWithRoleAsync())
            .ReturnsAsync((user, Role.USER.ToString()));
        _repositoryManagerMock.Setup(x => x.RepositoryRepository.GetByIdAsync(repository.Id))
            .ReturnsAsync(repository);
        _repositoryManagerMock.Setup(x => x.UserRepository.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var result = await _tagService.CanDelete(repository.Id);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.True);
    }

    [Test]
    public async Task CanDelete_ReturnsFalse_WhenRepositoryNotFound()
    {
        _userContextServiceMock.Setup(x => x.GetCurrentUserWithRoleAsync())
            .ReturnsAsync((new User { Id = 5, UserName = "nouser" }, Role.USER.ToString()));
        _repositoryManagerMock.Setup(x => x.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Model.Entity.Repository)null);

        var result = await _tagService.CanDelete(999);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.False);
    }
}
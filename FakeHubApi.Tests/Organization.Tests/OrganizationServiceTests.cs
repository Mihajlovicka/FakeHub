using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Moq;
using Newtonsoft.Json;

namespace FakeHubApi.Tests.Organization.Tests;

public class OrganizationServiceTests
{
    private Mock<IMapperManager> _mapperManagerMock;
    private IOrganizationService _organizationService;
    private Mock<IRepositoryManager> _repositoryManagerMock;
    private Mock<IUserContextService> _userContextServiceMock;

    private Mock<ICrudRepository<Model.Entity.Organization>> _organizationRepositoryMock;

    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _organizationRepositoryMock = new Mock<ICrudRepository<Model.Entity.Organization>>();

        _organizationService = new OrganizationService(
            _mapperManagerMock.Object,
            _repositoryManagerMock.Object,
            _userContextServiceMock.Object
        );
    }

    [Test]
    public async Task AddOgranization()
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
    public async Task AddOgranization_NameNotUnique()
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
    public async Task EditOgranization()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        var name = "Test Organization";
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
    public async Task EditOgranization_UserWithoutPermition()
    {
        var user = new User { Id = 1, UserName = "Test User" };
        var name = "Test Organization";
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
    public async Task EditOgranization_NotExists()
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
    public async Task SearchOgranization()
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
    public async Task SearchOgranization_Empty()
    {
        var wrongquery = "Wrong";

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

        var result = await _organizationService.Search(wrongquery);

        var responseOrganizations = result.Result as IEnumerable<OrganizationDto>;
        Assert.Multiple(() =>
        {
            Assert.That(responseOrganizations, Is.Empty);
        });
    }
}

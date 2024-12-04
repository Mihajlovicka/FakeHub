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

    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _userContextServiceMock = new Mock<IUserContextService>();

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
}

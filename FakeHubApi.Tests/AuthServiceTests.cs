using FakeHubApi.Data;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FakeHubApi.Tests;

public class Tests
{
    private Mock<AppDbContext> _mockDbContext;
    private Mock<IRepositoryManager> _repositoryManagerMock;
    private Mock<IMapperManager> _mapperManagerMock;
    private Mock<UserManager<ApplicationUser>> _mockUserManager;
    private IAuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mockDbContext = new Mock<AppDbContext>();

        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _mapperManagerMock = new Mock<IMapperManager>();

        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        _repositoryManagerMock
            .Setup(repo => repo.UserRepository)
            .Returns(new Mock<IUserRepository>().Object);

        _authService = new Service.Implementation.AuthService(
            _repositoryManagerMock.Object,
            _mockUserManager.Object,
            _mapperManagerMock.Object
        );
    }

    [Test]
    public async Task Register_UserCreatedSuccessfully_ReturnsEmptyString()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Username = "UserName",
            Password = "Password123!",
            Role = "User"
        };

        var user = new ApplicationUser
        {
            Email = "test@example.com",
            UserName = "UserName",
        };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToApplicationUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _repositoryManagerMock
            .Setup(r => r.UserRepository.GetByUsername(It.IsAny<string>()))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager
            .Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.Register(registrationRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ErrorMessage, Is.EqualTo(""));
        });
    }

    [Test]
    public async Task Register_UserCreationFails_ReturnsErrorDescription()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Username = "UserName",
            Password = "Password123!",
            Role = "User"
        };

        var user = new ApplicationUser
        {
            Email = "test@example.com",
            UserName = "UserName",
        };

        var identityError = new IdentityError { Description = "Error creating user." };
        var identityResult = IdentityResult.Failed(identityError);

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToApplicationUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _authService.Register(registrationRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo(identityError.Description));
        });
    }

    [Test]
    public async Task Register_ExceptionThrown_ReturnsErrorMessage()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Username = "UserName",
            Password = "Password123!",
            Role = "User"
        };

        var user = new ApplicationUser
        {
            Email = "test@example.com",
            UserName = "UserName",
        };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToApplicationUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Throws(new System.Exception("An error occurred during user creation"));

        // Act
        var result = await _authService.Register(registrationRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("An error occurred during user creation"));
        });
    }
}

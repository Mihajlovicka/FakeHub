using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FakeHubApi.Tests.Auth.Tests;

public class Tests
{
    private Mock<IMapperManager> _mapperManagerMock;
    private Mock<UserManager<User>> _mockUserManager;
    private Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private IAuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();

        _mockUserManager = new Mock<UserManager<User>>(
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

        _authService = new AuthService(
            _mockUserManager.Object,
            _mapperManagerMock.Object,
            _mockJwtTokenGenerator.Object
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
        };

        var user = new User { Email = "test@example.com", UserName = "UserName" };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToApplicationUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager
            .Setup(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        var result = await _authService.Register(registrationRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Null);
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
        };

        var user = new User { Email = "test@example.com", UserName = "UserName" };

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
            .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
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
        };

        var user = new User { Email = "test@example.com", UserName = "UserName" };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToApplicationUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
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

    [Test]
    public async Task Login_UserValid_ReturnsLoginResponseDto()
    {
        // Arrange
        var loginRequestDto = new LoginRequestDto
        {
            Email = "testuser@example.com",
            Password = "Password123!",
        };

        var user = new User
        {
            Email = loginRequestDto.Email,
            NormalizedEmail = loginRequestDto.Email.ToUpper(),
        };

        _mockUserManager.Setup(r => r.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockUserManager
            .Setup(um => um.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "User" });

        _mockJwtTokenGenerator
            .Setup(jwt => jwt.GenerateToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .Returns("mock-token");

        // Act
        var result = await _authService.Login(loginRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That((result.Result as LoginResponseDto).Token, Is.EqualTo("mock-token"));
        });
    }

    [Test]
    public async Task Login_UserInvalid_ReturnsLoginResponseDtoWithEmptyToken()
    {
        // Arrange
        var loginRequestDto = new LoginRequestDto
        {
            Email = "testuser@example.com",
            Password = "Password123!",
        };

        _mockUserManager
            .Setup(r => r.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);

        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.Login(loginRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
        });
    }

    [Test]
    public async Task GetUserProfileByUsernameAsync_UserFound_ReturnsUserProfile()
    {
        // Arrange
        var username = "testUser";
        var user = new User
        {
            UserName = username,
            Email = "testuser@example.com"
        };
        var userProfileDto = new UserProfileResponseDto
        {
            Username = user.UserName,
            Email = user.Email
        };
        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync(user);
        _mapperManagerMock
            .Setup(m => m.ApplicationUserToUserProfileResponseDto.Map(It.IsAny<User>()))
            .Returns(userProfileDto);
        // Act
        var result = await _authService.GetUserProfileByUsernameAsync(username);
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(userProfileDto));
            Assert.That(((UserProfileResponseDto)result.Result).Username, Is.EqualTo(user.UserName));
            Assert.That(result.ErrorMessage, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task GetUserProfileByUsernameAsync_UserNotFound_ReturnsErrorResponse()
    {
        // Arrange
        var username = "nonExistentUser";
        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync((User)null);
        // Act
        var result = await _authService.GetUserProfileByUsernameAsync(username);
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found"));
        });
    }

    [Test]
    public async Task GetUserProfileByUsernameAsync_ExceptionThrown_ReturnsErrorResponse()
    {
        // Arrange
        var username = "testUser";
        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ThrowsAsync(new System.Exception("An unexpected error occurred"));
        // Act
        var result = await _authService.GetUserProfileByUsernameAsync(username);
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("An unexpected error occurred"));
        });
    }
}
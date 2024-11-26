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
    private Mock<IUserContextService> _userContextService;

    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _userContextService = new Mock<IUserContextService>();
        
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
            _mockJwtTokenGenerator.Object,
            _userContextService.Object
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
        var result = await _authService.Register(registrationRequestDto, Role.USER.ToString());

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
        var result = await _authService.Register(registrationRequestDto, Role.USER.ToString());

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
        var result = await _authService.Register(registrationRequestDto, Role.USER.ToString());

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
            Assert.That((result.Result as LoginResponseDto)?.Token, Is.EqualTo("mock-token"));
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
            .ReturnsAsync((User)null!);

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
    public async Task Register_AdminCreatedSuccessfully_ReturnsEmptyString()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "newadmin@example.com",
            Username = "newAdmin",
            Password = "Password123!",
        };

        var user = new User { Email = "newadmin@example.com", UserName = "newAdmin" };

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
        var result = await _authService.Register(registrationRequestDto, Role.ADMIN.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Null);
        });
    }
    
    [Test]
    public async Task RegisterAdmin_ExceptionThrown_ReturnsErrorMessage()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "newadmin@example.com",
            Username = "newAdmin",
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
        var result = await _authService.Register(registrationRequestDto, Role.ADMIN.ToString());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("An error occurred during user creation"));
        });
    }

    [Test]
    public async Task ChangePassword_PasswordsDoNotMatch_ReturnsErrorResponse()
    {
        // Arrange
        var changePasswordRequestDto = new ChangePasswordRequestDto
        {
            OldPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            NewPasswordConfirm = "DifferentNewPassword123!"
        };

        // Act
        var result = await _authService.ChangePassword(changePasswordRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("New password and confirmation do not match"));
        });
    }

    [Test]
    public async Task ChangePassword_SuccessfulChange_ReturnsSuccessResponseWithEnabledTrueInToken()
    {
        // Arrange
        var changePasswordRequestDto = new ChangePasswordRequestDto
        {
            OldPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            NewPasswordConfirm = "NewPassword123!"
        };

        var user = new User();

        _userContextService.Setup(ucs => ucs.GetCurrentUserAsync()).ReturnsAsync(user);

        _mockUserManager
            .Setup(um => um.ChangePasswordAsync(user, changePasswordRequestDto.OldPassword, changePasswordRequestDto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager
            .Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Mocking the token to include the "enabled" property with value "true"
        const string generatedToken = "{\"token\": \"mock-token\", \"enabled\": true}";
        _mockJwtTokenGenerator
            .Setup(jwt => jwt.GenerateToken(user, It.IsAny<IList<string>>()))
            .Returns(generatedToken);

        // Act
        var result = await _authService.ChangePassword(changePasswordRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);

            var loginResponse = result.Result as LoginResponseDto;
            Assert.That(loginResponse?.Token, Does.Contain("\"enabled\": true"));
        });
    }


    [Test]
    public async Task ChangePassword_PasswordChangeFails_ReturnsErrorResponse()
    {
        // Arrange
        var changePasswordRequestDto = new ChangePasswordRequestDto
        {
            OldPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            NewPasswordConfirm = "NewPassword123!"
        };

        var user = new User();
        var identityError = new IdentityError { Description = "Password change failed" };
        var identityResult = IdentityResult.Failed(identityError);

        _userContextService.Setup(ucs => ucs.GetCurrentUserAsync()).ReturnsAsync(user);

        _mockUserManager
            .Setup(um => um.ChangePasswordAsync(user, changePasswordRequestDto.OldPassword, changePasswordRequestDto.NewPassword))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _authService.ChangePassword(changePasswordRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Password change failed"));
        });
    }

    [Test]
    public async Task ChangePassword_UserUpdateFails_ReturnsErrorResponse()
    {
        // Arrange
        var changePasswordRequestDto = new ChangePasswordRequestDto
        {
            OldPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            NewPasswordConfirm = "NewPassword123!"
        };

        var user = new User();

        _userContextService.Setup(ucs => ucs.GetCurrentUserAsync()).ReturnsAsync(user);

        _mockUserManager
            .Setup(um => um.ChangePasswordAsync(user, changePasswordRequestDto.OldPassword, changePasswordRequestDto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        var updateError = new IdentityError { Description = "Failed to update user settings" };
        var updateResult = IdentityResult.Failed(updateError);

        _mockUserManager
            .Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(updateResult);

        // Act
        var result = await _authService.ChangePassword(changePasswordRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Failed to update user settings"));
        });
    }

}

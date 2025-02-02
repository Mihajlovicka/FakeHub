using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FakeHubApi.Tests.Users.Tests;

public class UserServiceTests
{

    private Mock<IMapperManager> _mapperManagerMock;
    private Mock<UserManager<User>> _mockUserManager;
    private Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private IUserService _userService;
    private Mock<IUserContextService> _userContextService;
    private Mock<IRepositoryManager> _repositoryManager;
    
    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _userContextService = new Mock<IUserContextService>();
        _repositoryManager = new Mock<IRepositoryManager>();
        
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

        _userService = new UserService(
            _mockUserManager.Object,
            _mapperManagerMock.Object,
            _mockJwtTokenGenerator.Object,
            _userContextService.Object,
            _repositoryManager.Object
        );
    }
    
    [Test]
    public async Task GetUserProfileByUsernameAsync_UserFound_ReturnsUserProfile()
    {
        const string username = "testUser";
        var user = new User
        {
            UserName = username,
            Email = "testuser@example.com"
        };
        var userProfileDto = new UserDto
        {
            Username = user.UserName,
            Email = user.Email
        };
        var userRoles = new List<string>
        {
            "USER"
        };
        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync(user);
        _mapperManagerMock
            .Setup(m => m.UserToUserDtoMapper.Map(It.IsAny<User>()))
            .Returns(userProfileDto);
        _mockUserManager
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(userRoles);

        var result = await _userService.GetUserProfileByUsernameAsync(username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(userProfileDto));
            Assert.That(((UserDto)result.Result).Username, Is.EqualTo(user.UserName));
            Assert.That(result.ErrorMessage, Is.EqualTo(string.Empty));
        });
    }
    
    [Test]
    public async Task GetUserProfileByUsernameAsync_UserNotFound_ReturnsErrorResponse()
    {
        const string username = "nonExistentUser";
        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync((User)null);

        var result = await _userService.GetUserProfileByUsernameAsync(username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found"));
             });
    }
    
    [Test]
    public async Task ChangePassword_PasswordsDoNotMatch_ReturnsErrorResponse()
    {
        var changePasswordRequestDto = new ChangePasswordRequestDto
        {
            OldPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            NewPasswordConfirm = "DifferentNewPassword123!"
        };

        var result = await _userService.ChangePassword(changePasswordRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("New password and confirmation do not match"));
        });
    }

    [Test]
    public async Task GetUserProfileByUsernameAsync_ExceptionThrown_ReturnsErrorResponse()
    {
        var username = "testUser";
        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ThrowsAsync(new System.Exception("An unexpected error occurred"));

        var result = await _userService.GetUserProfileByUsernameAsync(username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("An unexpected error occurred"));
        });
    }

    [Test]
    public async Task ChangePassword_SuccessfulChange_ReturnsSuccessResponseWithEnabledTrueInToken()
    {
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

        const string generatedToken = "{\"token\": \"mock-token\", \"enabled\": true}";
        _mockJwtTokenGenerator
            .Setup(jwt => jwt.GenerateToken(user, It.IsAny<IList<string>>()))
            .Returns(generatedToken);

        var result = await _userService.ChangePassword(changePasswordRequestDto);

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
        
        var result = await _userService.ChangePassword(changePasswordRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Password change failed"));
        });
    }

    [Test]
    public async Task ChangePassword_UserUpdateFails_ReturnsErrorResponse()
    {
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

        var result = await _userService.ChangePassword(changePasswordRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Failed to update user settings"));
        });
    }

    [Test]
    public async Task ChangeEmail_SuccessfulChange_ReturnsSuccessResponseWithToken()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "CorrectPassword123!",
            NewEmail = "newemail@example.com"
        };

        var user = new User { Email = "email@example.com" };
        var changeEmailToken = "mock-change-email-token";
        var roles = new List<string> { "USER" };
        var responseToken = "mock-response-token";
        var userDto = new UserDto()
        {
            Username = user.Email,
            Role = "USER",
        };

        _userContextService.Setup(ucs => ucs.GetCurrentUserAsync()).ReturnsAsync(user);
        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(user, changeEmailRequestDto.Password))
            .ReturnsAsync(true);
        _mockUserManager
            .Setup(um => um.GenerateChangeEmailTokenAsync(user, changeEmailRequestDto.NewEmail))
            .ReturnsAsync(changeEmailToken);
        _mockUserManager
            .Setup(um => um.ChangeEmailAsync(user, changeEmailRequestDto.NewEmail, changeEmailToken))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles);
        _mockJwtTokenGenerator
            .Setup(jtg => jtg.GenerateToken(user, roles))
            .Returns(responseToken);

        var result = await _userService.ChangeEmailAsync(changeEmailRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(((LoginResponseDto)result.Result).Token, Is.EqualTo(responseToken));
        });
    }

    [Test]
    public async Task ChangeEmail_UserNotFound_ReturnsErrorResponse()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "CorrectPassword123!",
            NewEmail = "newemail@example.com"
        };

        _userContextService.Setup(ucs => ucs.GetCurrentUserAsync()).ReturnsAsync((User)null);

        var result = await _userService.ChangeEmailAsync(changeEmailRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found in current context"));
        });
    }

    [Test]
    public async Task ChangeEmail_InvalidPassword_ReturnsErrorResponse()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "InvalidPassword123!",
            NewEmail = "newemail@example.com"
        };

        var user = new User();

        _userContextService.Setup(ucs => ucs.GetCurrentUserAsync()).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, changeEmailRequestDto.Password)).ReturnsAsync(false);

        var result = await _userService.ChangeEmailAsync(changeEmailRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Password is incorrect"));
        });
    }

    [Test]
    public async Task ChangeEmail_SameEmail_ReturnsErrorResponse()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "CorrectPassword123!",
            NewEmail = "currentemail@example.com"
        };

        var user = new User
        {
            Email = "currentemail@example.com"
        };

        _userContextService
            .Setup(ucs => ucs.GetCurrentUserAsync())
            .ReturnsAsync(user);
        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(user, changeEmailRequestDto.Password))
            .ReturnsAsync(true);

        var result = await _userService.ChangeEmailAsync(changeEmailRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Email can't be the same as current"));
        });
    }

    [Test]
    public async Task ChangeEmail_ChangeEmailFails_ReturnsErrorResponse()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "CorrectPassword123!",
            NewEmail = "newemail@example.com"
        };
        var user = new User { Email = "email@example.com" };
        var changeEmailToken = "mock-change-email-token";
        var emailErrorMessage = "User email change failed";
        var emailChangeError = new IdentityError { Description = emailErrorMessage };
        var emailChangeResult = IdentityResult.Failed(emailChangeError);

        _userContextService
            .Setup(ucs => ucs.GetCurrentUserAsync())
            .ReturnsAsync(user);
        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(user, changeEmailRequestDto.Password))
            .ReturnsAsync(true);
        _mockUserManager
            .Setup(um => um.GenerateChangeEmailTokenAsync(user, changeEmailRequestDto.NewEmail))
            .ReturnsAsync(changeEmailToken);
        _mockUserManager
            .Setup(um => um.ChangeEmailAsync(user, changeEmailRequestDto.NewEmail, changeEmailToken))
            .ReturnsAsync(emailChangeResult);

        var result = await _userService.ChangeEmailAsync(changeEmailRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo(emailErrorMessage));
        });
    }

    [Test]
    public async Task ChangeEmail_UpdateUserFails_ReturnsErrorResponse()
    {
        var changeEmailRequestDto = new ChangeEmailRequestDto
        {
            Password = "CorrectPassword123!",
            NewEmail = "newemail@example.com"
        };
        var user = new User { Email = "email@example.com" };
        var changeEmailToken = "mock-change-email-token";
        var updateUserErrorMessage = "Update current user failed";
        var updateUserError = new IdentityError { Description = updateUserErrorMessage };
        var updateUserResult = IdentityResult.Failed(updateUserError);

        _userContextService
            .Setup(ucs => ucs.GetCurrentUserAsync())
            .ReturnsAsync(user);
        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(user, changeEmailRequestDto.Password))
            .ReturnsAsync(true);
        _mockUserManager
            .Setup(um => um.GenerateChangeEmailTokenAsync(user, changeEmailRequestDto.NewEmail))
            .ReturnsAsync(changeEmailToken);
        _mockUserManager
            .Setup(um => um.ChangeEmailAsync(user, changeEmailRequestDto.NewEmail, changeEmailToken))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.UpdateAsync(user)).ReturnsAsync(updateUserResult);

        var result = await _userService.ChangeEmailAsync(changeEmailRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo(updateUserErrorMessage));
        });
    }
    
    [Test]
    public async Task ChangeUserBadgeAsync_UserNotFound_ReturnsErrorResponse()
    {
        var changeUserBadgeRequestDto = new ChangeUserBadgeRequestDto
        {
            Badge = Badge.VerifiedPubisher,
            Username = "nonexistentUser"
        };

        _mockUserManager
            .Setup(um => um.FindByNameAsync(changeUserBadgeRequestDto.Username))
            .ReturnsAsync((User)null);

        var result = await _userService.ChangeUserBadgeAsync(changeUserBadgeRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found"));
        });

    }

    [Test]
    public async Task ChangeUserBadgeAsync_InvalidBadge_ReturnsErrorResponse()
    {
        var changeUserBadgeRequestDto = new ChangeUserBadgeRequestDto
        {
            Badge = (Badge)15,
            Username = "validUser"
        };

        var user = new User();
        _mockUserManager
            .Setup(um => um.FindByNameAsync(changeUserBadgeRequestDto.Username))
            .ReturnsAsync(user);

        var result = await _userService.ChangeUserBadgeAsync(changeUserBadgeRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid badge value"));
        });
    }

    [Test]
    public async Task ChangeUserBadgeAsync_FailedUpdateUserBadge_ReturnsErrorResponse()
    {
        var changeUserBadgeRequestDto = new ChangeUserBadgeRequestDto
        {
            Badge = Badge.VerifiedPubisher,
            Username = "validUser"
        };
        var user = new User();
        _mockUserManager
            .Setup(um => um.FindByNameAsync(changeUserBadgeRequestDto.Username))
            .ReturnsAsync(user);

        var identityError = new IdentityError { Description = "Failed to update badge" };
        _mockUserManager
            .Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var result = await _userService.ChangeUserBadgeAsync(changeUserBadgeRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Failed to update badge"));
        });
    }

    [Test]
    public async Task ChangeUserBadgeAsync_SuccessfulUpdate_ReturnsSuccessResponse()
    {
        var user = new User();
        var userProfileResponseDto = new UserDto();

        var changeUserBadgeRequestDto = new ChangeUserBadgeRequestDto
        {
            Badge = Badge.VerifiedPubisher,
            Username = "validUser"
        };

        _mockUserManager
            .Setup(um => um.FindByNameAsync(changeUserBadgeRequestDto.Username))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _mapperManagerMock
            .Setup(m => m.UserToUserDtoMapper
            .Map(user))
            .Returns(userProfileResponseDto);

        var result = await _userService.ChangeUserBadgeAsync(changeUserBadgeRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.ErrorMessage, Is.Empty);
            Assert.AreEqual(userProfileResponseDto, result.Result);
        });
    }
    
    [Test]
    public async Task GetUserProfileByUsernameAsync_ExceptionInServiceLayer_ReturnsErrorResponse()
    {
        const string username = "testUser";
    
        // Simulating an unexpected exception in the service layer
        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ThrowsAsync(new Exception("Unexpected error in service layer"));

        var result = await _userService.GetUserProfileByUsernameAsync(username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Unexpected error in service layer"));
        });
    }

    [Test]
    public async Task GetUserProfileByUsernameAsync_SlowRequest_TimeoutOccurs_ReturnsErrorResponse()
    {
        const string username = "testUser";
        var user = new User { UserName = username, Email = "testuser@example.com" };

        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ThrowsAsync(new TimeoutException("Operation timed out"));

        var result = await _userService.GetUserProfileByUsernameAsync(username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Operation timed out"));
        });
    }

    [Test]
    public async Task GetUserProfileByUsernameAsync_MapFailure_ReturnsErrorResponse()
    {
        const string username = "testUser";
        var user = new User { UserName = username, Email = "testuser@example.com" };

        // Simulate the mapping failure
        _mockUserManager
            .Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync(user);
        
        _mapperManagerMock
            .Setup(m => m.UserToUserDtoMapper.Map(It.IsAny<User>()))
            .Throws(new Exception("Mapping failed"));

        var result = await _userService.GetUserProfileByUsernameAsync(username);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
            Assert.That(result.ErrorMessage, Is.EqualTo("Mapping failed"));
        });
    }

    
}
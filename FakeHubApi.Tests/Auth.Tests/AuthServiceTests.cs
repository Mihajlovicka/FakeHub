using FakeHubApi.ContainerRegistry;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FakeHubApi.Tests.Auth.Tests;

public class AuthServiceTests
{
    private Mock<IMapperManager> _mapperManagerMock;
    private Mock<UserManager<User>> _mockUserManager;
    private Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private IAuthService _authService;
    private Mock<IUserContextService> _userContextService;
    private Mock<IHarborService> _harborServiceMock;

    [SetUp]
    public void Setup()
    {
        _mapperManagerMock = new Mock<IMapperManager>();
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _userContextService = new Mock<IUserContextService>();
        _harborServiceMock = new Mock<IHarborService>();

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
            _harborServiceMock.Object
        );
    }

    [Test]
    public async Task Register_UserCreatedSuccessfully_ReturnsEmptyString()
    {
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Username = "UserName",
            Password = "Password123!",
        };

        var user = new User { Email = "test@example.com", UserName = "UserName" };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToUserMapper.Map(
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
                m.RegistrationsRequestDtoToUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(identityResult);

        var result = await _authService.Register(registrationRequestDto, Role.USER.ToString());
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo(identityError.Description));
        });
    }

    [Test]
    public async Task Register_ExceptionThrown_ReturnsErrorMessage()
    {
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Username = "UserName",
            Password = "Password123!",
        };

        var user = new User { Email = "test@example.com", UserName = "UserName" };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Throws(new System.Exception("An error occurred during user creation"));

        var result = await _authService.Register(registrationRequestDto, Role.USER.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("An error occurred during user creation"));
        });
    }

    [Test]
    public async Task Login_UserValid_ReturnsLoginResponseDto()
    {
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

        var userDto = new UserDto()
        {
            Username = loginRequestDto.Email,
            Role = "USER",
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

        var result = await _authService.Login(loginRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
        });
    }
    
    [Test]
    public async Task Register_AdminCreatedSuccessfully_ReturnsEmptyString()
    {
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "newadmin@example.com",
            Username = "newAdmin",
            Password = "Password123!",
        };

        var user = new User { Email = "newadmin@example.com", UserName = "newAdmin" };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToUserMapper.Map(
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
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "newadmin@example.com",
            Username = "newAdmin",
            Password = "Password123!",
        };

        var user = new User { Email = "test@example.com", UserName = "UserName" };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Throws(new System.Exception("An error occurred during user creation"));

        var result = await _authService.Register(registrationRequestDto, Role.ADMIN.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("An error occurred during user creation"));
        });
    }
}

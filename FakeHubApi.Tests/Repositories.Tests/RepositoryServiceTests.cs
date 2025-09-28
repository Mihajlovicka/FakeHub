using FakeHubApi.ContainerRegistry;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Redis;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FakeHubApi.Tests.Repositories.Tests
{
    public class RepositoryServiceTests
    {
        private Mock<IMapperManager> _repositoryMapperMock;
        private Mock<IOrganizationService> _organizationServiceMock;
        private Mock<IRepositoryManager> _repositoryManagerMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IUserService> _userServiceMock;
        private IRepositoryService _repositoryService;
        private Mock<IHarborService> _harborServiceMock;
        private Mock<IRedisCacheService> _redisCacheServiceMock;
        private Mock<UserManager<User>> _userManagerMock;

        [SetUp]
        public void Setup()
        {
            _repositoryMapperMock = new Mock<IMapperManager>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _userServiceMock = new Mock<IUserService>();
            _harborServiceMock = new Mock<IHarborService>();
            _redisCacheServiceMock = new Mock<IRedisCacheService>();

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

            _repositoryService = new RepositoryService(
                _repositoryMapperMock.Object,
                _organizationServiceMock.Object,
                _repositoryManagerMock.Object,
                _userContextServiceMock.Object,
                _userServiceMock.Object,
                _harborServiceMock.Object,
                _redisCacheServiceMock.Object,
                _userManagerMock.Object
            );
        }

        [Test]
        public async Task Save_RepositoryWithUniqueName_SuccessResponse()
        {
            var currentUser = new User { Id = 99, UserName = "TestUser", HarborUserId = 1001 };
            var organization = new Model.Entity.Organization { Id = 1, Name = "Test Organization" };
            var repositoryDto = new RepositoryDto { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };
            var repository = new Model.Entity.Repository { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };

            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.Map(repositoryDto)).Returns(repository);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);
            _organizationServiceMock.Setup(m => m.GetOrganizationById(It.IsAny<int>())).ReturnsAsync(organization);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "USER"));
            _repositoryManagerMock.Setup(m => m.UserRepository.GetByIdAsync(currentUser.Id)).ReturnsAsync(currentUser);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.That(response.Success, Is.True);
        }

        [Test]
        public async Task Save_RepositoryWithDuplicateName_ErrorResponse()
        {
            var repositoryDto = new RepositoryDto { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };
            var repository = new Model.Entity.Repository { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };

            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.Map(repositoryDto)).Returns(repository);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(repository);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Name must be unique"));
            });
        }

        [Test]
        public async Task Save_RepositoryWithInvalidOrganization_ErrorResponse()
        {
            var repositoryDto = new RepositoryDto { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.Organization };
            var repository = new Model.Entity.Repository { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.Organization };
            var org = new Model.Entity.Organization { Id = 1 };

            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.Map(repositoryDto)).Returns(repository);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _organizationServiceMock.Setup(m => m.GetOrganizationById(It.IsAny<int>())).ReturnsAsync((Model.Entity.Organization)null);
            _repositoryManagerMock.Setup(m => m.OrganizationRepository.GetById(It.IsAny<int>())).ReturnsAsync((Model.Entity.Organization)null);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Organization does not exist"));
            });
        }

        [Test]
        public async Task Save_RepositoryWithOwnerIdMinusOne_UsesCurrentUser()
        {
            var repositoryDto = new RepositoryDto { OwnerId = -1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };
            var repository = new Model.Entity.Repository { OwnerId = -1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };
            var currentUser = new User { Id = 99 };

            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.Map(repositoryDto)).Returns(repository);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "USER"));
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);
            _organizationServiceMock
               .Setup(o => o.GetOrganizationById(99))
               .ReturnsAsync(new Model.Entity.Organization { Id = 99, Name = "TestUser" });
            _repositoryManagerMock.Setup(m => m.UserRepository.GetByIdAsync(currentUser.Id)).ReturnsAsync(currentUser);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.That(response.Success, Is.True);
            _repositoryMapperMock.Verify(m => m.RepositoryDtoToRepositoryMapper.Map(repositoryDto), Times.Once);
            _userContextServiceMock.Verify(m => m.GetCurrentUserWithRoleAsync(), Times.Once);
        }

        [Test]
        public async Task Save_WhenUserIsAdmin_ReturnsSuccessResponse()
        {
            var organization = new Model.Entity.Organization { Id = 1, Name = "Test Organization" };
            var repositoryDto = new RepositoryDto { OwnerId = -2, Name = "AdminRepository", OwnedBy = RepositoryOwnedBy.Admin };
            var repository = new Model.Entity.Repository { OwnerId = -2, Name = "AdminRepository", OwnedBy = RepositoryOwnedBy.Admin };
            var currentUser = new User { Id = 1 };

            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.Map(repositoryDto)).Returns(repository);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "ADMIN"));
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);
            _organizationServiceMock.Setup(m => m.GetOrganizationById(It.IsAny<int>())).ReturnsAsync(organization);
            _repositoryManagerMock.Setup(m => m.UserRepository.GetByIdAsync(currentUser.Id)).ReturnsAsync(currentUser);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.True);
                Assert.That(response.ErrorMessage, Is.Empty);
                Assert.That(repository.Badge, Is.EqualTo(Badge.DockerOfficialImage));
                Assert.That(repository.OwnerId, Is.EqualTo(1));
                Assert.That(repository.OwnedBy, Is.EqualTo(RepositoryOwnedBy.Admin));
            });
        }

        [Test]
        public async Task Save_WhenUserIsSuperAdmin_ReturnsSuccessResponse()
        {
            var organization = new Model.Entity.Organization { Id = 1, Name = "Test Organization" };
            var repositoryDto = new RepositoryDto { OwnerId = -2, Name = "SuperAdminRepository", OwnedBy = RepositoryOwnedBy.SuperAdmin };
            var repository = new Model.Entity.Repository { OwnerId = -2, Name = "SuperAdminRepository", OwnedBy = RepositoryOwnedBy.SuperAdmin };
            var currentUser = new User { Id = 11 };

            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.Map(repositoryDto)).Returns(repository);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "SUPERADMIN"));
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);
            _organizationServiceMock.Setup(m => m.GetOrganizationById(It.IsAny<int>())).ReturnsAsync(organization);
            _repositoryManagerMock.Setup(m => m.UserRepository.GetByIdAsync(currentUser.Id)).ReturnsAsync(currentUser);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.True);
                Assert.That(response.ErrorMessage, Is.Empty);
                Assert.That(repository.Badge, Is.EqualTo(Badge.DockerOfficialImage));
                Assert.That(repository.OwnerId, Is.EqualTo(11));
                Assert.That(repository.OwnedBy, Is.EqualTo(RepositoryOwnedBy.SuperAdmin));
            });
        }

        [Test]
        public async Task Save_WhenUserIsUserWithBadge_RepositoryBadgeEqualsUserBadge()
        {
            var organization = new Model.Entity.Organization { Id = 1, Name = "Test Organization" };
            var repositoryDto = new RepositoryDto { OwnerId = 1, Name = "UserRepo", OwnedBy = RepositoryOwnedBy.User };
            var repository = new Model.Entity.Repository { OwnerId = 1, Name = "UserRepo", OwnedBy = RepositoryOwnedBy.User };
            var currentUser = new User { Id = 1, Badge = Badge.SponsoredOSS };

            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.Map(repositoryDto)).Returns(repository);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "USER"));
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);
            _organizationServiceMock.Setup(m => m.GetOrganizationById(It.IsAny<int>())).ReturnsAsync(organization);
            _repositoryManagerMock.Setup(m => m.UserRepository.GetByIdAsync(currentUser.Id)).ReturnsAsync(currentUser);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.True);
                Assert.That(repository.Badge, Is.EqualTo(Badge.SponsoredOSS));
            });
        }

        [Test]
        public async Task GetAllRepositoriesForCurrentUser_UserRole_ReturnsSuccessResponse()
        {
            var user = new User { UserName = "User", Id = 1 };
            var role = "USER";
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = user.Id, Name = "UserRepo1", OwnedBy = RepositoryOwnedBy.User },
                new() { Id = 2, OwnerId = user.Id, Name = "UserRepo2", OwnedBy = RepositoryOwnedBy.User }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy
            }).ToList();

            _userContextServiceMock
                .Setup(m => m.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((user, role));

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetUserRepositoriesByOwnerId(user.Id, false))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(m => m.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(user);

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositories[0].Id))
                .ReturnsAsync(repositories[0]);

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositories[1].Id))
                .ReturnsAsync(repositories[1]);

            var response = await _repositoryService.GetAllRepositoriesForCurrentUser();

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Name, Is.EqualTo("UserRepo1"));
                Assert.That(resultDtos![1].Name, Is.EqualTo("UserRepo2"));
                Assert.That(resultDtos![0].FullName, Is.EqualTo("User-UserRepo1/UserRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("User-UserRepo2/UserRepo2"));
            });
        }


        [Test]
        public async Task GetAllRepositoriesForCurrentUser_AdminRole_ReturnsSuccessResponse()
        {
            var user = new User { UserName = "Admin", Id = 1 };
            var role = "ADMIN";
            var repositories = new List<Model.Entity.Repository>
            {
                 new() { Id = 1, OwnerId = user.Id, Name = "AdminRepo1", OwnedBy = RepositoryOwnedBy.Admin },
                 new() { Id = 2, OwnerId = user.Id, Name = "AdminRepo2", OwnedBy = RepositoryOwnedBy.Admin }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy
            }).ToList();

            _userContextServiceMock
                .Setup(m => m.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((user, role));

            _repositoryManagerMock
                .Setup(m => m.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(user);
            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetAllAsync())
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositories[0].Id))
                .ReturnsAsync(repositories[0]);

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositories[1].Id))
                .ReturnsAsync(repositories[1]);

            var response = await _repositoryService.GetAllRepositoriesForCurrentUser();

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Name, Is.EqualTo("AdminRepo1"));
                Assert.That(resultDtos![1].Name, Is.EqualTo("AdminRepo2"));
                Assert.That(resultDtos![0].FullName, Is.EqualTo("Admin-AdminRepo1/AdminRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("Admin-AdminRepo2/AdminRepo2"));
            });
        }

        [Test]
        public async Task GetAllRepositoriesForCurrentUser_NoRepositories_ReturnsEmptyList() //pada
        {
            var user = new User { UserName = "UserWithoutRepos", Id = 1 };
            var role = "USER";

            _userContextServiceMock
                .Setup(m => m.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((user, role));

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetUserRepositoriesByOwnerId(user.Id, false))
                .ReturnsAsync(new List<Model.Entity.Repository>());

            _repositoryManagerMock
                .Setup(m => m.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(user);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => new RepositoryDto
                {
                    Id = r.Id,
                    OwnerId = r.OwnerId,
                    Name = r.Name,
                    OwnedBy = r.OwnedBy
                });

            var response = await _repositoryService.GetAllRepositoriesForCurrentUser();

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Success, Is.True);

            var resultDtos = response.Result as List<RepositoryDto>;
            Assert.That(resultDtos, Is.Not.Null);
            Assert.That(resultDtos, Is.Empty);
        }

        [Test]
        public async Task GetAllRepositoriesForCurrentUser_OrganizationRepositories_ReturnsSuccessResponse()
        {
            var user = new User { UserName = "OrgUser", Id = 2 };
            var role = "USER";
            var organization = new Model.Entity.Organization { Name = "MyOrg1", Id = 10 };
            var organization2 = new Model.Entity.Organization { Name = "MyOrg2", Id = 11 };
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = organization.Id, Name = "OrgRepo1", OwnedBy = RepositoryOwnedBy.Organization },
                new() { Id = 2, OwnerId = organization2.Id, Name = "OrgRepo2", OwnedBy = RepositoryOwnedBy.Organization }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy
            }).ToList();

            _userContextServiceMock
                .Setup(m => m.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((user, role));

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetUserRepositoriesByOwnerId(user.Id, false))
                .ReturnsAsync(repositories);

            _repositoryManagerMock
                .Setup(m => m.OrganizationRepository.GetById(organization.Id))
                .ReturnsAsync(organization);

            _repositoryManagerMock
                .Setup(m => m.OrganizationRepository.GetById(organization2.Id))
                .ReturnsAsync(organization2);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositories[0].Id))
                .ReturnsAsync(repositories[0]);

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositories[1].Id))
                .ReturnsAsync(repositories[1]);

            var response = await _repositoryService.GetAllRepositoriesForCurrentUser();

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Name, Is.EqualTo("OrgRepo1"));
                Assert.That(resultDtos![1].Name, Is.EqualTo("OrgRepo2"));
                Assert.That(resultDtos![0].FullName, Is.EqualTo("MyOrg1-OrgRepo1/OrgRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("MyOrg2-OrgRepo2/OrgRepo2"));
            });
        }

        [Test]
        public async Task GetAllVisibleRepositoriesForUser_InvalidUsername_ReturnsErrorResponse()
        {
            var invalidUsername = "NonExistingUsername";

            var userResponseBase = new ResponseBase { Result = null, Success = false };

            _userServiceMock
                .Setup(us => us.GetUserByUsernameAsync(invalidUsername))
                .ReturnsAsync(userResponseBase);

            var response = await _repositoryService.GetAllVisibleRepositoriesForUser(invalidUsername);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.False);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Result, Is.Null);
            });
        }

        [Test]
        public async Task GetAllVisibleRepositoriesForUser_NotCurrentUserProfile_ReturnsSuccessResponse()
        {
            var user = new User { UserName = "User", Id = 2 };
            var userResponseBase = new ResponseBase { Result = user, Success = true };
            var currentUser = new User { UserName = "CurrentUser", Id = 3 };
            var role = "USER";
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = user.Id, Name = "PublicUserRepo1", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false },
                new() { Id = 2, OwnerId = user.Id, Name = "PublicUserRepo2", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy,
                IsPrivate = repo.IsPrivate
            }).ToList();

            _userServiceMock
                .Setup(us => us.GetUserByUsernameAsync(user.UserName))
                .ReturnsAsync(userResponseBase);

            _userContextServiceMock
                .Setup(m => m.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, role));

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetUserRepositoriesByOwnerId(user.Id, true))
                .ReturnsAsync(repositories);

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositories[0].Id))
                .ReturnsAsync(repositories[0]);

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositories[1].Id))
                .ReturnsAsync(repositories[1]);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(m => m.UserRepository.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            var response = await _repositoryService.GetAllVisibleRepositoriesForUser(user.UserName);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Name, Is.EqualTo("PublicUserRepo1"));
                Assert.That(resultDtos![1].Name, Is.EqualTo("PublicUserRepo2"));
                Assert.That(resultDtos![0].FullName, Is.EqualTo("User-PublicUserRepo1/PublicUserRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("User-PublicUserRepo2/PublicUserRepo2"));
                Assert.That(resultDtos, Is.All.Matches<RepositoryDto>(r => r.IsPrivate == false));
            });
        }

        [Test]
        public async Task GetAllRepositoriesForOrganization_ValidOrganizationName_ReturnsSuccessResponse()
        {
            var orgName = "Organization";
            var organization = new Model.Entity.Organization { Name = orgName, Id = 1 };
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = organization.Id, Name = "OrgRepo1", OwnedBy = RepositoryOwnedBy.Organization, IsPrivate = false },
                new() { Id = 2, OwnerId = organization.Id, Name = "OrgRepo2", OwnedBy = RepositoryOwnedBy.Organization, IsPrivate = false }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy
            }).ToList();

            _organizationServiceMock
                .Setup(o => o.GetOrganization(orgName))
                .ReturnsAsync(organization);

            _repositoryManagerMock
                .Setup(or => or.RepositoryRepository.GetOrganizationRepositoriesByOrganizationId(organization.Id))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            var response = await _repositoryService.GetAllRepositoriesForOrganization(orgName);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Name, Is.EqualTo("OrgRepo1"));
                Assert.That(resultDtos![1].Name, Is.EqualTo("OrgRepo2"));
                Assert.That(resultDtos, Is.All.Matches<RepositoryDto>(r => r.IsPrivate == false));
                Assert.That(resultDtos, Is.All.Matches<RepositoryDto>(r => r.OwnedBy == RepositoryOwnedBy.Organization));
            });
        }

        [Test]
        public async Task GetAllRepositoriesForOrganization_InvalidOrganizationName_ReturnsErrorResponse()
        {
            var orgName = "NonExistentOrg";

            _organizationServiceMock
                .Setup(o => o.GetOrganization(orgName))
                .ReturnsAsync((Model.Entity.Organization)null);

            var response = await _repositoryService.GetAllRepositoriesForOrganization(orgName);

            Assert.Multiple(() =>
            {
                Assert.That(response.Result, Is.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Organization not found"));
            });
        }

        [Test]
        public async Task GetAllRepositoriesForOrganization_NoRepositories_ReturnsSuccessResponse() //pada
        {
            var orgName = "Organization";
            var organization = new Model.Entity.Organization { Name = orgName, Id = 1 };

            _organizationServiceMock
                .Setup(o => o.GetOrganization(orgName))
                .ReturnsAsync(organization);

            _repositoryManagerMock
                .Setup(or => or.RepositoryRepository.GetOrganizationRepositoriesByOrganizationId(organization.Id))
                .ReturnsAsync(new List<Model.Entity.Repository>());

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => new RepositoryDto
                {
                    Id = r.Id,
                    OwnerId = r.OwnerId,
                    Name = r.Name,
                    OwnedBy = r.OwnedBy
                });

            var response = await _repositoryService.GetAllRepositoriesForOrganization(orgName);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Empty);
            });
        }

        [Test]
        public async Task DeleteRepository_RepositoryNotFound_ReturnsErrorResponse()
        {
            int repositoryId = 1;

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositoryId))
                .ReturnsAsync((Model.Entity.Repository)null);

            var response = await _repositoryService.DeleteRepository(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Repository not found"));
            });
        }

        [Test]
        public async Task DeleteRepository_UserWithoutPermission_ReturnsErrorResponse()
        {
            int repositoryId = 1;
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                OwnerId = 2,
                OwnedBy = RepositoryOwnedBy.Admin
            };

            var currentUser = new User { Id = 1, UserName = "test" };
            var owner = new User { Id = 2, UserName = "owner" };

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositoryId))
                .ReturnsAsync(repository);

            _userContextServiceMock
                .Setup(m => m.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "USER"));

            _repositoryManagerMock
                .Setup(o => o.UserRepository.GetByIdAsync(currentUser.Id))
                .ReturnsAsync(currentUser);

            _repositoryManagerMock
                .Setup(o => o.UserRepository.GetByIdAsync(owner.Id))
                .ReturnsAsync(owner);

            var response = await _repositoryService.DeleteRepository(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("You do not have permission to delete this repository"));
            });
        }

        [Test]
        public async Task DeleteRepository_UserWithPermission_ReturnsSuccessResponse()
        {
            int repositoryId = 1;
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.Admin,
                Name = "repo1"
            };

            var currentUser = new User { Id = 10, UserName = "adminUser" };

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.GetByIdAsync(repositoryId))
                .ReturnsAsync(repository);

            _userContextServiceMock
                .Setup(m => m.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "ADMIN"));

            _repositoryManagerMock
                .Setup(m => m.RepositoryRepository.DeleteAsync(repository.Id))
                .Returns(Task.CompletedTask);

            _harborServiceMock
                .Setup(m => m.deleteProject("adminUser-repo1", repository.Name))
                .ReturnsAsync(true);

            _repositoryManagerMock
                .Setup(o => o.UserRepository.GetByIdAsync(currentUser.Id))
                .ReturnsAsync(currentUser);


            var response = await _repositoryService.DeleteRepository(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.True);
                Assert.That(response.ErrorMessage, Is.Empty);
            });

            _harborServiceMock.Verify(
                m => m.deleteProject("adminUser-repo1", repository.Name), Times.Once);
        }

        [Test]
        public async Task EditRepository_UserAllowedToEdit_ReturnsUpdatedRepository()
        {
            const int repositoryId = 1;
            var editDto = new EditRepositoryDto(repositoryId, "Updated description", true);

            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "TestRepo",
                Description = "Old description",
                IsPrivate = false,
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.Admin
            };

            var currentUser = new User { Id = 10, UserName = "adminUser" };

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "ADMIN"));

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(repositoryId))
                .ReturnsAsync(repository);

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(repository.OwnerId))
                .ReturnsAsync(currentUser);

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllAsync())
                .ReturnsAsync(new List<Model.Entity.Repository> { repository });

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.UpdateAsync(It.IsAny<Model.Entity.Repository>()))
                .Returns(Task.CompletedTask);

            _harborServiceMock
                .Setup(h => h.UpdateProjectVisibility(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns<Model.Entity.Repository>(r => new RepositoryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsPrivate = r.IsPrivate,
                    OwnerId = r.OwnerId,
                    OwnedBy = r.OwnedBy
                });

            _repositoryManagerMock
                .Setup(m => m.UserRepository.GetByIdAsync(repository.OwnerId))
                .ReturnsAsync(currentUser);

            var response = await _repositoryService.EditRepository(editDto);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var updatedDto = response.Result as RepositoryDto;
                Assert.That(updatedDto, Is.Not.Null);
                Assert.That(updatedDto?.Description, Is.EqualTo(editDto.Description));
                Assert.That(updatedDto is { IsPrivate: true }, Is.EqualTo(editDto.IsPrivate));
            });

            _repositoryManagerMock.Verify(r => r.RepositoryRepository.UpdateAsync(It.Is<Model.Entity.Repository>(
                r => r.Description == editDto.Description && r.IsPrivate == editDto.IsPrivate)), Times.Once);

            _harborServiceMock.Verify(h => h.UpdateProjectVisibility(It.IsAny<string>(), false), Times.Once);
        }

        [Test]
        public async Task EditRepository_UserNotAllowedToEdit_ReturnsErrorResponse()
        {
            const int repositoryId = 1;
            var currentUser = new User { Id = 99, UserName = "notAllowedUser" };

            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "TestRepo",
                Description = "Old description",
                IsPrivate = false,
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.Admin
            };

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "USER"));

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(repositoryId))
                .ReturnsAsync(repository);

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(repository.OwnerId))
                .ReturnsAsync(new User { Id = repository.OwnerId, UserName = "adminUser" });

            var editDto = new EditRepositoryDto(repositoryId, "Updated description", true);

            var response = await _repositoryService.EditRepository(editDto);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Repository not found."));
            });
        }

        [Test]
        public async Task GetAllPublicRepositories_PublicReposExist_ReturnsSuccessResponse()
        {
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = 1, Name = "PublicRepo1", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false },
                new() { Id = 2, OwnerId = 2, Name = "PublicRepo2", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy,
                IsPrivate = repo.IsPrivate
            }).ToList();

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllPublicRepositories(It.IsAny<RepositorySearchDto>()))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User { Id = id, UserName = $"User{id}" });

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => repositories.FirstOrDefault(r => r.Id == id));

            var response = await _repositoryService.GetAllPublicRepositories(null);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Name, Is.EqualTo("PublicRepo1"));
                Assert.That(resultDtos![1].Name, Is.EqualTo("PublicRepo2"));
                Assert.That(resultDtos![0].FullName, Is.EqualTo("User1-PublicRepo1/PublicRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("User2-PublicRepo2/PublicRepo2"));
            });
        }

        [Test]
        public async Task GetAllPublicRepositories_NoPublicRepos_ReturnsEmptyList()
        {
            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllPublicRepositories(It.IsAny<RepositorySearchDto>()))
                .ReturnsAsync(new List<Model.Entity.Repository>());

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => new RepositoryDto());

            var response = await _repositoryService.GetAllPublicRepositories(null);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);
                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Is.Empty);
            });
        }

        [Test]
        public async Task GetRepository_PublicRepository_ReturnsSuccessResponse()
        {
            var repositoryId = 1;
            var collaborator = new User { Id = 1, UserName = "RepoCollaborator", Email = "collaborator@example.com" };
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "PublicRepo",
                IsPrivate = false,
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.User
            };
            var repositoryWithCollaborators = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "PublicRepo",
                IsPrivate = false,
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.User,
                Collaborators = new List<User> { collaborator },
            };

            var repoDto = new RepositoryDto
            {
                Id = repositoryId,
                Name = "PublicRepo",
                IsPrivate = false,
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.User
            };

            var artifacts = new List<HarborArtifact>
            {
                new()
                {
                    Id = 101,
                    RepositoryName = "user10/publicrepo",
                    Tags = new List<HarborTag> { new() { Name = "latest" } },
                    ExtraAttrs = new ExtraAttrs { Os = "linux" }
                }
            };
            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(repositoryId))
                .ReturnsAsync(repository);

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(repositoryWithCollaborators);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(repositoryWithCollaborators))
                .Returns(repoDto);

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(repository.OwnerId))
                .ReturnsAsync(new User { Id = 10, UserName = "User10" });

            _harborServiceMock
                .Setup(h => h.GetTags("User10-PublicRepo", repository.Name))
                .ReturnsAsync(artifacts);

            var response = await _repositoryService.GetRepository(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response.Success, Is.True);

                var result = response.Result as RepositoryDto;
                Assert.That(result, Is.Not.Null);

                Assert.That(result!.Id, Is.EqualTo(repositoryId));
                Assert.That(result.Name, Is.EqualTo("PublicRepo"));

                Assert.That(result.FullName, Is.EqualTo("User10-PublicRepo/PublicRepo"));
                Assert.That(result.OwnerUsername, Is.EqualTo("User10"));
            });
        }

        [Test]
        public async Task GetRepository_RepositoryNotFound_ReturnsErrorResponse()
        {
            var repositoryId = 999;

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync((Model.Entity.Repository?)null);

            var response = await _repositoryService.GetRepository(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.Result, Is.Null);
                Assert.That(response.ErrorMessage, Is.EqualTo($"Repository with id {repositoryId} does not exist."));
            });

            _repositoryManagerMock.Verify(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId), Times.Once);
            _harborServiceMock.Verify(h => h.GetTags(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetRepository_PrivateRepositoryWithoutPermission_ReturnsErrorResponse()
        {
            var repositoryId = 2;
            var collaborator = new User { Id = 1, UserName = "RepoCollaborator", Email = "collaborator@example.com" };
            var privateRepo = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "PrivateRepo",
                IsPrivate = true,
                OwnerId = 1,
                OwnedBy = RepositoryOwnedBy.User,
                Collaborators = new List<User> { collaborator },
            };

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(privateRepo);

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((new User(), string.Empty));

            var response = await _repositoryService.GetRepository(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.Result, Is.Null);
                Assert.That(response.ErrorMessage, Is.EqualTo($"Repository with id {repositoryId} does not exist."));
            });

            _repositoryManagerMock.Verify(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId), Times.Once);

            _userContextServiceMock.Verify(u => u.GetCurrentUserWithRoleAsync(), Times.Once);

            _repositoryMapperMock.Verify(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()), Times.Never);
            _harborServiceMock.Verify(h => h.GetTags(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetAllPublicRepositories_WithEmptyQuery_ReturnsSuccessResponse()
        {
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = 1, Name = "PublicRepo1", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy,
                IsPrivate = repo.IsPrivate
            }).ToList();

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllPublicRepositories(It.IsAny<RepositorySearchDto>()))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User { Id = id, UserName = $"User{id}" });

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => repositories.FirstOrDefault(r => r.Id == id));

            var response = await _repositoryService.GetAllPublicRepositories("");

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(1));
                Assert.That(resultDtos![0].Name, Is.EqualTo("PublicRepo1"));
                Assert.That(resultDtos![0].IsPrivate, Is.False);
            });
        }

        [Test]
        public async Task GetAllPublicRepositories_WithSpecificQuery_ReturnsFilteredRepositories()
        {
            var repoNameQuery = "FilteredRepo";
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = 1, Name = "FilteredRepo", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false, Description = "Test description" }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy,
                IsPrivate = repo.IsPrivate,
                Description = repo.Description
            }).ToList();

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllPublicRepositories(It.Is<RepositorySearchDto>(f => f.Name == repoNameQuery)))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User { Id = id, UserName = $"User{id}" });

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => repositories.FirstOrDefault(r => r.Id == id));

            var response = await _repositoryService.GetAllPublicRepositories($"name:{repoNameQuery}");

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(1));
                Assert.That(resultDtos![0].Name, Is.EqualTo(repoNameQuery));
                Assert.That(resultDtos![0].FullName, Is.EqualTo("User1-FilteredRepo/FilteredRepo"));
                Assert.That(resultDtos![0].IsPrivate, Is.False);
            });
        }

        [Test]
        public async Task GetAllPublicRepositories_NoRepositoriesFound_ReturnsEmptyList()
        {
            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllPublicRepositories(It.IsAny<RepositorySearchDto>()))
                .ReturnsAsync(new List<Model.Entity.Repository>());

            var response = await _repositoryService.GetAllPublicRepositories("name:NonExistentRepo");

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Is.Empty);
            });
        }

        [Test]
        public async Task GetAllPublicRepositories_WithComplexQuery_ReturnsSuccessResponse()
        {
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = 1, Name = "Repo", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false, Badge = Badge.SponsoredOSS }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy,
                IsPrivate = repo.IsPrivate,
                Badge = repo.Badge
            }).ToList();

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllPublicRepositories(It.Is<RepositorySearchDto>(f =>
                    f.Name == "Repo" && f.Badges.Contains(Badge.SponsoredOSS))))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User { Id = id, UserName = $"User{id}" });

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => repositories.FirstOrDefault(r => r.Id == id));

            var response = await _repositoryService.GetAllPublicRepositories("name:Repo badge:SponsoredOSS");

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(1));
                Assert.That(resultDtos![0].Name, Is.EqualTo("Repo"));
                Assert.That(resultDtos![0].Badge, Is.EqualTo(Badge.SponsoredOSS));
                Assert.That(resultDtos![0].IsPrivate, Is.False);
            });
        }

        [Test]
        public async Task GetAllPublicRepositories_WithAuthorQueryMatchingUser_ReturnsSuccessResponse()
        {
            var authorNameQuery = "TestAuthor";
            var author = new User { Id = 5, UserName = "TestAuthorUser" };
            var organization = new Model.Entity.Organization { Id = 10, Name = "TestOrganization" };
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = 5, Name = "AuthorRepo", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false },
                new() { Id = 2, OwnerId = 10, Name = "OrganizationRepo", OwnedBy = RepositoryOwnedBy.Organization, IsPrivate = false }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy,
                IsPrivate = repo.IsPrivate
            }).ToList();

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetUsersByUsernameContaining(authorNameQuery))
                .ReturnsAsync(new List<User> { author });

            _repositoryManagerMock
                .Setup(r => r.OrganizationRepository.GetOrganizationsByNameContaining(authorNameQuery))
                .ReturnsAsync(new List<Model.Entity.Organization>());

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllPublicRepositories(It.Is<RepositorySearchDto>(f =>
                    f.AuthorUserIds.Contains(author.Id))))
                .ReturnsAsync(repositories.Where(r => r.OwnerId == author.Id).ToList());

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
               .Setup(r => r.UserRepository.GetByIdAsync(author.Id))
               .ReturnsAsync(author);

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => repositories.FirstOrDefault(r => r.Id == id));

            var response = await _repositoryService.GetAllPublicRepositories("author:TestAuthor");

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(1));
                Assert.That(resultDtos![0].Name, Is.EqualTo("AuthorRepo"));
                Assert.That(resultDtos![0].FullName, Is.EqualTo("TestAuthorUser-AuthorRepo/AuthorRepo"));
            });
        }

        [Test]
        public async Task GetAllPublicRepositories_WithAuthorQueryMatchingUserAndOrganization_ReturnsSuccessResponse()
        {
            var authorNameQuery = "TestAuthor";
            var user = new User { Id = 5, UserName = "TestAuthorUser" };
            var organization = new Model.Entity.Organization { Id = 10, Name = "TestAuthorOrganization" };
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = 5, Name = "UserRepo", OwnedBy = RepositoryOwnedBy.User, IsPrivate = false },
                new() { Id = 2, OwnerId = 10, Name = "OrganizationRepo", OwnedBy = RepositoryOwnedBy.Organization, IsPrivate = false }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                OwnerId = repo.OwnerId,
                Name = repo.Name,
                OwnedBy = repo.OwnedBy,
                IsPrivate = repo.IsPrivate
            }).ToList();

            var filter = new RepositorySearchDto
            {
                AuthorName = authorNameQuery,
                AuthorUserIds = [user.Id],
                AuthorOrganizationIds = [organization.Id]
            };

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetUsersByUsernameContaining(authorNameQuery))
                .ReturnsAsync(new List<User> { user });

            _repositoryManagerMock
                .Setup(r => r.OrganizationRepository.GetOrganizationsByNameContaining(authorNameQuery))
                .ReturnsAsync(new List<Model.Entity.Organization> { organization });

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetAllPublicRepositories(It.Is<RepositorySearchDto>(f =>
                    f.AuthorUserIds.Contains(5))))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            _repositoryManagerMock
                .Setup(r => r.OrganizationRepository.GetById(organization.Id))
                .ReturnsAsync(organization);

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => repositories.FirstOrDefault(r => r.Id == id));

            var response = await _repositoryService.GetAllPublicRepositories($"author:{authorNameQuery}");

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(repositories.Count));
                Assert.That(resultDtos, Is.All.Matches<RepositoryDto>(r => r.IsPrivate == false));
                Assert.That(resultDtos, Is.All.Matches<RepositoryDto>(r => 
                (r.OwnedBy == RepositoryOwnedBy.User && filter.AuthorUserIds.Contains(r.OwnerId)) 
                || ( r.OwnedBy == RepositoryOwnedBy.Organization && filter.AuthorOrganizationIds.Contains(r.OwnerId))
                ));
            });
        }

        [Test]
        public async Task AddCollaborator_ValidRepositoryAndUser_ReturnsSuccessResponse()
        {
            var repositoryId = 1;
            var username = "collaborator";
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "TestRepo",
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.User,
                Collaborators = new List<User>()
            };
            var collaboratorUser = new User { Id = 5, UserName = username, HarborUserId = 1005 };
            var currentUser = new User { Id = 10, UserName = "owner" };

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(repository);

            _userServiceMock
                .Setup(u => u.GetUserByUsernameAsync(username))
                .ReturnsAsync(ResponseBase.SuccessResponse(collaboratorUser));

            _userManagerMock
                .Setup(u => u.GetRolesAsync(collaboratorUser))
                .ReturnsAsync(new List<string> { "USER" });

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "USER"));

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.UpdateAsync(It.IsAny<Model.Entity.Repository>()))
                .Returns(Task.CompletedTask);

            _harborServiceMock
                .Setup(h => h.addMember(It.IsAny<string>(), It.IsAny<HarborProjectMember>()))
                .ReturnsAsync(true);

            var response = await _repositoryService.AddCollaborator(repositoryId, username);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);
                Assert.That(repository.Collaborators, Contains.Item(collaboratorUser));
            });

            _repositoryManagerMock.Verify(r => r.RepositoryRepository.UpdateAsync(repository), Times.Once);
            _harborServiceMock.Verify(h => h.addMember(It.IsAny<string>(), It.IsAny<HarborProjectMember>()), Times.Once);
        }

        [Test]
        public async Task AddCollaborator_RepositoryNotFound_ReturnsErrorResponse()
        {
            var repositoryId = 999;
            var username = "collaborator";

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync((Model.Entity.Repository)null);

            var response = await _repositoryService.AddCollaborator(repositoryId, username);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Repository not found."));
            });
        }

        [Test]
        public async Task AddCollaborator_OrganizationRepository_ReturnsErrorResponse()
        {
            var repositoryId = 1;
            var username = "collaborator";
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "OrgRepo",
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.Organization,
                Collaborators = new List<User>()
            };

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(repository);

            var response = await _repositoryService.AddCollaborator(repositoryId, username);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Cannot add collaborators to organization repositories."));
            });
        }

        [Test]
        public async Task AddCollaborator_UserAlreadyExists_ReturnsErrorResponse()
        {
            var repositoryId = 1;
            var username = "collaborator";
            var collaboratorUser = new User { Id = 5, UserName = username };
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "TestRepo",
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.User,
                Collaborators = new List<User> { collaboratorUser }
            };
            var currentUser = new User { Id = 10, UserName = "owner" };

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(repository);

            _userServiceMock
                .Setup(u => u.GetUserByUsernameAsync(username))
                .ReturnsAsync(ResponseBase.SuccessResponse(collaboratorUser));

            _userManagerMock
                .Setup(u => u.GetRolesAsync(collaboratorUser))
                .ReturnsAsync(new List<string> { "USER" });

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "USER"));

            var response = await _repositoryService.AddCollaborator(repositoryId, username);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("User already added."));
            });
        }

        [Test]
        public async Task AddCollaborator_AddingOwnerAsCollaborator_ReturnsErrorResponse()
        {
            var repositoryId = 1;
            var username = "owner";
            var ownerUser = new User { Id = 10, UserName = username };
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "TestRepo",
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.User,
                Collaborators = new List<User>()
            };
            var currentUser = new User { Id = 10, UserName = "owner" };

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(repository);

            _userServiceMock
                .Setup(u => u.GetUserByUsernameAsync(username))
                .ReturnsAsync(ResponseBase.SuccessResponse(ownerUser));

            _userManagerMock
                .Setup(u => u.GetRolesAsync(ownerUser))
                .ReturnsAsync(new List<string> { "USER" });

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "USER"));
                
            var response = await _repositoryService.AddCollaborator(repositoryId, username);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Cannot add owner as collaborator."));
            });
        }

        [Test]
        public async Task GetCollaborators_UserRepository_ReturnsCollaborators()
        {
            var currentUser = new User { Id = 5, UserName = "admin" };
            var repositoryId = 1;
            var collaborator1 = new User { Id = 5, UserName = "collab1" };
            var collaborator2 = new User { Id = 6, UserName = "collab2" };
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "TestRepo",
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.User,
                Collaborators = new List<User> { collaborator1, collaborator2 }
            };

            var userDtos = new List<UserDto>
            {
                new() { Username = "collab1", Email = "collab1@test.com" },
                new() { Username = "collab2", Email = "collab2@test.com" }
            };

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(repository);

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "ADMIN"));

            _repositoryMapperMock
                .Setup(m => m.UserToUserDtoMapper.Map(It.IsAny<User>()))
                .Returns((User u) => userDtos.First(dto => dto.Username == u.UserName));

            var response = await _repositoryService.GetCollaborators(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<UserDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Username, Is.EqualTo("collab1"));
                Assert.That(resultDtos![1].Username, Is.EqualTo("collab2"));
            });
        }

        [Test]
        public async Task GetCollaborators_OrganizationRepository_ReturnsTeams()
        {
            var currentUser = new User { Id = 5, UserName = "collaborator" };
            var organizationOwnerUser = new User { Id = 6, UserName = "orgOwner" };
            var repositoryId = 1;
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "orgRepo",
                OwnerId = 10,
                OwnedBy = RepositoryOwnedBy.Organization
            };
            var organization = new Model.Entity.Organization { Id = 10, Name = "org" };

            var teams = new List<Model.Entity.Team>
            {
                new() { Id = 1, Name = "Team1", TeamRole = TeamRole.Admin },
                new() { Id = 2, Name = "Team2", TeamRole = TeamRole.ReadWrite, Users = new List<User> {currentUser } }
            };

            var teamDtos = new List<TeamDto>
            {
                new() { Name = "Team1", TeamRole = "Admin" },
                new() { Name = "Team2", TeamRole = "ReadWrite" }
            };

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(repository);

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "USER"));

            _repositoryManagerMock
                .Setup(r => r.OrganizationRepository.GetByIdAsync(repository.OwnerId))
                .ReturnsAsync(organization);

            _repositoryManagerMock
                .Setup(r => r.TeamRepository.GetAllByRepositoryIdAsync(repositoryId))
                .ReturnsAsync(teams);

            _repositoryMapperMock
                .Setup(m => m.TeamDtoToTeamMapper.ReverseMap(It.IsAny<Model.Entity.Team>()))
                .Returns((Model.Entity.Team t) => teamDtos.First(dto => dto.Name == t.Name));

            var response = await _repositoryService.GetCollaborators(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<TeamDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Name, Is.EqualTo("Team1"));
                Assert.That(resultDtos![1].Name, Is.EqualTo("Team2"));
            });
        }

        [Test]
        public async Task GetCollaborators_RepositoryNotFound_ReturnsErrorResponse()
        {
            var repositoryId = 999;

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync((Model.Entity.Repository)null);

            var response = await _repositoryService.GetCollaborators(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("Repository not found."));
            });
        }

        [Test]
        public async Task GetCollaborators_UserRepositoryWithNoCollaborators_ReturnsEmptyList()
        {
            var currentUser = new User { Id = 5, UserName = "owner" };
            var repositoryId = 1;
            var repository = new Model.Entity.Repository
            {
                Id = repositoryId,
                Name = "TestRepo",
                OwnerId = 5,
                OwnedBy = RepositoryOwnedBy.User,
                Collaborators = new List<User>()
            };

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId))
                .ReturnsAsync(repository);

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((currentUser, "USER"));

            _repositoryMapperMock
                .Setup(m => m.UserToUserDtoMapper.Map(It.IsAny<User>()))
                .Returns(new UserDto());

            var response = await _repositoryService.GetCollaborators(repositoryId);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<UserDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Is.Empty);
            });
        }

        [Test]
        public async Task GetRepositoriesUserContributed_ValidUser_ReturnsContributedRepositories()
        {
            var username = "contributor";
            var user = new User { Id = 5, UserName = username };
            var loggedInUser = new User { Id = 5, UserName = username };
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, Name = "ContribRepo1", OwnerId = 10, OwnedBy = RepositoryOwnedBy.User },
                new() { Id = 2, Name = "ContribRepo2", OwnerId = 11, OwnedBy = RepositoryOwnedBy.User }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                Name = repo.Name,
                OwnerId = repo.OwnerId,
                OwnedBy = repo.OwnedBy
            }).ToList();

            _userManagerMock
                .Setup(u => u.FindByNameAsync(username))
                .ReturnsAsync(user);

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((loggedInUser, "USER"));

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetContributedByUserIdAsync(user.Id, false))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User { Id = id, UserName = $"User{id}" });

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => repositories.FirstOrDefault(r => r.Id == id));

            var response = await _repositoryService.GetRepositoriesUserContributed(username);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(2));
                Assert.That(resultDtos![0].Name, Is.EqualTo("ContribRepo1"));
                Assert.That(resultDtos![1].Name, Is.EqualTo("ContribRepo2"));
            });
        }

        [Test]
        public async Task GetRepositoriesUserContributed_UserNotFound_ReturnsErrorResponse()
        {
            var username = "nonexistent";

            _userManagerMock
                .Setup(u => u.FindByNameAsync(username))
                .ReturnsAsync((User)null);

            var response = await _repositoryService.GetRepositoriesUserContributed(username);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.ErrorMessage, Is.EqualTo("User does not exist!"));
            });
        }

        [Test]
        public async Task GetRepositoriesUserContributed_AdminAccess_ReturnsContributedRepositories()
        {
            var username = "contributor";
            var user = new User { Id = 5, UserName = username };
            var loggedInUser = new User { Id = 10, UserName = "admin" };
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, Name = "ContribRepo1", OwnerId = 15, OwnedBy = RepositoryOwnedBy.User }
            };

            var repositoryDtos = repositories.Select(repo => new RepositoryDto
            {
                Id = repo.Id,
                Name = repo.Name,
                OwnerId = repo.OwnerId,
                OwnedBy = repo.OwnedBy
            }).ToList();

            _userManagerMock
                .Setup(u => u.FindByNameAsync(username))
                .ReturnsAsync(user);

            _userContextServiceMock
                .Setup(u => u.GetCurrentUserWithRoleAsync())
                .ReturnsAsync((loggedInUser, "ADMIN"));

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetContributedByUserIdAsync(user.Id, false))
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.RepositoryDtoToRepositoryMapper.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(r => r.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User { Id = id, UserName = $"User{id}" });

            _repositoryManagerMock
                .Setup(r => r.RepositoryRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => repositories.FirstOrDefault(r => r.Id == id));

            var response = await _repositoryService.GetRepositoriesUserContributed(username);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Not.Null);
                Assert.That(resultDtos, Has.Count.EqualTo(1));
                Assert.That(resultDtos![0].Name, Is.EqualTo("ContribRepo1"));
            });
        }

    }
}

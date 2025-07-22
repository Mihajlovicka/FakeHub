using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Moq;

namespace FakeHubApi.Tests.Repositories.Tests
{
    public class RepositoryServiceTests
    {
        private Mock<IBaseMapper<RepositoryDto, Model.Entity.Repository>> _repositoryMapperMock;
        private Mock<IOrganizationService> _organizationServiceMock;
        private Mock<IRepositoryManager> _repositoryManagerMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IUserService> _userServiceMock;
        private IRepositoryService _repositoryService;

        [SetUp]
        public void Setup()
        {
            _repositoryMapperMock = new Mock<IBaseMapper<RepositoryDto, Model.Entity.Repository>>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _userServiceMock = new Mock<IUserService>();

            _repositoryService = new RepositoryService(
                _repositoryMapperMock.Object,
                _organizationServiceMock.Object,
                _repositoryManagerMock.Object,
                _userContextServiceMock.Object,
                _userServiceMock.Object
            );
        }

        [Test]
        public async Task Save_RepositoryWithUniqueName_SuccessResponse()
        {
            var repositoryDto = new RepositoryDto { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };
            var repository = new Model.Entity.Repository { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };

            _repositoryMapperMock.Setup(m => m.Map(repositoryDto)).Returns(repository);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.That(response.Success, Is.True);
        }

        [Test]
        public async Task Save_RepositoryWithDuplicateName_ErrorResponse()
        {
            var repositoryDto = new RepositoryDto { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };
            var repository = new Model.Entity.Repository { OwnerId = 1, Name = "TestRepo", OwnedBy = RepositoryOwnedBy.User };

            _repositoryMapperMock.Setup(m => m.Map(repositoryDto)).Returns(repository);
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

            _repositoryMapperMock.Setup(m => m.Map(repositoryDto)).Returns(repository);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _organizationServiceMock.Setup(m => m.GetOrganizationById(It.IsAny<int>())).ReturnsAsync((Model.Entity.Organization)null);

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

            _repositoryMapperMock.Setup(m => m.Map(repositoryDto)).Returns(repository);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "USER"));
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.That(response.Success, Is.True);
            _repositoryMapperMock.Verify(m => m.Map(repositoryDto), Times.Once);
            _userContextServiceMock.Verify(m => m.GetCurrentUserWithRoleAsync(), Times.Once);
        }

        [Test]
        public async Task Save_WhenUserIsAdmin_ReturnsSuccessResponse()
        {
            var repositoryDto = new RepositoryDto { OwnerId = -2, Name = "AdminRepository", OwnedBy = RepositoryOwnedBy.Admin };
            var repository = new Model.Entity.Repository { OwnerId = -2, Name = "AdminRepository", OwnedBy = RepositoryOwnedBy.Admin };
            var currentUser = new User { Id = 1 };

            _repositoryMapperMock.Setup(m => m.Map(repositoryDto)).Returns(repository);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "ADMIN"));
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);

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
            var repositoryDto = new RepositoryDto { OwnerId = -2, Name = "SuperAdminRepository", OwnedBy = RepositoryOwnedBy.SuperAdmin };
            var repository = new Model.Entity.Repository { OwnerId = -2, Name = "SuperAdminRepository", OwnedBy = RepositoryOwnedBy.SuperAdmin };
            var currentUser = new User { Id = 11 };

            _repositoryMapperMock.Setup(m => m.Map(repositoryDto)).Returns(repository);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "SUPERADMIN"));
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);

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
            var repositoryDto = new RepositoryDto { OwnerId = 1, Name = "UserRepo", OwnedBy = RepositoryOwnedBy.User };
            var repository = new Model.Entity.Repository { OwnerId = 1, Name = "UserRepo", OwnedBy = RepositoryOwnedBy.User };
            var currentUser = new User { Id = 1, Badge = Badge.SponsoredOSS };

            _repositoryMapperMock.Setup(m => m.Map(repositoryDto)).Returns(repository);
            _userContextServiceMock.Setup(m => m.GetCurrentUserWithRoleAsync()).ReturnsAsync((currentUser, "USER"));
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);

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
                .Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

            _repositoryManagerMock
                .Setup(m => m.UserRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(user);

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
                Assert.That(resultDtos![0].FullName, Is.EqualTo("User/UserRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("User/UserRepo2"));
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
                .Setup(m => m.RepositoryRepository.GetAllAsync())
                .ReturnsAsync(repositories);

            _repositoryMapperMock
                .Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

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
                Assert.That(resultDtos![0].FullName, Is.EqualTo("AdminRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("AdminRepo2"));
            });
        }

        [Test]
        public async Task GetAllRepositoriesForCurrentUser_NoRepositories_ReturnsEmptyList()
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
            var organization = new Model.Entity.Organization { Name = "MyOrg", Id = 10 };
            var repositories = new List<Model.Entity.Repository>
            {
                new() { Id = 1, OwnerId = organization.Id, Name = "OrgRepo1", OwnedBy = RepositoryOwnedBy.Organization },
                new() { Id = 2, OwnerId = organization.Id, Name = "OrgRepo2", OwnedBy = RepositoryOwnedBy.Organization }
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
                .Setup(m => m.OrganizationRepository.GetByIdAsync(organization.Id))
                .ReturnsAsync(organization);

            _repositoryMapperMock
                .Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>()))
                .Returns((Model.Entity.Repository r) => repositoryDtos.First(d => d.Id == r.Id));

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
                Assert.That(resultDtos![0].FullName, Is.EqualTo("MyOrg/OrgRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("MyOrg/OrgRepo2"));
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

            _repositoryMapperMock
                .Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>()))
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
                Assert.That(resultDtos![0].FullName, Is.EqualTo("User/PublicUserRepo1"));
                Assert.That(resultDtos![1].FullName, Is.EqualTo("User/PublicUserRepo2"));
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
                .Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>()))
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
        public async Task GetAllRepositoriesForOrganization_NoRepositories_ReturnsSuccessResponse()
        {
            var orgName = "Organization";
            var organization = new Model.Entity.Organization { Name = orgName, Id = 1 };

            _organizationServiceMock
                .Setup(o => o.GetOrganization(orgName))
                .ReturnsAsync(organization);

            _repositoryManagerMock
                .Setup(or => or.RepositoryRepository.GetOrganizationRepositoriesByOrganizationId(organization.Id))
                .ReturnsAsync(new List<Model.Entity.Repository>());

            var response = await _repositoryService.GetAllRepositoriesForOrganization(orgName);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);

                var resultDtos = response.Result as List<RepositoryDto>;
                Assert.That(resultDtos, Is.Empty);
            });
        }
    }
}

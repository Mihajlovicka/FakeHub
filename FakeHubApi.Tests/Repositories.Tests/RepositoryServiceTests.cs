using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
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
        private IRepositoryService _repositoryService;

        [SetUp]
        public void Setup()
        {
            _repositoryMapperMock = new Mock<IBaseMapper<RepositoryDto, Model.Entity.Repository>>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _userContextServiceMock = new Mock<IUserContextService>();

            _repositoryService = new RepositoryService(
                _repositoryMapperMock.Object,
                _organizationServiceMock.Object,
                _repositoryManagerMock.Object,
                _userContextServiceMock.Object
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
            _userContextServiceMock.Setup(m => m.GetCurrentUserAsync()).ReturnsAsync(currentUser);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.GetByOwnerAndName(It.IsAny<RepositoryOwnedBy>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Model.Entity.Repository)null);
            _repositoryManagerMock.Setup(m => m.RepositoryRepository.AddAsync(It.IsAny<Model.Entity.Repository>())).Returns(Task.CompletedTask);
            _repositoryMapperMock.Setup(m => m.ReverseMap(It.IsAny<Model.Entity.Repository>())).Returns(repositoryDto);

            var response = await _repositoryService.Save(repositoryDto);

            Assert.That(response.Success, Is.True);
            _repositoryMapperMock.Verify(m => m.Map(repositoryDto), Times.Once);
            _userContextServiceMock.Verify(m => m.GetCurrentUserAsync(), Times.Once);
        }
    }
}

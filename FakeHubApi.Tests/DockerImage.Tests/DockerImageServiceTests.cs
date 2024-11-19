using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MySqlX.XDevAPI.Common;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Tests.DockerImage.Tests
{
    public class DockerImageServiceTests
    {
        private Mock<IDockerImageService> _dockerImageServiceMock;
        private IDockerImageService _dockerImageService;

        [SetUp]
        public void Setup()
        {
            _dockerImageServiceMock = new Mock<IDockerImageService>();
            _dockerImageService = new DockerImageService();
        }

        [Test]
        public async Task GetDockerImagesAsync_ReturnsListOfImages()
        {
            // Arrange:
            var mockImages = new List<DockerImageDto>
            {
                new DockerImageDto
                {
                    Title = "Image 1",
                    Description = "First Docker image",
                    LikesCount = 100,
                    DownloadsCount = 500,
                    LogoIcon = "logo1.png"
                },
                new DockerImageDto
                {
                    Title = "Image 2",
                    Description = "Second Docker image",
                    LikesCount = 200,
                    DownloadsCount = 700,
                    LogoIcon = "logo2.png"
                }
            };
            var successfullResponse = ResponseBase.SuccessResponse(mockImages);

            _dockerImageServiceMock
                .Setup(service => service.GetDockerImagesAsync())
                .ReturnsAsync(successfullResponse);

            // Act
            var result = await _dockerImageServiceMock.Object.GetDockerImagesAsync();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(((List<DockerImageDto>)result.Result).Count, Is.EqualTo(2));
                Assert.That(((List<DockerImageDto>)result.Result)[0].Title, Is.EqualTo("Image 1"));
                Assert.That(((List<DockerImageDto>)result.Result)[0].LikesCount, Is.EqualTo(100));
                Assert.That(((List<DockerImageDto>)result.Result)[1].LogoIcon, Is.EqualTo("logo2.png"));
            });
        }

        [Test]
        public async Task GetDockerImagesAsync_ReturnsEmptyList_WhenNoImagesAvailable()
        {
            // Arrange
            var mockImages = new List<DockerImageDto>();

            var successfullResponse = ResponseBase.SuccessResponse(mockImages);

            _dockerImageServiceMock
                .Setup(service => service.GetDockerImagesAsync())
                .ReturnsAsync(successfullResponse);

            // Act
            var result = await _dockerImageServiceMock.Object.GetDockerImagesAsync();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(((List<DockerImageDto>)result.Result).Count, Is.EqualTo(0));
            });
        }

        [Test]
        public async Task GetDockerImagesAsync_ThrowsException_WhenErrorOccurs()
        {
            var errorResponse = ResponseBase.ErrorResponse("An error occurred while getting Docker images");
            // Arrange
            _dockerImageServiceMock
                .Setup(service => service.GetDockerImagesAsync())
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _dockerImageServiceMock.Object.GetDockerImagesAsync();
            
            // Asserts
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.ErrorMessage, Is.EqualTo("An error occurred while getting Docker images"));
            });
        }
    }
}

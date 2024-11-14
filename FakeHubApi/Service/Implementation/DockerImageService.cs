using FakeHubApi.Mocks;
using FakeHubApi.Model.Dto;
using FakeHubApi.Service.Contract;

namespace FakeHubApi.Service.Implementation
{
    public class DockerImageService : IDockerImageService
    {
        public Task<List<DockerImageDto>> GetDockerImagesAsync()
        {
            var dockerImages = DockerImageMockData.GetMockDockerImages();

            return Task.FromResult(dockerImages);
        }
    }
}

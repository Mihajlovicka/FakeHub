using FakeHubApi.Model.Dto;

namespace FakeHubApi.Service.Contract
{
    public interface IDockerImageService
    {
        Task<List<DockerImageDto>> GetDockerImagesAsync();
    }
}

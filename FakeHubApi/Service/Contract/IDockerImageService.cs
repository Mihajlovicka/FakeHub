using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract
{
    public interface IDockerImageService
    {
        Task<ResponseBase> GetDockerImagesAsync();
    }
}

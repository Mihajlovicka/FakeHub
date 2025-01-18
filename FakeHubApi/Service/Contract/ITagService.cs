using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface ITagService
{
    Task<ResponseBase> CanDelete(int repositoryId);
    Task<ResponseBase> DeleteTag(ArtifactDto artifact, int repositoryId);
    Task<ResponseBase> GetTags(int repositoryId);
}

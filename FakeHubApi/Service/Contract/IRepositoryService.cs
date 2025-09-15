using FakeHubApi.ContainerRegistry;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface IRepositoryService
{
    Task<ResponseBase> Save(RepositoryDto repository);
    Task<ResponseBase> GetAllRepositoriesForCurrentUser();
    Task<ResponseBase> GetAllVisibleRepositoriesForUser(string username);
    Task<ResponseBase> GetAllRepositoriesForOrganization(string orgName);
    Task<ResponseBase> GetRepository(int repositoryId);
    Task<ResponseBase> DeleteRepository(int repositoryId);
    Task<ResponseBase> CanEditRepository(int repositoryId);
    Task<ResponseBase> EditRepository(EditRepositoryDto data);
    Task<(string, string)> GetFullProjectRepositoryName(int repositoryId);
    Task<ResponseBase> DeleteRepositoriesOfOrganization(Organization existingOrganization);
    List<ArtifactDto> MapHarborArtifactToArtifactDto(HarborArtifact source);
    Task<ResponseBase> Search(string? query);
}
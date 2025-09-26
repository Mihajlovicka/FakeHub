using System.Runtime.CompilerServices;
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
    Task<ResponseBase> Search(string? query);
    Task<ResponseBase> GetAllPublicRepositories(string? query);
    Task<ResponseBase> AddCollaborator(int repositoryId, string username);
    Task<ResponseBase> GetCollaborators(int repositoryId);
    Task<ResponseBase> GetRepositoriesUserContributed(string username);
}
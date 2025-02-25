using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface IRepositoryService
{
    Task<ResponseBase> Save(RepositoryDto repository);
    Task<ResponseBase> GetAllRepositoriesForCurrentUser();
    Task<ResponseBase> GetAllVisibleRepositoriesForUser(string username);
    Task<ResponseBase> GetAllRepositoriesForOrganization(string orgName);
}
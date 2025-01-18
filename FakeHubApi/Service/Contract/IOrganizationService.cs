using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface IOrganizationService
{
    Task<ResponseBase> Add(OrganizationDto model);
    Task<ResponseBase> Update(string name, UpdateOrganizationDto model);
    Task<ResponseBase> GetByName(string name);
    Task<ResponseBase> GetByUser();
    Task<ResponseBase> Search(string? query);
    Task<Organization?> GetOrganization(string name);
    Task<bool> IsLoggedInUserOwner(Organization organization);
    Task<ResponseBase> AddUser(string name, List<string> usernames);
    Task<ResponseBase> DeleteUser(string name, string username);
    Task<ResponseBase> DeactivateOrganization(string name);
    Task<ResponseBase> SearchUsersInOrganization(string name, string? query);
    Task<Organization?> GetOrganizationById(int id);
    Task<ResponseBase> GetByUserIdNamePair();
}

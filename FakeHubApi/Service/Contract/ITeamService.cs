using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface ITeamService
{
    Task<ResponseBase> Add(TeamDto model);
    Task<ResponseBase> Get(string organizationName, string teamName);
    Task<ResponseBase> Update(string organizationName, string teamName, UpdateTeamDto model);
    Task<ResponseBase> DeleteTeamFromOrganization(string organizationName, string teamName);
    Task<ResponseBase> AddUser(string organizationName, string teamName, List<string> usernames);
    Task<ResponseBase> DeleteUser(string organizationName, string teamName, string username);
}

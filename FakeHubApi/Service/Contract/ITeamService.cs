using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface ITeamService
{
    Task<ResponseBase> Add(TeamDto model);
    Task<ResponseBase> Get(string organizationName, string teamName);
    Task<ResponseBase> Update(string organizationName, string teamName, UpdateTeamDto model);
}

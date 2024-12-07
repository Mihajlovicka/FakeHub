using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface IOrganizationService
{
    Task<ResponseBase> Add(OrganizationDto model);
    Task<ResponseBase> Update(string name, UpdateOrganizationDto model);
    Task<ResponseBase> GetByName(string name);
    Task<ResponseBase> GetByUser();
    Task<ResponseBase> Search(string name);
}

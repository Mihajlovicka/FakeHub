using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface IOrganizationService
{
    Task<ResponseBase> Add(OrganizationDto model);
}

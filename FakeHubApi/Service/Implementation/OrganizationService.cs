using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;

namespace FakeHubApi.Service.Implementation;

public class OrganizationService(
    IMapperManager mapperManager,
    IRepositoryManager repositoryManager,
    IUserContextService userContext
) : IOrganizationService
{
    public async Task<ResponseBase> Add(OrganizationDto model)
    {
        var user = await userContext.GetCurrentUserAsync();

        Organization organization = mapperManager.OrganizationDtoToOrganizationMapper.Map(model);
        if (await repositoryManager.OrganizationRepository.GetByName(organization.Name) != null)
            return ResponseBase.ErrorResponse("Organization name is not unique.");

        organization.Owner = user;
        await repositoryManager.OrganizationRepository.AddAsync(organization);
        return ResponseBase.SuccessResponse();
    }
}

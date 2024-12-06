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

    public async Task<ResponseBase> Update(string name, UpdateOrganizationDto model)
    {
        var user = await userContext.GetCurrentUserAsync();

        var existingOrganization = await repositoryManager.OrganizationRepository.GetByName(name);
        if (existingOrganization == null)
            return ResponseBase.ErrorResponse("Organization not found.");

        if (existingOrganization.OwnerId != user.Id)
            return ResponseBase.ErrorResponse(
                "You are not authorized to update this organization."
            );

        existingOrganization.Description = model.Description;
        existingOrganization.ImageBase64 = model.ImageBase64;
        await repositoryManager.OrganizationRepository.UpdateAsync(existingOrganization);
        return ResponseBase.SuccessResponse();
    }

    public async Task<ResponseBase> GetByName(string name)
    {
        var organization = await repositoryManager.OrganizationRepository.GetByName(name);
        if (organization == null)
            return ResponseBase.ErrorResponse("Organization not found.");

        return ResponseBase.SuccessResponse(
            mapperManager.OrganizationDtoToOrganizationMapper.ReverseMap(organization)
        );
    }

    public async Task<ResponseBase> GetByUser()
    {
        var user = await userContext.GetCurrentUserAsync();
        var organizations = await repositoryManager.OrganizationRepository.GetByUser(user.Id);
        return ResponseBase.SuccessResponse(
            organizations.Select(mapperManager.OrganizationDtoToOrganizationMapper.ReverseMap)
        );
    }
}

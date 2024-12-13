using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Service.Implementation;

public class OrganizationService(
    UserManager<User> userManager,
    IMapperManager mapperManager,
    IRepositoryManager repositoryManager,
    IUserContextService userContext
) : IOrganizationService
{
    public async Task<ResponseBase> Add(OrganizationDto model)
    {
        var user = await userContext.GetCurrentUserAsync();

        Organization organization = mapperManager.OrganizationDtoToOrganizationMapper.Map(model);
        if (await GetOrganization(organization.Name) != null)
            return ResponseBase.ErrorResponse("Organization name is not unique.");

        organization.Owner = user;
        await repositoryManager.OrganizationRepository.AddAsync(organization);
        return ResponseBase.SuccessResponse();
    }

    public async Task<ResponseBase> Update(string name, UpdateOrganizationDto model)
    {
        var user = await userContext.GetCurrentUserAsync();

        var existingOrganization = await GetOrganization(name);
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
        var organization = await GetOrganization(name);
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

    public async Task<ResponseBase> Search(string name)
    {
        var user = await userContext.GetCurrentUserAsync();
        var organizations = await repositoryManager.OrganizationRepository.Search(name, user.Id);
        return ResponseBase.SuccessResponse(
            organizations.Select(mapperManager.OrganizationDtoToOrganizationMapper.ReverseMap)
        );
    }

    public async Task<Organization?> GetOrganization(string name)
    {
        return await repositoryManager.OrganizationRepository.GetByName(name);
    }

    public async Task<bool> IsLoggedInUserOwner(Organization organization)
    {
        var user = await userContext.GetCurrentUserAsync();
        return organization?.OwnerId == user.Id;
    }
    
    public async Task<ResponseBase> AddUser(string name, List<string> usernames)
    {
        if (usernames == null || usernames.Count == 0)
            return ResponseBase.ErrorResponse("No usernames provided");

        try
        {
            var organization = await repositoryManager.OrganizationRepository.GetByName(name);
            if (organization == null)
                return ResponseBase.ErrorResponse("Organization not found");

            var organizationUsernames = organization.Users.Select(x => x.UserName).ToHashSet();

            var usersToAdd = userManager.Users
                .Where(u => usernames.Contains(u.UserName) &&
                            !string.Equals(u.UserName, organization.Owner.UserName) &&
                            !organizationUsernames.Contains(u.UserName))
                .ToList();

            if (!usersToAdd.Any())
                return ResponseBase.ErrorResponse("No eligible users found");

            var responseUsers = new List<UserDto>();

            foreach (var user in usersToAdd)
            {
                organization.Users.Add(user);
                var responseUser = mapperManager.UserToUserDto.Map(
                user
            );
                responseUsers.Add(responseUser);
            }

            await repositoryManager.OrganizationRepository.UpdateAsync(organization);

            return ResponseBase.SuccessResponse(responseUsers);
        }
        catch (Exception ex)
        {
            return ResponseBase.ErrorResponse(ex.Message);
        }
    }
}

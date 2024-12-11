using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;

namespace FakeHubApi.Service.Implementation;

public class TeamService(
    IOrganizationService organizationService,
    IMapperManager mapperManager,
    IRepositoryManager repositoryManager,
    IUserContextService userContextService
) : ITeamService
{
    public async Task<ResponseBase> Add(TeamDto model)
    {
        var user = await userContextService.GetCurrentUserAsync();
        var team = mapperManager.TeamDtoToTeamMapper.Map(model);
        var organization = await organizationService.GetOrganization(model.OrganizationName);
        if (organization == null)
            return ResponseBase.ErrorResponse("Organization not found");
        if (organization.OwnerId != user.Id)
            return ResponseBase.ErrorResponse("You are not the owner of this organization.");
        if (organization.Teams.Any(x => x.Name == team.Name))
            return ResponseBase.ErrorResponse("Team name is not unique.");
        team.Organization = organization;
        await repositoryManager.TeamRepository.AddAsync(team);
        return ResponseBase.SuccessResponse();
    }
}

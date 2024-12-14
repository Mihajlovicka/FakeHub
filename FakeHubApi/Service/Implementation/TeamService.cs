using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
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
        var team = mapperManager.TeamDtoToTeamMapper.Map(model);
        var organization = await organizationService.GetOrganization(model.OrganizationName);

        var response = ResponseBase.SuccessResponse();
        var (success, errorMessage) = await ValidateNewTeam(model, organization);
        if (!success)
            response = ResponseBase.ErrorResponse(errorMessage);
        else
        {
            team.Organization = organization!;
            await repositoryManager.TeamRepository.AddAsync(team);
        }
        return response;
    }

    public async Task<ResponseBase> Get(string organizationName, string teamName)
    {
        var team = await repositoryManager.TeamRepository.GetTeam(organizationName, teamName);
        if (team == null)
            return ResponseBase.ErrorResponse("Team not found.");
        var teamDto = mapperManager.TeamDtoToTeamMapper.ReverseMap(team);
        teamDto.Owner = team.Organization.Owner.UserName!;
        return ResponseBase.SuccessResponse(teamDto);
    }

    private async Task<(bool, string)> ValidateNewTeam(TeamDto model, Organization? organization)
    {
        var response = (true, string.Empty);
        var user = await userContextService.GetCurrentUserAsync();
        if (organization == null)
            response = (false, "Organization not found");
        if (organization!.OwnerId != user.Id)
            response = (false, "You are not the owner of this organization.");
        if (organization.Teams.Any(x => x.Name == model.Name))
            response = (false, "Team name is not unique.");
        return response;
    }
}

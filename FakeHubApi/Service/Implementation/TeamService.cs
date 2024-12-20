using System.ComponentModel;
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

    public async Task<ResponseBase> Update(
        string organizationName,
        string teamName,
        UpdateTeamDto model
    )
    {
        var response = ResponseBase.SuccessResponse();
        var organization = await organizationService.GetOrganization(organizationName);
        var team = organization?.Teams.FirstOrDefault(x => x.Name == teamName);
        var (success, errorMessage) = await ValidateUpdateTeam(model, organization, team);
        if (!success)
            response = ResponseBase.ErrorResponse(errorMessage);
        else
        {
            team!.Name = model.Name;
            team!.Description = model.Description;
            await repositoryManager.TeamRepository.UpdateAsync(team);
        }
        return response;
    }

    public async Task<ResponseBase> DeleteTeamFromOrganization(string organizationName, string teamName)
    {
        var response = ResponseBase.SuccessResponse();
        var organization = await organizationService.GetOrganization(organizationName);
        var team = organization?.Teams.FirstOrDefault(x => x.Name == teamName);
        
        var (success, errorMessage) = await ValidateTeamFromOrganization(organization, team);
        if (!success)
            response = ResponseBase.ErrorResponse(errorMessage);
        else
        {
            organization!.Teams.Remove(team!);
            await repositoryManager.TeamRepository.UpdateAsync(team!);
        }
        return response;
    }

    private async Task<(bool, string)> ValidateTeamFromOrganization(Organization? organization, Team? team)
    {
        var response = (true, string.Empty);
        if (organization == null)
        {
            response = (false, "Organization not found.");
        }
        else if (!await organizationService.IsLoggedInUserOwner(organization))
            response = (false, "You are not the owner of this organization.");
        else if (team == null)
        {
            response = (false, "Team not found in organization.");
        }

        return response;
    }

    private async Task<(bool, string)> ValidateNewTeam(TeamDto model, Organization? organization)
    {
        var response = (true, string.Empty);
        if (organization == null)
            response = (false, "Organization not found");
        else if (!await organizationService.IsLoggedInUserOwner(organization))
            response = (false, "You are not the owner of this organization.");
        else if (!IsTeamNameUnique(organization, model.Name))
            response = (false, "Team name is not unique.");
        return response;
    }

    private async Task<(bool, string)> ValidateUpdateTeam(
        UpdateTeamDto model,
        Organization? organization,
        Team? team
    )
    {
        var response = (true, string.Empty);
        if (organization == null)
            response = (false, "Organization not found");
        else if (!await organizationService.IsLoggedInUserOwner(organization))
            response = (false, "You are not the owner of this organization.");
        else
        {
            if (team == null)
                response = (false, "Team not found.");
            else if (team.Name != model.Name && !IsTeamNameUnique(organization, model.Name))
                response = (false, "Team name is not unique.");
        }
        return response;
    }

    private bool IsTeamNameUnique(Organization organization, string teamName)
    {
        return organization.Teams.All(x => x.Name != teamName);
    }
}

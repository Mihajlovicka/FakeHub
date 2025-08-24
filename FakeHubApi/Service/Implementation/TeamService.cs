using FakeHubApi.ContainerRegistry;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using Org.BouncyCastle.Bcpg;

namespace FakeHubApi.Service.Implementation;

public class TeamService(
    IOrganizationService organizationService,
    IMapperManager mapperManager,
    IRepositoryManager repositoryManager,
    IUserService userService,
    IHarborService harborService
) : ITeamService
{
    public async Task<ResponseBase> Add(TeamDto model)
    {
        var team = mapperManager.TeamDtoToTeamMapper.Map(model);
        var organization = await organizationService.GetOrganization(model.OrganizationName);
        var repository = await repositoryManager.RepositoryRepository.GetByIdAsync((int)model.Repository.Id);

        var response = ResponseBase.SuccessResponse();
        var (success, errorMessage) = await ValidateNewTeam(model, organization, repository);
        if (!success)
            response = ResponseBase.ErrorResponse(errorMessage);
        else
        {
            team.Organization = organization!;
            team.Repository = repository!;
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
        teamDto.Users = team.Users.Select(mapperManager.UserToUserDtoMapper.Map);
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

    public async Task<ResponseBase> DeleteTeamFromOrganization(
        string organizationName,
        string teamName
    )
    {
        var response = ResponseBase.SuccessResponse();
        var organization = await organizationService.GetOrganization(organizationName);
        var team = organization?.Teams.FirstOrDefault(x => x.Name == teamName);

        var (success, errorMessage) = await ValidateTeamFromOrganization(organization, team);
        if (!success)
            response = ResponseBase.ErrorResponse(errorMessage);
        else
        {
            await harborService.removeMembersByRole($"{organizationName}-{team!.Repository.Name}", mapTeamRoleToHarborRole(team!.TeamRole));
            organization!.Teams.Remove(team!);
            await repositoryManager.TeamRepository.UpdateAsync(team!);
        }
        return response;
    }

    public async Task<ResponseBase> AddUser(
        string organizationName,
        string teamName,
        List<string> usernames
    )
    {
        var response = ResponseBase.SuccessResponse();
        var team = await repositoryManager.TeamRepository.GetTeam(organizationName, teamName);
        var (success, errorMessage) = ValidateTeamExists(team);
        if (!success)
            response = ResponseBase.ErrorResponse(errorMessage);
        else
        {
            var foundUsers = userService.GetUsers(usernames);
            var addedUsers = new List<User>();
            (success, errorMessage) = await ValidateAddMemberToTeam(team!, foundUsers);
            if (!success)
                response = ResponseBase.ErrorResponse(errorMessage);
            else
            {
                foreach (var user in foundUsers)
                {
                    if (
                        team!.Organization.Owner.UserName == user.UserName
                        || !team.Organization.Users.Any(x => x.UserName == user.UserName)
                    )
                        continue;
                    team!.Users.Add(user);
                    addedUsers.Add(user);

                    var harborProjectMember = new HarborProjectMember
                    {
                        MemberUser = new HarborProjectMemberUser
                        {
                            UserId = user.HarborUserId,
                            Username = user.UserName
                        },
                        RoleId = mapTeamRoleToHarborRole(team!.TeamRole)
                    };
                    await harborService.addMember($"{organizationName}-{team.Repository.Name}", harborProjectMember);
                }
                await repositoryManager.TeamRepository.UpdateAsync(team!);
                response.Result = addedUsers.Select(mapperManager.UserToUserDtoMapper.Map);


            }
        }
        return response;
    }


    public async Task<ResponseBase> DeleteUser(
        string organizationName,
        string teamName,
        string username)
    {
        try
        {
            var userResponse = await userService.GetUserProfileByUsernameAsync(username);

            if (!userResponse.Success)
                return ResponseBase.ErrorResponse(userResponse.ErrorMessage);

            var user = userResponse.Result as UserDto;

            if (user == null)
                return ResponseBase.ErrorResponse("User not found");

            var team = await repositoryManager.TeamRepository.GetTeam(organizationName, teamName);

            if (team == null)
                return ResponseBase.ErrorResponse("Team not in organization");

            if (!await organizationService.IsLoggedInUserOwner(team.Organization))
                return ResponseBase.ErrorResponse("You are not the owner of this organization");
                
            var deleteRelation = team.Users.FirstOrDefault(u =>
                u.UserName == user.Username
            );

            if (deleteRelation == null)
                return ResponseBase.ErrorResponse("User is not member of team");

            await harborService.removeMemberFromTeam($"{organizationName}-{team.Repository.Name}", username);

            team.Users.Remove(deleteRelation);

            await repositoryManager.TeamRepository.UpdateAsync(team);

            return ResponseBase.SuccessResponse(user);
        }
        catch (Exception ex)
        {
            return ResponseBase.ErrorResponse(ex.Message);
        }
    }

    private async Task<(bool, string)> ValidateTeamFromOrganization(
        Organization? organization,
        Team? team
    )
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

    private async Task<(bool, string)> ValidateAddMemberToTeam(Team team, List<User> users)
    {
        var response = (true, string.Empty);
        if (users.Count == 0)
            response = (false, "No users found.");
        else if (!await organizationService.IsLoggedInUserOwner(team.Organization))
            response = (false, "You are not the owner of this organization.");
        return response;
    }

    private (bool, string) ValidateTeamExists(Team? team)
    {
        var response = (true, string.Empty);
        if (team == null)
            response = (false, "Team not found.");
        return response;
    }

    private async Task<(bool, string)> ValidateNewTeam(TeamDto model, Organization? organization, Model.Entity.Repository? repository)
    {
        var response = (true, string.Empty);
        if (organization == null)
            response = (false, "Organization not found");
        else if (!await organizationService.IsLoggedInUserOwner(organization))
            response = (false, "You are not the owner of this organization.");
        else if (!IsTeamNameUnique(organization, model.Name))
            response = (false, "Team name is not unique.");
        else if (!IsTeamRoleUniqueInsideRepository(organization, repository, model.TeamRole))
            response = (false, "Team role is not unique within the repository.");
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

    private bool IsTeamRoleUniqueInsideRepository(Organization organization, Model.Entity.Repository repository, string teamRole)
    {
        return organization.Teams
            .Where(x => x.RepositoryId == repository.Id)
            .All(x => x.TeamRole.ToString() != teamRole);
    }

    private int mapTeamRoleToHarborRole(TeamRole teamRole)
    {
        return teamRole switch
        {
            TeamRole.Admin => (int)HarborRoles.Admin,
            TeamRole.ReadOnly => (int)HarborRoles.Guest,
            TeamRole.ReadWrite => (int)HarborRoles.Developer,
            _ => (int)HarborRoles.Guest
        };
    }
}

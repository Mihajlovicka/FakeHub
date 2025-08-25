using FakeHubApi.ContainerRegistry;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;

namespace FakeHubApi.Service.Implementation;

public class RepositoryService(
    IMapperManager mapperManager,
    IOrganizationService organizationService,
    IRepositoryManager repositoryManager,
    IUserContextService userContextService,
    IUserService userService,
    IHarborService harborService
) : IRepositoryService
{
    public async Task<ResponseBase> Save(RepositoryDto repositoryDto)
    {
        var repository = mapperManager.RepositoryDtoToRepositoryMapper.Map(repositoryDto);
        var (currentUser, currentUserRole) = await userContextService.GetCurrentUserWithRoleAsync();

        if(currentUserRole is "ADMIN" or "SUPERADMIN")
        {
            repository.Badge = Badge.DockerOfficialImage;
            repository.OwnerId = currentUser.Id;
            repository.OwnedBy = currentUserRole == "ADMIN" ? RepositoryOwnedBy.Admin : RepositoryOwnedBy.SuperAdmin;
        } 
        else if(currentUserRole == "USER" && currentUser.Badge != Badge.None)
        {
            repository.Badge = currentUser.Badge;
        }

        if (repositoryDto.OwnerId == -1)
        {
            repository.OwnerId = currentUser.Id;
        }

        var (success, errorMessage) = await ValidateRepository(repository);
        if (!success) return ResponseBase.ErrorResponse(errorMessage);
        else
        {
            await repositoryManager.RepositoryRepository.AddAsync(repository);
        }

        var projectName = repositoryDto.OwnerId == -1 ? currentUser.UserName : (await organizationService.GetOrganizationById(repositoryDto.OwnerId)).Name;
        projectName += "-" + repository.Name;

        await harborService.createUpdateProject(new HarborProjectCreate { ProjectName = projectName, Public = !repositoryDto.IsPrivate });
        await harborService.addMember(projectName, new HarborProjectMember { RoleId = (int)HarborRoles.Admin, MemberUser = new HarborProjectMemberUser { UserId = currentUser.HarborUserId, Username = currentUser.UserName } });

        return ResponseBase.SuccessResponse(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap(repository));
    }

    private async Task<(bool, string)> ValidateRepository(Model.Entity.Repository repositoryDto)
    {
        var response = (true, string.Empty);
        if (!(await RepositoryNameUniqueForOwner(repositoryDto.OwnedBy, repositoryDto.OwnerId, repositoryDto.Name))) return (false, "Name must be unique");
        if (repositoryDto.OwnedBy != RepositoryOwnedBy.Organization) return response;
        return (await organizationService.GetOrganizationById(repositoryDto.OwnerId)) == null ? (false, "Organization does not exist") : response;
    }

    private async Task<bool> RepositoryNameUniqueForOwner(RepositoryOwnedBy ownedBy, int ownerId, string repositoryName)
    {
        var repository = await repositoryManager.RepositoryRepository.GetByOwnerAndName(ownedBy, ownerId, repositoryName);

        return repository == null;
    }

    public async Task<ResponseBase> GetAllRepositoriesForCurrentUser()
    {
        var (user, role) = await userContextService.GetCurrentUserWithRoleAsync();

        var repositories = await GetRepositoriesByRole(user.Id, role);

        var repositoryDtos = repositories.Select(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap).ToList();

        var updatedDtos = await GetFullNames(repositoryDtos);

        return ResponseBase.SuccessResponse(updatedDtos);
    }

    public async Task<ResponseBase> GetAllVisibleRepositoriesForUser(string username)
    {
        var userResponse = await userService.GetUserByUsernameAsync(username);
        if (!userResponse.Success)
            return ResponseBase.ErrorResponse(userResponse.ErrorMessage);

        var user = userResponse.Result as User;

        var (loggedInUser, role) = await userContextService.GetCurrentUserWithRoleAsync();

        var showOnlyPublic = role == "USER" && user?.Id != loggedInUser.Id;

        if (user == null)
        {
            return ResponseBase.ErrorResponse("User does not exist!");
        }

        var filteredRepositories = await repositoryManager.RepositoryRepository.GetUserRepositoriesByOwnerId(user.Id, showOnlyPublic);

        var repositoryDtos = filteredRepositories.Select(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap).ToList();

        var updatedDtos = await GetFullNames(repositoryDtos);

        return ResponseBase.SuccessResponse(updatedDtos);
    }

    public async Task<ResponseBase> GetAllRepositoriesForOrganization(string orgName)
    {
        var organization = await organizationService.GetOrganization(orgName);

        if (organization == null)
        {
            return ResponseBase.ErrorResponse("Organization not found");
        }

        var orgRepositories = await repositoryManager.RepositoryRepository.GetOrganizationRepositoriesByOrganizationId(organization.Id);

        var repositoryDtos = orgRepositories.Select(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap).ToList();

        return ResponseBase.SuccessResponse(repositoryDtos);

    }

    public async Task<ResponseBase> GetRepository(int repositoryId)
    {
        var repository =  await GetRepositoryForCurrentUser(repositoryId);
        if (repository == null)
            return ResponseBase.ErrorResponse($"Repository with id {repositoryId} does not exist.");

        var repoDto = await MapModelToDto(repository);
        var projectName = await GetRepoOwnerUsername(repoDto) + "-" + repository.Name;
        List<HarborArtifact> artifacts = await harborService.GetTags(projectName, repository.Name);
        repoDto.Artifacts = artifacts.Select(mapperManager.HarborArtifactToArtifactDtoMapper.Map).ToList();
        return ResponseBase.SuccessResponse(repoDto);
    }

    private async Task<RepositoryDto> MapModelToDto(Model.Entity.Repository repository)
    {
        var repositoryDto = mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap(repository);
        repositoryDto.FullName = await GetFullName(repositoryDto);
        repositoryDto.OwnerUsername = await GetRepoOwnerUsername(repositoryDto) ?? "";

        return repositoryDto;
    }
    private async Task<IEnumerable<Model.Entity.Repository>> GetRepositoriesByRole(int userId, string role)
    {
        return role == "USER"
            ? await repositoryManager.RepositoryRepository.GetUserRepositoriesByOwnerId(userId)
            : await repositoryManager.RepositoryRepository.GetAllAsync();
    }

    private async Task<List<RepositoryDto>> GetFullNames(List<RepositoryDto> repositoryDtos)
    {
        var updatedDtos = new List<RepositoryDto>();

        foreach (var repositoryDto in repositoryDtos)
        {
            repositoryDto.FullName = await GetFullName(repositoryDto);

            updatedDtos.Add(repositoryDto);
        }

        return updatedDtos;
    }

    private async Task<string> GetFullName(RepositoryDto repositoryDto)
    {
        if (repositoryDto.OwnedBy is not RepositoryOwnedBy.Organization and not RepositoryOwnedBy.User)
            return repositoryDto.Name;

        var defaultUsername = repositoryDto.OwnedBy == RepositoryOwnedBy.Organization ? "UnknownOrg" : "UnknownUser";
        var username = await GetRepoOwnerUsername(repositoryDto) ?? defaultUsername;
        return $"{username}/{repositoryDto.Name}";
    }
    
    private async Task<string?> GetRepoOwnerUsername(RepositoryDto repositoryDto)
    {
        string? fullName;
        switch (repositoryDto.OwnedBy)
        {
            case RepositoryOwnedBy.Organization:
                fullName = (await repositoryManager.OrganizationRepository.GetByIdAsync(repositoryDto.OwnerId))?.Name;
                break;
            case RepositoryOwnedBy.User:
            case RepositoryOwnedBy.Admin:
            case RepositoryOwnedBy.SuperAdmin:
                fullName = (await repositoryManager.UserRepository.GetByIdAsync(repositoryDto.OwnerId))?.UserName;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return fullName;
    }

    private async Task<Model.Entity.Repository?> GetRepositoryForCurrentUser(int repositoryId)
    {
        var (user, role) = await userContextService.GetCurrentUserWithRoleAsync();

        var repositories = await GetRepositoriesByRole(user.Id, role);

        return repositories.FirstOrDefault(x => x.Id == repositoryId);
    }
}
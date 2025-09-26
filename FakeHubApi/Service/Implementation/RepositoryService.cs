using FakeHubApi.ContainerRegistry;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Redis;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Service.Implementation;

public class RepositoryService(
    IMapperManager mapperManager,
    IOrganizationService organizationService,
    IRepositoryManager repositoryManager,
    IUserContextService userContextService,
    IUserService userService,
    IHarborService harborService,
    IRedisCacheService _cacheService,
    UserManager<User> userManager
) : IRepositoryService
{
    public async Task<ResponseBase> Save(RepositoryDto repositoryDto)
    {
        var repository = mapperManager.RepositoryDtoToRepositoryMapper.Map(repositoryDto);
        var (currentUser, currentUserRole) = await userContextService.GetCurrentUserWithRoleAsync();

        switch (currentUserRole)
        {
            case "ADMIN" or "SUPERADMIN":
                repository.Badge = Badge.DockerOfficialImage;
                repository.OwnerId = currentUser.Id;
                repository.OwnedBy = currentUserRole == "ADMIN" ? RepositoryOwnedBy.Admin : RepositoryOwnedBy.SuperAdmin;
                break;
            case "USER" when currentUser.Badge != Badge.None:
                repository.Badge = currentUser.Badge;
                break;
        }

        if (repositoryDto.OwnerId == -1)
        {
            repository.OwnerId = currentUser.Id;
            await _cacheService.RemoveCacheValueAsync($"RepositoriesByUser:{currentUser.UserName}");
        }
        else
        {
            await RemoveCacheRepositoriesByOrganization(repository);
        }

        var (success, errorMessage) = await ValidateRepository(repository);
        if (!success) return ResponseBase.ErrorResponse(errorMessage);
        else
        {
            await repositoryManager.RepositoryRepository.AddAsync(repository);
        }

        var projectName = await GetHarborProjectName(repository);

        await harborService.createUpdateProject(new HarborProjectCreate { ProjectName = projectName, Public = !repositoryDto.IsPrivate });

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

        var cacheKey = $"RepositoriesByUser:{user.UserName}";
        var cachedRepository = await _cacheService.GetCacheValueAsync<List<RepositoryDto>>(cacheKey);
        if (cachedRepository != null && cachedRepository.Count > 0)
        {
            return ResponseBase.SuccessResponse(cachedRepository);
        }

        var repositories = await GetRepositoriesByRole(user.Id, role);

        var repositoryDtos = repositories.Select(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap).ToList();

        var updatedDtos = await GetFullNames(repositoryDtos);

        await _cacheService.SetCacheValueAsync(cacheKey, updatedDtos);

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
        var cacheKey = $"RepositoriesByOrganization:{orgName}";
        var cachedRepository = await _cacheService.GetCacheValueAsync<List<RepositoryDto>>(cacheKey);
        if (cachedRepository != null)
        {
            return ResponseBase.SuccessResponse(cachedRepository);
        }

        var organization = await organizationService.GetOrganization(orgName);

        if (organization == null)
        {
            return ResponseBase.ErrorResponse("Organization not found");
        }

        var orgRepositories = await repositoryManager.RepositoryRepository.GetOrganizationRepositoriesByOrganizationId(organization.Id);

        var repositoryDtos = orgRepositories.Select(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap).ToList();

        await _cacheService.SetCacheValueAsync(cacheKey, repositoryDtos);

        return ResponseBase.SuccessResponse(repositoryDtos);

    }

    public async Task<(string, string)> GetFullProjectRepositoryName(int repositoryId)
    {
        var repository = await repositoryManager.RepositoryRepository.GetByIdAsync(repositoryId);
        if (repository == null)
            throw new Exception("Repository not found");

        var projectName = await GetHarborProjectName(repository);
        return (projectName, repository.Name);
    }

    public async Task<ResponseBase> GetRepository(int repositoryId)
    {
        var cacheKey = $"Repository:{repositoryId}";
        var cachedRepository = await _cacheService.GetCacheValueAsync<RepositoryDto>(cacheKey);
        if (cachedRepository is { IsPrivate: false })
        {
            return ResponseBase.SuccessResponse(cachedRepository);
        }

        var repository = await repositoryManager.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId);

        if (repository == null
            || !(await CanCurrentUserAccessRepository(repository)))
            return ResponseBase.ErrorResponse($"Repository with id {repositoryId} does not exist.");

        if (cachedRepository != null)
        {
            return ResponseBase.SuccessResponse(cachedRepository);
        }

        var repoDto = await MapModelToDto(repository);
        await _cacheService.SetCacheValueAsync(cacheKey, repoDto);

        return ResponseBase.SuccessResponse(repoDto);
    }

    public async Task<ResponseBase> DeleteRepository(int repositoryId)
    {
        var repository = await repositoryManager.RepositoryRepository.GetByIdAsync(repositoryId);
        var (currentUser, role) = await userContextService.GetCurrentUserWithRoleAsync();

        if (repository == null)
            return ResponseBase.ErrorResponse("Repository not found");

        if (!(role is "ADMIN" or "SUPERADMIN") && (await GetAdminUsersInRepository(repository)).All(el => el.UserName != currentUser.UserName))
            return ResponseBase.ErrorResponse("You do not have permission to delete this repository");

        await repositoryManager.RepositoryRepository.DeleteAsync(repositoryId);

        await harborService.deleteProject(await GetHarborProjectName(repository), repository.Name);

        await _cacheService.RemoveCacheValueAsync($"Repository:{repositoryId}");
        await _cacheService.RemoveCacheValueAsync($"RepositoriesByUser:{currentUser.UserName}");
        await RemoveCacheRepositoriesByOrganization(repository);

        return ResponseBase.SuccessResponse();
    }

    public async Task<ResponseBase> CanEditRepository(int repositoryId)
    {
        var (isAllowed, _) = await IsEditAllowed(repositoryId);

        return ResponseBase.SuccessResponse(isAllowed);
    }

    public async Task<ResponseBase> GetAllPublicRepositories(string? query)
    {
        var filters = await ParseQuery(query);

        var publicRepos = await repositoryManager.RepositoryRepository.GetAllPublicRepositories(filters);

        var repositoryDtos = publicRepos?.Any() == true ?
            publicRepos.Select(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap).ToList() :
            new List<RepositoryDto>();

        var updatedDtos = await GetFullNames(repositoryDtos);

        return ResponseBase.SuccessResponse(updatedDtos);
    }

    private async Task<List<User>> GetAdminUsersInRepository(Model.Entity.Repository repository)
    {
        var users = new List<User>();
        if (repository.OwnedBy == RepositoryOwnedBy.Organization)
        {
            var organization = await repositoryManager.OrganizationRepository.GetById(repository.OwnerId);
            users.Add(organization!.Owner);
            var firstAdminTeam = organization.Teams.FirstOrDefault(el => el.TeamRole == TeamRole.Admin && el.RepositoryId == repository.Id);
            if (firstAdminTeam != null && firstAdminTeam.Users.Count > 0)
            {
                users.AddRange(firstAdminTeam.Users);
            }
        }
        else
        {
            var user = await repositoryManager.UserRepository.GetByIdAsync(repository.OwnerId);
            if (user != null) users.Add(user);
        }
        return users;
    }

    public async Task<ResponseBase> EditRepository(EditRepositoryDto data)
    {
        var (isEditAllowed, repository) = await IsEditAllowed(data.Id);

        if (!isEditAllowed || repository is null)
        {
            return ResponseBase.ErrorResponse("Repository not found.");
        }

        repository.Description = data.Description;
        repository.IsPrivate = data.IsPrivate;

        await repositoryManager.RepositoryRepository.UpdateAsync(repository);

        var updated = await harborService.UpdateProjectVisibility((await GetHarborProjectName(repository)), !repository.IsPrivate);
        if (!updated)
        {
            return ResponseBase.ErrorResponse("Harbor server error.");
        }

        var updatedDto = await MapModelToDto(repository);

        var (currentUser, _) = await userContextService.GetCurrentUserWithRoleAsync();
        await _cacheService.RemoveCacheValueAsync($"Repository:{data.Id}");
        await _cacheService.RemoveCacheValueAsync($"RepositoriesByUser:{currentUser.UserName}");
        await RemoveCacheRepositoriesByOrganization(repository);

        return ResponseBase.SuccessResponse(updatedDto);
    }

    public async Task<ResponseBase> DeleteRepositoriesOfOrganization(Organization existingOrganization)
    {
        var orgRepositories = await repositoryManager.RepositoryRepository.GetOrganizationRepositoriesByOrganizationId(existingOrganization.Id);
        foreach (var repo in orgRepositories)
        {
            await repositoryManager.RepositoryRepository.DeleteAsync(repo.Id);
            await harborService.deleteProject(await GetHarborProjectName(repo), repo.Name);
            await RemoveCacheRepositoriesByOrganization(repo);
        }
        return ResponseBase.SuccessResponse();
    }

    public async Task<ResponseBase> AddCollaborator(int repositoryId, string username)
    {
        var repo = await repositoryManager.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId);
        if (repo == null)
        {
            return ResponseBase.ErrorResponse("Repository not found.");
        }

        if (repo.OwnedBy == RepositoryOwnedBy.Organization)
            return ResponseBase.ErrorResponse("Cannot add collaborators to organization repositories.");

        var userResponse = await userService.GetUserByUsernameAsync(username);
        var user = userResponse.Result as User;
        if (user == null)
        {
            return ResponseBase.ErrorResponse("User does not exist.");
        }

        if(user.Id == repo.OwnerId)
        {
            return ResponseBase.ErrorResponse("Cannot add owner as collaborator.");
        }

        var (currentUser, role) = await userContextService.GetCurrentUserWithRoleAsync();
        if (repo.OwnerId != currentUser.Id)
        {
            return ResponseBase.ErrorResponse("You are not the owner.");
        }

        var alreadyExists = repo.Collaborators.Any(c => c.Id == user.Id);
        if (alreadyExists)
        {
            return ResponseBase.ErrorResponse("User already added.");
        }

        repo.Collaborators.Add(user);
        await repositoryManager.RepositoryRepository.UpdateAsync(repo);
        var harborProjectMember = new HarborProjectMember
        {
            MemberUser = new HarborProjectMemberUser
            {
                UserId = user.HarborUserId,
                Username = user.UserName
            },
            RoleId = (int)HarborRoles.Developer
        };
        await harborService.addMember($"{currentUser.UserName}-{repo.Name}", harborProjectMember);

        return ResponseBase.SuccessResponse();
    }

    public async Task<ResponseBase> GetCollaborators(int repositoryId)
    {
        var repo = await repositoryManager.RepositoryRepository.GetByIdWithCollaboratorsAsync(repositoryId);
        if (repo == null)
        {
            return ResponseBase.ErrorResponse("Repository not found.");
        }

        var (currentUser, currentUserRole) = await userContextService.GetCurrentUserWithRoleAsync();
        if (currentUser == null)
        {
            return ResponseBase.ErrorResponse("User does not exist.");
        }

        if (repo.OwnedBy == RepositoryOwnedBy.Organization)
        {
            var teams = await repositoryManager.TeamRepository
            .GetAllByRepositoryIdAsync(repositoryId);

            if (!await isCurrentUserCollaborator<Team>(currentUser.Id, currentUserRole, RepositoryOwnedBy.Organization, repo.OwnerId, teams))
            {
                return ResponseBase.SuccessResponse(null);
            }

            var teamDtos = teams
            .Select(mapperManager.TeamDtoToTeamMapper.ReverseMap)
            .ToList();

            return ResponseBase.SuccessResponse(teamDtos);
        }

        if (!await isCurrentUserCollaborator<User>(currentUser.Id, currentUserRole, RepositoryOwnedBy.User, repo.OwnerId , repo.Collaborators))
        {
            return ResponseBase.SuccessResponse(null);
        }

        var collaborators = repo.Collaborators.Select(mapperManager.UserToUserDtoMapper.Map).ToList();
        return ResponseBase.SuccessResponse(collaborators);
    }

    public async Task<ResponseBase> GetRepositoriesUserContributed(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
        {
            return ResponseBase.ErrorResponse("User does not exist!");
        }

        var (loggedInUser, loggedInUserRole) = await userContextService.GetCurrentUserWithRoleAsync();
        var showOnlyPublic = loggedInUserRole == "USER" && user?.Id != loggedInUser.Id;
        var contributedRepositories = await repositoryManager.RepositoryRepository.GetContributedByUserIdAsync(user.Id, showOnlyPublic);
        var repositoryDtos = contributedRepositories.Select(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap).ToList();

        var updatedDtos = await GetFullNames(repositoryDtos);

        return ResponseBase.SuccessResponse(updatedDtos);
    }

    private async Task<(bool IsAllowed, Model.Entity.Repository? Repository)> IsEditAllowed(int repositoryId)
    {
        var (user, role) = await userContextService.GetCurrentUserWithRoleAsync();
        var repository = await GetRepositoryForCurrentUser(repositoryId);

        var isAllowed = repository != null && (await GetAdminUsersInRepository(repository))
            .Any(el => el.UserName == user.UserName);

        return (isAllowed, repository);
    }

    private async Task<RepositorySearchDto> ParseQuery(string? query)
    {
        var filters = new RepositorySearchDto();

        if (string.IsNullOrWhiteSpace(query))
            return filters;

        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var term in terms)
        {
            if (term.StartsWith("name:", StringComparison.OrdinalIgnoreCase))
            {
                filters.Name = term[5..];
            }
            else if (term.StartsWith("description:", StringComparison.OrdinalIgnoreCase))
            {
                filters.Description = term[12..];
            }
            else if (term.StartsWith("badge:", StringComparison.OrdinalIgnoreCase))
            {
                var value = term[6..];
                if (Enum.TryParse<Badge>(value, true, out var parsedBadge))
                {
                    filters.Badge = parsedBadge;
                }
                else
                {
                    var match = Enum.GetValues<Badge>()
                        .FirstOrDefault(b => b.ToString()
                            .Contains(value, StringComparison.OrdinalIgnoreCase));
                    if (match != Badge.None)
                        filters.Badge = match;
                }
            }
            else if (term.StartsWith("author:", StringComparison.OrdinalIgnoreCase))
            {
                filters.AuthorName = term[7..];
                
                var users = await repositoryManager.UserRepository.GetUsersByUsernameContaining(filters.AuthorName);
                foreach (var user in users)
                {
                    filters.AuthorUserIds.Add(user.Id);
                }
                
                var organizations = await repositoryManager.OrganizationRepository.GetOrganizationsByNameContaining(filters.AuthorName);
                foreach (var org in organizations)
                {
                    filters.AuthorOrganizationIds.Add(org.Id);
                }
            }
            else
            {
                filters.GeneralTerms.Add(term);
            }
        }

        return filters;
    }


    private async Task<RepositoryDto> MapModelToDto(Model.Entity.Repository repository)
    {
        var repositoryDto = mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap(repository);
        repositoryDto.FullName = await GetFullName(repositoryDto);
        repositoryDto.OwnerUsername = await GetRepoOwnerUsername(repositoryDto.OwnedBy, repositoryDto.OwnerId) ?? "";

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
        var (projectName, repoName) = await GetFullProjectRepositoryName(repositoryDto.Id ?? -1);
        return $"{projectName}/{repoName}";
    }

    private async Task<string?> GetRepoOwnerUsername(RepositoryOwnedBy ownedBy, int ownerId)
    {
        string? fullName;
        switch (ownedBy)
        {
            case RepositoryOwnedBy.Organization:
                fullName = (await repositoryManager.OrganizationRepository.GetById(ownerId))?.Name;
                break;
            case RepositoryOwnedBy.User:
            case RepositoryOwnedBy.Admin:
            case RepositoryOwnedBy.SuperAdmin:
                fullName = (await repositoryManager.UserRepository.GetByIdAsync(ownerId))?.UserName;
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

    private async Task<bool> CanCurrentUserAccessRepository(Model.Entity.Repository repository)
    {
        try
        {
            if (!repository.IsPrivate)
                return true;

            var (currentUser, role) = await userContextService.GetCurrentUserWithRoleAsync();
            if (currentUser == null)
                return false;

            if (role is "SUPERADMIN" or "ADMIN")
                return true;

            if (repository.OwnedBy == RepositoryOwnedBy.User
            && (repository.OwnerId == currentUser.Id || repository.Collaborators.Any(c => c.Id == currentUser.Id)))
                return true;

            if (repository.OwnedBy == RepositoryOwnedBy.Organization &&
                await GetOrganizationUserAsync(repository.OwnerId, currentUser.Id) != null)
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<User?> GetOrganizationUserAsync(int organizationId, int userId)
    {
        var organization = await repositoryManager.OrganizationRepository.GetById(organizationId);
        if (organization == null)
            return null;

        return organization.OwnerId == userId 
            ? organization.Owner 
            : organization.Users.FirstOrDefault(u => u.Id == userId);
    }

    private async Task<string> GetHarborProjectName(Model.Entity.Repository repository)
    {
        return await GetRepoOwnerUsername(repository.OwnedBy, repository.OwnerId) + "-" + repository.Name;
    }
    
    private async Task RemoveCacheRepositoriesByOrganization(Model.Entity.Repository repository)
    {
        if (repository.OwnedBy == RepositoryOwnedBy.Organization)
        {
            var organization = await repositoryManager.OrganizationRepository.GetById(repository.OwnerId);
            if (organization != null)
            {
                await _cacheService.RemoveCacheValueAsync($"RepositoriesByOrganization:{organization.Name}");
            }
        }
    }

    private async Task<bool> isCurrentUserCollaborator<T>(int userId, string userRole, RepositoryOwnedBy ownedBy, int ownerId, List<T> items)
    {
        if (userRole is "SUPERADMIN" or "ADMIN") return true;

        if (ownedBy == RepositoryOwnedBy.Organization)
        {
            var teams = items as List<Team>;
            var ownerOrganization = await repositoryManager.OrganizationRepository.GetByIdAsync(ownerId);

            if (ownerOrganization == null)
                return false;

            return userId == ownerOrganization.OwnerId || teams.Any(t => t.Users.Any(u => u.Id == userId));
        }

        var collaborators = items as List<User>;
        return ownerId == userId || collaborators.Any(c => c.Id == userId);
    }

    public async Task<ResponseBase> Search(string? query)
    {
        var (user, role) = await userContextService.GetCurrentUserWithRoleAsync();
        var repositories = role == "USER" ?
            await repositoryManager.RepositoryRepository.SearchByOwnerId(query, user.Id) :
            await repositoryManager.RepositoryRepository.SearchAllAsync(query);

        var repositoryDtos = repositories
            .Select(mapperManager.RepositoryDtoToRepositoryMapper.ReverseMap)
            .ToList();
        var updatedDtos = await GetFullNames(repositoryDtos);

        return ResponseBase.SuccessResponse(updatedDtos);
    }
}
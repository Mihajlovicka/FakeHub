using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Service.Implementation;

public class RepositoryService(
    IBaseMapper<RepositoryDto, Model.Entity.Repository> repositoryMapper,
    IOrganizationService organizationService,
    IRepositoryManager repositoryManager,
    IUserContextService userContextService,
    IUserService userService
) : IRepositoryService
{
    public async Task<ResponseBase> Save(RepositoryDto repositoryDto)
    {
        var repository = repositoryMapper.Map(repositoryDto);
        var (currentUser, currentUserRole) = await userContextService.GetCurrentUserWithRoleAsync();

        if(currentUserRole == "ADMIN" || currentUserRole == "SUPERADMIN")
        {
            repository.Badge = Badge.DockerOfficialImage;
            repository.OwnerId = currentUser.Id;
            repository.OwnedBy = currentUserRole == "ADMIN" ? RepositoryOwnedBy.Admin : RepositoryOwnedBy.SuperAdmin;
        } 
        else if (repositoryDto.OwnerId == -1)
        {
            repository.OwnerId = currentUser.Id;
        }

        var (success, errorMessage) = await ValidateRepository(repository);
        if (!success) return ResponseBase.ErrorResponse(errorMessage);
        else
        {
            await repositoryManager.RepositoryRepository.AddAsync(repository);
        }

        return ResponseBase.SuccessResponse(repositoryMapper.ReverseMap(repository));
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

        var repositoryDtos = repositories.Select(repositoryMapper.ReverseMap).ToList();

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

        bool showOnlyPublic = role == "USER" && user?.Id != loggedInUser.Id;

        var filteredRepositories = await repositoryManager.RepositoryRepository.GetUserRepositoriesByOwnerId(user.Id, showOnlyPublic);

        var repositoryDtos = filteredRepositories.Select(repositoryMapper.ReverseMap).ToList();

        var updatedDtos = await GetFullNames(repositoryDtos);

        return ResponseBase.SuccessResponse(updatedDtos);
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
            switch(repositoryDto.OwnedBy)
            {
                case RepositoryOwnedBy.Organization:
                    repositoryDto.FullName = $"{(await repositoryManager.OrganizationRepository.GetByIdAsync(repositoryDto.OwnerId))?.Name ?? "UnknownOrg"}/{repositoryDto.Name}";
                    break;
                case RepositoryOwnedBy.User:
                    repositoryDto.FullName = $"{(await repositoryManager.UserRepository.GetByIdAsync(repositoryDto.OwnerId))?.UserName ?? "UnknownUser"}/{repositoryDto.Name}";
                    break;
                default:
                    repositoryDto.FullName = repositoryDto.Name;
                    break;
            }

            updatedDtos.Add(repositoryDto);
        }

        return updatedDtos;
    }
}
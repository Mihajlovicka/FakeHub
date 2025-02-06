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
    IUserContextService userContextService
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
}
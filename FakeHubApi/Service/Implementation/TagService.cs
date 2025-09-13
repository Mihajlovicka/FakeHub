using FakeHubApi.ContainerRegistry;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;

namespace FakeHubApi.Service.Implementation;

public class TagService(
    IRepositoryManager repositoryManager,
    IUserContextService userContextService,
    IRepositoryService repositoryService,
    IHarborService harborService
) : ITagService
{
    public async Task<ResponseBase> CanDelete(int repositoryId)
    {
        var (user, role) = await userContextService.GetCurrentUserWithRoleAsync();
        var repository = await repositoryManager.RepositoryRepository.GetByIdAsync(repositoryId);

        var isAllowed = repository != null && (await GetUsersInRepositoryWhoCanDeleteTags(repository))
            .Any(el => el.UserName == user.UserName);
        return ResponseBase.SuccessResponse(isAllowed);
    }

    public async Task<ResponseBase> DeleteTag(ArtifactDto artifact, int repositoryId)
    {
        var justReadWriteAccess = await JustReadWriteAccess(await repositoryManager.RepositoryRepository.GetByIdAsync(repositoryId));
        var (projectName, repositoryName) = await repositoryService.GetFullProjectRepositoryName(repositoryId);

        var harborResponse = await harborService.deleteTag(projectName, repositoryName, artifact.Digest, artifact.Tag.Name, justReadWriteAccess);

        List<HarborArtifact> artifacts = await harborService.GetTags(projectName, repositoryName);
        var artifactsDtos = artifacts.SelectMany(repositoryService.MapHarborArtifactToArtifactDto).ToList();
        return ResponseBase.SuccessResponse(artifactsDtos);
    }

    private async Task<List<User>> GetUsersInRepositoryWhoCanDeleteTags(Model.Entity.Repository repository)
    {
        var users = new List<User>();
        if (repository.OwnedBy == RepositoryOwnedBy.Organization)
        {
            var organization = await repositoryManager.OrganizationRepository.GetById(repository.OwnerId);
            users.Add(organization!.Owner);
            var firstAdminTeam = organization.Teams.FirstOrDefault(el => (el.TeamRole == TeamRole.Admin || el.TeamRole == TeamRole.ReadWrite) && el.RepositoryId == repository.Id);
            if (firstAdminTeam != null && firstAdminTeam.Users.Count > 0)
            {
                users.AddRange(firstAdminTeam.Users);
            }
        }
        else
        {
            var user = await repositoryManager.UserRepository.GetByIdAsync(repository.OwnerId);
            if (user != null) users.Add(user);
            //TODO colaboratori
        }
        return users;
    }

    private async Task<bool> JustReadWriteAccess(Model.Entity.Repository repository)
    {
        var (user, role) = await userContextService.GetCurrentUserWithRoleAsync();
        if (repository.OwnedBy == RepositoryOwnedBy.Organization)
        {
            var organization = await repositoryManager.OrganizationRepository.GetById(repository.OwnerId);
            var firstAdminTeam = organization.Teams.FirstOrDefault(el => (el.TeamRole == TeamRole.ReadWrite) && el.RepositoryId == repository.Id);
            if (firstAdminTeam != null && firstAdminTeam.Users.Count > 0)
            {
                return firstAdminTeam.Users.Contains(user);
            }
        }
        else
        {
            //TODO colaboratori
        }
        return false;
    }
}
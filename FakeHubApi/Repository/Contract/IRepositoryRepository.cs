using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IRepositoryRepository : ICrudRepository<Model.Entity.Repository>
{
    Task<Model.Entity.Repository?> GetByOwnerAndName(RepositoryOwnedBy ownedBy, int ownerId, string name);
    Task<IEnumerable<Model.Entity.Repository>> GetUserRepositoriesByOwnerId(int ownerId, bool onlyPublic = false);
    Task<IEnumerable<Model.Entity.Repository>> GetOrganizationRepositoriesByOrganizationId(int orgId);
    Task<IEnumerable<Model.Entity.Repository>> SearchByOwnerId(string? query, int userId);
    Task<IEnumerable<Model.Entity.Repository>> SearchAllAsync(string? query);
    Task<IEnumerable<Model.Entity.Repository>> GetAllPublicRepositories(RepositorySearchDto filters);
    Task<Model.Entity.Repository?> GetByIdWithCollaboratorsAsync(int repositoryId);
    Task<IEnumerable<Model.Entity.Repository>> GetContributedByUserIdAsync(int userId, bool onlyPublic = false);
}
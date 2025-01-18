using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IRepositoryRepository : ICrudRepository<Model.Entity.Repository>
{
    Task<Model.Entity.Repository?> GetByOwnerAndName(RepositoryOwnedBy ownedBy, int ownerId, string name);
}
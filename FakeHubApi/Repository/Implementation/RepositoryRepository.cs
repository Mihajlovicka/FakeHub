using FakeHubApi.Data;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class RepositoryRepository(AppDbContext context): CrudRepository<Model.Entity.Repository>(context), IRepositoryRepository
{
    public async Task<Model.Entity.Repository?> GetByOwnerAndName(RepositoryOwnedBy ownedBy, int ownerId, string name)
    {
        return await _context.Repositories.FirstOrDefaultAsync(x =>
            x.Name.Equals(name) && x.OwnerId == ownerId && x.OwnedBy.Equals(ownedBy));
    }
}
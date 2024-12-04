using FakeHubApi.Data;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace FakeHubApi.Repository.Implementation;

public class OrganizationRepository(AppDbContext context)
    : CrudRepository<Organization>(context),
        IOrganizationRepository
{
    public Task<Organization?> GetByName(string name) =>
        _context.Organizations.FirstOrDefaultAsync(x => x.Name == name);
}

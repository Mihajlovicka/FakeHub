using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IOrganizationRepository : ICrudRepository<Organization>
{
    Task<Organization?> GetByName(string name);
    Task<List<Organization>> GetByUser(int userId);
    Task<List<Organization>> Search(string? query, int userId);
    Task<Organization?> GetById(int id);
    Task<List<Organization>> GetOrganizationsByNameContaining(string name);
}

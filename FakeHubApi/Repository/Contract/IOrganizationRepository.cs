using FakeHubApi.Model.Entity;

namespace FakeHubApi.Repository.Contract;

public interface IOrganizationRepository : ICrudRepository<Organization>
{
    Task<Organization?> GetByName(string name);
}

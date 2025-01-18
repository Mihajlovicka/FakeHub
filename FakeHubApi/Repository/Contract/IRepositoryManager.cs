namespace FakeHubApi.Repository.Contract;

public interface IRepositoryManager
{
    public IUserRepository UserRepository { get; }
    public IOrganizationRepository OrganizationRepository { get; }
    public ITeamRepository TeamRepository { get; }
    public IRepositoryRepository RepositoryRepository { get; }
}

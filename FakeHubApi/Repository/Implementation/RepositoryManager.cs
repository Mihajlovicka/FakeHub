using FakeHubApi.Repository.Contract;

namespace FakeHubApi.Repository.Implementation;

public class RepositoryManager(
    IUserRepository userRepository,
    IOrganizationRepository organizationRepository,
    ITeamRepository teamRepository,
    IRepositoryRepository repositoryRepository
) : IRepositoryManager
{
    public IUserRepository UserRepository { get; } = userRepository;
    public IOrganizationRepository OrganizationRepository { get; } = organizationRepository;
    public ITeamRepository TeamRepository { get; } = teamRepository;
    public IRepositoryRepository RepositoryRepository { get; } = repositoryRepository;
}

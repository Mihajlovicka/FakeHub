using FakeHubApi.Repository.Contract;

namespace FakeHubApi.Repository.Implementation;

public class RepositoryManager(
    IUserRepository userRepository,
    IOrganizationRepository organizationRepository
) : IRepositoryManager
{
    public IUserRepository UserRepository { get; } = userRepository;
    public IOrganizationRepository OrganizationRepository { get; } = organizationRepository;
}

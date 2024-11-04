using FakeHubApi.Repository.Contract;

namespace FakeHubApi.Repository.Implementation;

public class RepositoryManager(IUserRepository userRepository) : IRepositoryManager
{
    public IUserRepository UserRepository {get;} = userRepository;
}
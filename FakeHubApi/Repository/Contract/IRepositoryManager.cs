namespace FakeHubApi.Repository.Contract;

public interface IRepositoryManager
{
    public IUserRepository UserRepository { get; }
}
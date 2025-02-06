using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
namespace FakeHubApi.Mapper.RepositoryMapper;

public class RepositoryDtoToRepositoryMapper() : BaseMapper<RepositoryDto, Model.Entity.Repository>
{
    public override Model.Entity.Repository Map(RepositoryDto source)
    {
        var repository = new Model.Entity.Repository()
        {
            Name = source.Name,
            Description = source.Description,
            IsPrivate = source.IsPrivate,
            OwnedBy = source.OwnedBy,
            OwnerId = source.OwnerId,
            Badge = source.Badge
        };

        if (source.Id != null) repository.Id = (int)source.Id;

        return repository;
    }
    
    public override RepositoryDto ReverseMap(Model.Entity.Repository destination)
    {
        return new RepositoryDto()
        {
            Id = destination.Id,
            Name = destination.Name,
            Description = destination.Description,
            IsPrivate = destination.IsPrivate,
            OwnedBy = destination.OwnedBy,
            OwnerId = destination.OwnerId,
            Badge = destination.Badge
        };
    }
}
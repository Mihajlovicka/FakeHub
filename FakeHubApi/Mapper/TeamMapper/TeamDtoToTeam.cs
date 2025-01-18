using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper.TeamMapper;

public class TeamDtoToTeam(IBaseMapper<RepositoryDto, Model.Entity.Repository> repositoryMapper, IBaseMapper<User, UserDto> userMapper)
 : BaseMapper<TeamDto, Team>
{
    public override Team Map(TeamDto source)
    {
        return new()
        {
            Name = source.Name,
            Description = source.Description,
            TeamRole = Enum.Parse<TeamRole>(source.TeamRole)
        };
    }

    public override TeamDto ReverseMap(Team source)
    {
        return new()
        {
            Name = source.Name,
            Description = source.Description,
            CreatedAt = source.CreatedAt,
            TeamRole = source.TeamRole.ToString(),
            Repository = repositoryMapper.ReverseMap(source.Repository),
            Users =  source.Users.Select(userMapper.Map).ToList()
        };
    }
}

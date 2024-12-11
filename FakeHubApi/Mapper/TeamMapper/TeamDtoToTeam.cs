using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper.TeamMapper;

public class TeamDtoToTeam : BaseMapper<TeamDto, Team>
{
    public override Team Map(TeamDto source)
    {
        return new() { Name = source.Name, Description = source.Description };
    }
}

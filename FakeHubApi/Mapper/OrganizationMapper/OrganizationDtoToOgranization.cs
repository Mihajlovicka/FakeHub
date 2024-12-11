using FakeHubApi.Mapper.TeamMapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper.OrganizationMapper;

public class OrganizationDtoToOgranization(IBaseMapper<TeamDto, Team> teamDtoToTeamMapper)
    : BaseMapper<OrganizationDto, Organization>
{
    public override Organization Map(OrganizationDto source)
    {
        return new()
        {
            Name = source.Name,
            Description = source.Description,
            ImageBase64 = source.ImageBase64,
        };
    }

    public override OrganizationDto ReverseMap(Organization source)
    {
        return new()
        {
            Name = source.Name,
            Description = source.Description,
            ImageBase64 = source.ImageBase64,
            Owner = source.Owner?.UserName,
            Teams = source.Teams.Select(teamDtoToTeamMapper.ReverseMap).ToList(),
        };
    }
}

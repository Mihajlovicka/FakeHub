using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper.OrganizationMapper;

public class OrganizationDtoToOgranization : BaseMapper<OrganizationDto, Organization>
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
        };
    }
}

using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper;

public class MapperManager(
    IBaseMapper<RegistrationRequestDto, User> registrationsRequestDtoToApplicationUserMapper,
    IBaseMapper<User, UserProfileResponseDto> applicationUserToUserProfileResponseDto,
    IBaseMapper<OrganizationDto, Organization> organizationDtoToOrganizationMapper,
    IBaseMapper<TeamDto, Team> teamDtoToTeamMapper
) : IMapperManager
{
    public IBaseMapper<
        RegistrationRequestDto,
        User
    > RegistrationsRequestDtoToApplicationUserMapper { get; } =
        registrationsRequestDtoToApplicationUserMapper;

    public IBaseMapper<
        User,
        UserProfileResponseDto
    > ApplicationUserToUserProfileResponseDto { get; } = applicationUserToUserProfileResponseDto;

    public IBaseMapper<OrganizationDto, Organization> OrganizationDtoToOrganizationMapper { get; } =
        organizationDtoToOrganizationMapper;

    public IBaseMapper<TeamDto, Team> TeamDtoToTeamMapper { get; } = teamDtoToTeamMapper;
}

using FakeHubApi.Mapper.UserMapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper;

public interface IMapperManager
{
    IBaseMapper<
        RegistrationRequestDto,
        User
    > RegistrationsRequestDtoToApplicationUserMapper { get; }

    IBaseMapper<User, UserProfileResponseDto> ApplicationUserToUserProfileResponseDto { get; }

    IBaseMapper<OrganizationDto, Organization> OrganizationDtoToOrganizationMapper { get; }

    IBaseMapper<TeamDto, Team> TeamDtoToTeamMapper { get; }
}

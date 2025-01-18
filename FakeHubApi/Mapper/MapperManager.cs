using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper;

public class MapperManager(
    IBaseMapper<RegistrationRequestDto, User> registrationsRequestDtoToUserMapper,
    IBaseMapper<User, UserDto> userToUserDto,
    IBaseMapper<OrganizationDto, Organization> organizationDtoToOrganizationMapper,
    IBaseMapper<TeamDto, Team> teamDtoToTeamMapper,
    IBaseMapper<RepositoryDto, Model.Entity.Repository> repositoryDtoToRepositoryMapper
        ) : IMapperManager
{
    public IBaseMapper<
        RegistrationRequestDto,
        User
    > RegistrationsRequestDtoToUserMapper { get; } =
        registrationsRequestDtoToUserMapper;

    public IBaseMapper<
        User,
        UserDto
    > UserToUserDtoMapper { get; } = userToUserDto;

    public IBaseMapper<OrganizationDto, Organization> OrganizationDtoToOrganizationMapper { get; } =
        organizationDtoToOrganizationMapper;

    public IBaseMapper<TeamDto, Team> TeamDtoToTeamMapper { get; } = teamDtoToTeamMapper;

    public IBaseMapper<RepositoryDto, Model.Entity.Repository> RepositoryDtoToRepositoryMapper { get; set; } =
        repositoryDtoToRepositoryMapper;
}

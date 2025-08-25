using FakeHubApi.ContainerRegistry;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper;

public interface IMapperManager
{
    IBaseMapper<
        RegistrationRequestDto,
        User
    > RegistrationsRequestDtoToUserMapper
    { get; }

    IBaseMapper<User, UserDto> UserToUserDtoMapper { get; }

    IBaseMapper<OrganizationDto, Organization> OrganizationDtoToOrganizationMapper { get; }

    IBaseMapper<TeamDto, Team> TeamDtoToTeamMapper { get; }

    IBaseMapper<RepositoryDto, Model.Entity.Repository> RepositoryDtoToRepositoryMapper { get; set; }

    IBaseMapper<HarborArtifact, ArtifactDto> HarborArtifactToArtifactDtoMapper { get; set; }
}

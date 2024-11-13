using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper;

public class MapperManager(
    IBaseMapper<RegistrationRequestDto, User> registrationsRequestDtoToApplicationUserMapper
) : IMapperManager
{
    public IBaseMapper<
        RegistrationRequestDto,
        User
    > RegistrationsRequestDtoToApplicationUserMapper { get; } =
        registrationsRequestDtoToApplicationUserMapper;
}

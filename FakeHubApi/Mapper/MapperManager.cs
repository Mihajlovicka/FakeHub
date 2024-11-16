using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper;

public class MapperManager(
    IBaseMapper<RegistrationRequestDto, User> registrationsRequestDtoToApplicationUserMapper,
    IBaseMapper<User,UserProfileResponseDto> applicationUserToUserProfileResponseDto
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
    > ApplicationUserToUserProfileResponseDto
    { get; } =
        applicationUserToUserProfileResponseDto;
}

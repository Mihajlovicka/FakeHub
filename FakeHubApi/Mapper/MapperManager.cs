using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper;

public class MapperManager(
    IBaseMapper<RegistrationRequestDto, ApplicationUser> registrationsRequestDtoToApplicationUserMapper) : IMapperManager
{
    public IBaseMapper<RegistrationRequestDto, ApplicationUser> RegistrationsRequestDtoToApplicationUserMapper { get; } = registrationsRequestDtoToApplicationUserMapper;
}
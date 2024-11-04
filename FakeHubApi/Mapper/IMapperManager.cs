using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper;

public interface IMapperManager
{
    IBaseMapper<RegistrationRequestDto, ApplicationUser> RegistrationsRequestDtoToApplicationUserMapper { get; }
}
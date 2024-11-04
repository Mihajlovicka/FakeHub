
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace AuthService.Mapper.UserMapper;

public class RegistrationsRequestDtoToApplicationUser: BaseMapper<RegistrationRequestDto, ApplicationUser>
{
    public override ApplicationUser Map(RegistrationRequestDto source)
    {
        return new()
        {
            UserName = source.Username,
            Email = source.Email,
        };
    }
}
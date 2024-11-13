using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper.UserMapper;

public class RegistrationsRequestDtoToApplicationUser : BaseMapper<RegistrationRequestDto, User>
{
    public override User Map(RegistrationRequestDto source)
    {
        return new() { UserName = source.Username, Email = source.Email };
    }
}

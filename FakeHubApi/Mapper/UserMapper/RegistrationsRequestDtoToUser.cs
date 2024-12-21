using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper.UserMapper;

public class RegistrationsRequestDtoToUser : BaseMapper<RegistrationRequestDto, User>
{
    public override User Map(RegistrationRequestDto source)
    {
        return new User() { UserName = source.Username, Email = source.Email };
    }
}

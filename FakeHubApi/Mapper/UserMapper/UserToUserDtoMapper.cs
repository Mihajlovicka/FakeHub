using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper.UserMapper;

public class UserToUserDto : BaseMapper<User, UserDto>
{
    public override UserDto Map(User source)
    {
        return new UserDto()
        {
            Username = source?.UserName,
            Email = source?.Email,
            CreatedAt = source?.CreatedAt,
            Badge = source.Badge
        };
    }
}
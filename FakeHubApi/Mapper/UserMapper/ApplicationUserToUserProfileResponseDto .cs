using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Mapper.UserMapper;

public class ApplicationUserToUserProfileResponseDto : BaseMapper<User, UserProfileResponseDto>
{
    public override UserProfileResponseDto Map(User source)
    {
        return new()
        {
            Username = source?.UserName,
            Email = source?.Email,
            CreatedAt = source?.CreatedAt
        };
    }
}
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Mapper.UserMapper
{
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
}

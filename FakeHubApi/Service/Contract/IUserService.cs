using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface IUserService
{
    Task<ResponseBase> GetUserProfileByUsernameAsync(string username);
    Task<ResponseBase> ChangePassword(ChangePasswordRequestDto changePasswordRequestDto);
    Task<ResponseBase> ChangeEmailAsync(ChangeEmailRequestDto changeEmailRequestDto);
    Task<ResponseBase> ChangeUserBadgeAsync(ChangeUserBadgeRequestDto changeUserBadgeRequestDto);
    Task<ResponseBase> GetUsersByQuery(string query, Role role);
}
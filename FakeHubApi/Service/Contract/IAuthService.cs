using FakeHubApi.Model;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Service.Contract;

public interface IAuthService
{
    Task<ResponseBase> Register(RegistrationRequestDto registrationRequestDto, string role);
    Task<ResponseBase> Login(LoginRequestDto loginRequestDto);
    Task<ResponseBase> GetUserProfileByUsernameAsync(string username);
    Task<ResponseBase> ChangePassword(ChangePasswordRequestDto changePasswordRequestDto);
    Task<ResponseBase> ChangeEmailAsync(ChangeEmailRequestDto changeEmailRequestDto);
    Task<ResponseBase> ChangeUserBadgeAsync(ChangeUserBadgeRequestDto changeUserBadgeRequestDto);
}

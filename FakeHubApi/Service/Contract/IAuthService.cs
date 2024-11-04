using FakeHubApi.Model;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface IAuthService
{
    Task<ResponseBase> Register(RegistrationRequestDto registrationRequestDto);
    Task<ResponseBase> Login(LoginRequestDto loginRequestDto);
}
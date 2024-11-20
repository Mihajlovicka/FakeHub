using FakeHubApi.Model;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Service.Contract;

public interface IAuthService
{
    Task<ResponseBase> Register(RegistrationRequestDto registrationRequestDto, string role="USER"); //zameniti konstantom
    Task<ResponseBase> Login(LoginRequestDto loginRequestDto);
}
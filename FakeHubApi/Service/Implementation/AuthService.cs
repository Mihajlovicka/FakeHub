using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Service.Implementation;

public class AuthService(
    IRepositoryManager _repository,
    UserManager<ApplicationUser> _userManager,
    IMapperManager _mapperManager
) : IAuthService
{

    public async Task<ResponseBase> Register(RegistrationRequestDto registrationRequestDto)
    {
        var response = new ResponseBase();
        var user = _mapperManager.RegistrationsRequestDtoToApplicationUserMapper.Map(
            registrationRequestDto
        );
        try
        {
            var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
            if (!result.Succeeded)
            {
                return CreateErrorResponse(
                    result.Errors.FirstOrDefault()?.Description ?? "Registration failed"
                );
            }
        }
        catch
        {
            return CreateErrorResponse("An error occurred during user creation");
        }

        var createdUser = await _repository.UserRepository.GetByUsername(
            registrationRequestDto.Username
        );
        if (createdUser == null)
        {
            return CreateErrorResponse("User creation failed");
        }

        await _userManager.AddToRoleAsync(user, registrationRequestDto.Role);
        return response;
    }

    private ResponseBase CreateErrorResponse(string errorMessage)
    {
        return new ResponseBase { Success = false, ErrorMessage = errorMessage };
    }
}

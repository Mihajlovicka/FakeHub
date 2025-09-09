using FakeHubApi.ContainerRegistry;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Implementation;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Service.Implementation;

public class AuthService(
    UserManager<User> userManager,
    IMapperManager mapperManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IHarborService harborService,
    IHarborTokenService harborTokenService
) : IAuthService
{
    public async Task<ResponseBase> Login(LoginRequestDto loginRequestDto)
    {
        var user = await userManager.FindByEmailAsync(loginRequestDto.Email);
        if (user == null)
        {
            return ResponseBase.ErrorResponse("User not found");
        }

        if (!await userManager.CheckPasswordAsync(user, loginRequestDto.Password))
            return ResponseBase.ErrorResponse("Email or password is incorrect");

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenGenerator.GenerateToken(user, roles);

        await harborTokenService.GenerateAndStoreHarborToken(user.Id.ToString(), user.Email, loginRequestDto.Password);


        return ResponseBase.SuccessResponse(new LoginResponseDto
        {
            Token = token,
        });
    }

    public async Task<ResponseBase> Register(RegistrationRequestDto registrationRequestDto, string role)
    {
        var user = mapperManager.RegistrationsRequestDtoToUserMapper.Map(
            registrationRequestDto
        );
        user.EmailConfirmed = true;
        user.TwoFactorEnabled = true;
        try
        {
            var result = await userManager.CreateAsync(user, registrationRequestDto.Password);
            if (!result.Succeeded)
            {
                return ResponseBase.ErrorResponse(
                    result.Errors.FirstOrDefault()?.Description ?? "Registration failed"
                );
            }
        }
        catch
        {
            return ResponseBase.ErrorResponse("An error occurred during user creation");
        }

        var createdUser = await userManager.FindByEmailAsync(registrationRequestDto.Email);
        if (createdUser == null)
        {
            return ResponseBase.ErrorResponse("User creation failed");
        }

        await userManager.AddToRoleAsync(user, role);

        var resp = await harborService.createUser(new HarborUser
            {
                Username = user.UserName,
                Email = user.Email,
                Password = registrationRequestDto.Password,
                Realname = user.UserName,
            });

        if (resp)
        {
            var userId = await harborService.getUserId(user.UserName);
            if (userId != null)
            {
                user.HarborUserId = userId.Value;
                await userManager.UpdateAsync(user);
            }
        }
        
        return ResponseBase.SuccessResponse();
    }
}

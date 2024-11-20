using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Service.Implementation;

public class AuthService(
    UserManager<User> userManager,
    IMapperManager mapperManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IUserContextService userContextService
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
            throw new BadHttpRequestException("Email or password is incorrect");

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenGenerator.GenerateToken(user, roles);
        return ResponseBase.SuccessResponse(new LoginResponseDto { Token = token });
    }

    public async Task<ResponseBase> Register(RegistrationRequestDto registrationRequestDto, string role)
    {
        var user = mapperManager.RegistrationsRequestDtoToApplicationUserMapper.Map(
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
        return ResponseBase.SuccessResponse();
    }
    
    public async Task<ResponseBase> GetUserProfileByUsernameAsync(string username)
    {
        try
        {
            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                return ResponseBase.ErrorResponse("User not found");
            }

            var responseUser = mapperManager.ApplicationUserToUserProfileResponseDto.Map(
                user
            );

            return ResponseBase.SuccessResponse(responseUser);
        }
        catch (Exception ex)
        {
            return ResponseBase.ErrorResponse(ex.Message);
        }

    }
    public async Task<ResponseBase> ChangePassword(ChangePasswordRequestDto changePasswordRequestDto)
    {
        if (!changePasswordRequestDto.NewPassword.Equals(changePasswordRequestDto.NewPasswordConfirm))
        {
            return ResponseBase.ErrorResponse("New password and confirmation do not match");
        }

        var user = await userContextService.GetCurrentUserAsync();
        user.TwoFactorEnabled = true;

        var result = await userManager.ChangePasswordAsync(user, changePasswordRequestDto.OldPassword,
            changePasswordRequestDto.NewPassword);

        if (!result.Succeeded)
        {
            return ResponseBase.ErrorResponse(result.Errors.FirstOrDefault()?.Description ?? "Password change failed");
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ResponseBase.ErrorResponse(updateResult.Errors.FirstOrDefault()?.Description ?? 
                                              "Failed to update user settings");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenGenerator.GenerateToken(user, roles);
        return ResponseBase.SuccessResponse(new LoginResponseDto { Token = token });
    }
}

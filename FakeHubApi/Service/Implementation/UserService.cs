using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Service.Implementation;

public class UserService(
    UserManager<User> userManager,
    IMapperManager mapperManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IUserContextService userContextService,
    IRepositoryManager repositoryManager
    ) : IUserService
{
    public async Task<ResponseBase> GetUserProfileByUsernameAsync(string username)
    {
        if (string.IsNullOrEmpty(username)) return ResponseBase.ErrorResponse("Username is empty");

        try
        {
            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                return ResponseBase.ErrorResponse("User not found");
            }

            var responseUser = mapperManager.UserToUserDto.Map(
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

    public async Task<ResponseBase> ChangeEmailAsync(ChangeEmailRequestDto changeEmailRequestDto)
    {
        var user = await userContextService.GetCurrentUserAsync();
        if(user == null)
        {
            return ResponseBase.ErrorResponse("User not found in current context");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, changeEmailRequestDto.Password);
        if(!isPasswordValid)
        {
            return ResponseBase.ErrorResponse("Password is incorrect");
        }

        if(user.Email != null && user.Email.Equals(changeEmailRequestDto.NewEmail))
        {
            return ResponseBase.ErrorResponse("Email can't be the same as current");
        }

        var emailToken = await userManager.GenerateChangeEmailTokenAsync(user, changeEmailRequestDto.NewEmail);

        var changeEmailResult = await userManager.ChangeEmailAsync(user, changeEmailRequestDto.NewEmail, emailToken);
        if (!changeEmailResult.Succeeded)
        {
            return ResponseBase.ErrorResponse(changeEmailResult.Errors.FirstOrDefault()?.Description ?? "Email change failed");
        }

        var updateUserResult = await userManager.UpdateAsync(user);
        if (!updateUserResult.Succeeded)
        {
            return ResponseBase.ErrorResponse(updateUserResult.Errors.FirstOrDefault()?.Description ?? "Update current user failed");
        }

        var roles = await userManager.GetRolesAsync(user);
        var newToken = jwtTokenGenerator.GenerateToken(user, roles);
        return ResponseBase.SuccessResponse(new LoginResponseDto
        {
            Token = newToken,
        });
    }
    
    public async Task<ResponseBase> ChangeUserBadgeAsync(ChangeUserBadgeRequestDto changeUserBadgeRequestDto)
    {
        try
        {
            var user = await userManager.FindByNameAsync(changeUserBadgeRequestDto.Username);
            if (user == null)
            {
                return ResponseBase.ErrorResponse("User not found");
            }

            if (!Enum.IsDefined(typeof(Badge), changeUserBadgeRequestDto.Badge))
            {
                return ResponseBase.ErrorResponse("Invalid badge value");
            }

            user.Badge = changeUserBadgeRequestDto.Badge;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return ResponseBase.ErrorResponse("Failed to update badge");
            }

            var userProfileResponseDto = mapperManager.UserToUserDto.Map(
                user
            );

            return ResponseBase.SuccessResponse(userProfileResponseDto);

            
        }
        catch (Exception ex)
        {
            return ResponseBase.ErrorResponse(ex.Message);
        }
    }

    public async Task<ResponseBase> GetUsersByQuery(string query, Role role)
    {
        var queriesUsername = ParseUsernameQuery(query);
        var queriesEmail = ParseEmailQuery(query);
        
        var result = await repositoryManager.UserRepository.GetUsersByQueries(queriesUsername, queriesEmail, role);

        return ResponseBase.SuccessResponse(result.Select(mapperManager.UserToUserDto.Map));
    }

    private static List<string> ParseEmailQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }
        
        var invalidList = new[]
        {
            "(", ")", "<", ">", "[", "]", ",", ":", ";", "\\", "/", "|", "=", "~", "&", "!", "#", "`", "\""
        };

        query = invalidList.Aggregate(query, (current, invalidChar) => current.Replace(invalidChar, " "));

        var result = query
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        return result;
    }

    private static List<string> ParseUsernameQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }
        
        return query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .ToList();
    }
}
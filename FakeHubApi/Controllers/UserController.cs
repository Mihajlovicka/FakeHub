using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [Authorize(Roles = "SUPERADMIN")]
    [HttpGet("admins")]
    public async Task<IActionResult> GetAdminsByQuery([FromQuery] string? query)
    {
        var response = await userService.GetUsersByQueryGeneralSearch(query, Role.ADMIN);

        return Ok(response);
    }

    [Authorize(Roles = "USER,ADMIN,SUPERADMIN"), HttpGet("{username}")]
    public async Task<IActionResult> GetUserProfileByUsername(string username)
    {
        var response = await userService.GetUserProfileByUsernameAsync(username);

        if (response.Success)
        {
            return Ok(response);
        }

        return NotFound(response);
    }

    [Authorize(Roles = "USER,ADMIN,SUPERADMIN"), HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto changePasswordRequestDto
    )
    {
        var response = await userService.ChangePassword(changePasswordRequestDto);
        if (response.Success)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    [Authorize(Roles = "USER,ADMIN,SUPERADMIN"), HttpPost("change-email")]
    public async Task<IActionResult> ChangeUserEmail(ChangeEmailRequestDto changeEmailRequestDto)
    {
        var response = await userService.ChangeEmailAsync(changeEmailRequestDto);

        if (response.Success)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    [Authorize(Roles = "ADMIN,SUPERADMIN")]
    [HttpPost("change-user-badge")]
    public async Task<IActionResult> ChangeUserBadge(
        ChangeUserBadgeRequestDto changeUserBadgeRequestDto
    )
    {
        var response = await userService.ChangeUserBadgeAsync(changeUserBadgeRequestDto);

        if (response.Success)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    [Authorize(Roles = "ADMIN,SUPERADMIN,USER")]
    [HttpGet]
    public async Task<IActionResult> GetUsersByQuery([FromQuery] string? query)
    {
        var response = await userService.GetUsersByQueryGeneralSearch(query, Role.USER);

        return Ok(response);
    }
}
